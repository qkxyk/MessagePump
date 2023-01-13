//#define Debug
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Windows.Forms;
using Dapper;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
/*202008新增设备在线处理功能
 * 
 * 
 * 
 * 
 * 
 * 
 */

namespace MessagePump_Dapper
{
    public partial class FrmMain : Form
    {
        MqttClient mqtt;
        string uid;//mqtt标识
        int iReceive, iSend, iAlarm, iSendAlarm, iOther;//接收到的数据条数，已发送的历史数据，接收到的报警数据，已上传城的的报警数据，其它数据
        volatile int iInterval = 5;
        private string ConnectionString { get; }
        private string MySqlConnectionString { get; }//mysql数据库连接字符串
        volatile bool bConnectServer = false;

        FrmMessage fm;
        ILog AppLog;

        #region 处理分包数据
        private static readonly object Pagelocker = new object();
        private ConcurrentDictionary<string, CacheData> dicPage;
        Thread thCache;//定时清理过期分包数据线程

        //处理包,如果是缓存完整包返回true，否则返回false
        public bool HandleMessage(Dictionary<string, object> dic)
        {
            //解析包,保存uuid，包序号，包的类型，包大小
            int seq = dic.ContainsKey("seq") == false ? 0 : int.Parse(dic["seq"].ToString());//包的序列号，防止不同的包混到一起
            string deviceNo = dic["uuid"].ToString();//那个设备的包
            string message = dic["data"].ToString();//包要合并的内容
            int Total = int.Parse(dic["totalpage"].ToString());//包的总包数
            string key = deviceNo + seq.ToString();//存到缓存的标示
            if (dicPage.ContainsKey(key))//已添加到缓存中
            {
                HandleData hd = new HandleData();//包含要合并的内容，和包序号
                hd.Message = message;
                hd.Num = int.Parse(dic["pagenum"].ToString());
                //判断是否有重复的key
                if (dicPage[key].Data.Where(a => a.Num == hd.Num).Count() == 0)
                {
                    dicPage[key].Data.Add(hd);
                }
                //判断包是否接收完
                if (dicPage[key].Data.Count == Total)   //处理包
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else //缓存中没有
            {
                int num = int.Parse(dic["pagenum"].ToString());

                string action = dic["action"].ToString();
                HandleData hd = new HandleData();
                hd.Message = message;
                hd.Num = num;
                CacheData cd = new CacheData() { DeviceNo = deviceNo, Action = action, Total = Total, Data = new List<HandleData> { hd }, dt = DateTime.Now };
                lock (Pagelocker)
                {
                    dicPage.TryAdd(key, cd);
                }
                statusStrip1.Invoke(new MethodInvoker(() =>
                {
                    tsl.Text = $"新增一条缓存，现有{dicPage.Count}条缓存";
                }));
                if (num == Total)
                {
                    return true;
                }
                return false;
            }
        }
        //合并包
        public string HandlePackage(CacheData cd)
        {
            SData sd = new SData() { action = cd.Action, uuid = cd.DeviceNo, data = new Dictionary<string, object>() };
            foreach (var item in cd.Data.OrderBy(a => a.Num))
            {
                var t1 = JsonConvert.DeserializeObject<Dictionary<string, object>>(item.Message);
                //跳过重复的key
                foreach (var it in t1)
                {
                    if (!sd.data.ContainsKey(it.Key))
                    {
                        sd.data.Add(it.Key, it.Value);
                    }
                }
                //整体拷贝
                // sd.data = sd.data.Concat(t1).ToDictionary(k => k.Key, v => v.Value);
            }
            string message = JsonConvert.SerializeObject(sd);
            return message;
        }

        #endregion
        #region 设备在线
        private static readonly object OnlineLocker = new object();
        private ConcurrentDictionary<string, OnlineData> dicOnLine;//兼容老设备，缓存在线数据，老设备定期处理是否在线，新设备根据遗言进行处理
        private Thread thOnLine;
        //上传设备在线数据
        private void HandleDeviceOnlineData(string DeviceNo, bool isOnline, string groupId, string devicesn, string dataContent = null, string title = null)
        {
#if Debug
            return;
#else

            using (IDbConnection conn = new SqlConnection(ConnectionString))
            {
                //直接插入，数据库有约束，如果设备不存在，则不能加入
                try
                {
                    //更新设备在线时间
                    string sql = "select count(1) from deviceonline where devicesn=@devicesn";
                    var qu = conn.Query<int>(sql, new { devicesn = devicesn }).FirstOrDefault();
                    string updateOrInsert;
                    if (qu > 0)//存在设备数据
                    {
                        if (isOnline)//设备上线
                        {
                            updateOrInsert = "update deviceonline set dt=@dt,State=@State,deviceNo=@deviceNo,DataContent=@dataContent,DataTitle=@title where devicesn=@devicesn ";
                        }
                        else//设备下线
                        {
                            updateOrInsert = "update deviceonline set OffLine=@dt,State=@State,deviceNo=@deviceNo where devicesn=@devicesn ";
                        }
                        var num = conn.Execute(updateOrInsert, new { dt = DateTime.Now, State = isOnline, deviceNo = DeviceNo, dataContent = dataContent, title = title, devicesn = devicesn });
                    }
                    else//设备上线
                    {
                        if (isOnline)//设备上线时添加数据，下线则不处理
                        {
                            updateOrInsert = "insert into deviceonline (devicesn,deviceno,dt,GroupId,State,DataContent,DataTitle) values(@devicesn,@deviceno,@dt,@groupId,@State,@dataContent,@title)";
                            var m = conn.Execute(updateOrInsert, new { devicesn = devicesn, deviceno = DeviceNo, dt = DateTime.Now, groupId = groupId, State = isOnline, dataContent = dataContent, title = title });
                        }
                    }
                }
                catch (Exception ex)
                {
                    //写日志
                    AppLog.Error("处理设备上线数据失败，错误原因->" + ex.Message);
                }
            }
#endif
        }
        #endregion

        public FrmMain()
        {
            InitializeComponent();
            AppLog = LogManager.GetLogger("FrmMain");
            ConnectionString = ConfigurationManager.ConnectionStrings["SqlConString"].ConnectionString;
            MySqlConnectionString = ConfigurationManager.ConnectionStrings["mysqldb"].ConnectionString;
            dicPage = new ConcurrentDictionary<string, CacheData>();//初始化分包数据
            dicOnLine = new ConcurrentDictionary<string, OnlineData>();//初始化在线设备数据
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            tsOnline.Text = "没有在线设备";
            uid = Guid.NewGuid().ToString("N");
            InitListView();
            //测试地址
            //txtAddress.Text = "183.62.237.211:18831";
            ActConnetct = MonitorConnect;

            #region 清理多包缓存
            //定时清理缓存
            thCache = new Thread(() =>
            {
                while (true)
                {
                    if (dicPage.Count > 0)
                    {
                        foreach (var item in dicPage)
                        {
                            TimeSpan ts = DateTime.Now - item.Value.dt;
                            if (ts.TotalMinutes > 3)
                            {
                                lock (Pagelocker)
                                {
                                    CacheData cd;
                                    dicPage.TryRemove(item.Key, out cd);
                                    AppLog.Warn($"清除设备编号为{cd.DeviceNo}的缓存包，包内容为->{cd.ToString()}");
                                    this.statusStrip1.Invoke(new MethodInvoker(() =>
                                    {
                                        tsl.Text = $"清理一条缓存，还有{dicPage.Count}条缓存";
                                    })
                                   );
                                }
                            }
                        }
                    }
                    else
                    {
                        this.statusStrip1.Invoke((MethodInvoker)delegate ()
                        {
                            tsl.Text = "没有缓存数据";
                        });
                    }
                    Thread.Sleep(new TimeSpan(0, 3, 0));
                }
            });
            thCache.Start();
            #endregion

            #region 处理在线数据
            thOnLine = new Thread(() =>
            {
                while (true)
                {
                    if (dicOnLine.Count > 0)
                    {
                        lock (OnlineLocker)
                        {
                            foreach (var item in dicOnLine.Where(a => a.Value.IsWill == false))//只处理没有遗言的设备
                            {
                                TimeSpan ts = DateTime.Now - item.Value.Dt;
                                if (ts.TotalMinutes > 55)//如果55分钟没有设备上传数据，则判定改设备下线
                                {
                                    OnlineData data;
                                    dicOnLine.TryRemove(item.Key, out data);
                                    //更改设备在线状态
                                    HandleDeviceOnlineData(item.Key, false, data.GroupId, data.DeviceSn);

                                    //从设备缓存中剔除出已掉线的设备(目的是更新设备缓存列表，处理设备no和sn不一致的问题)
                                    //DicDevice.Remove(item.Key);
                                    SetOnlineData();
                                }
                            }
                        }
                    }
                    Thread.Sleep(new TimeSpan(0, 3, 0));//每隔10分钟清理一次在线数据
                }
            });
            thOnLine.Start();
            #endregion
            //启动检查mysql数据表是否存在
            CheckMysqlDataBase();

        }

        #region 系统启动检查mysql数据表是否存在，不存在创建当日的数据表
        public void CheckMysqlDataBase()
        {
            using (IDbConnection conn = new MySqlConnection(MySqlConnectionString))
            {
                //直接插入，数据库有约束，如果设备不存在，则不能加入
                string tableName = $"devhis_{DateTime.Now.ToString("yyyyMMdd")}";
                string sql = $"show tables like '{tableName}'";
                int count = conn.Query(sql).Count();
                if (count > 0)
                {
                    return;
                }
                else
                {
                    string createSql = $"create table if not exists {tableName} " +
                        $"(`id` int auto_increment,`DeviceSn` varchar(100),`Dt` datetime,`DataContent` TEXT,`DataTitle` varchar(100)," +
                        $"`groupId` VARCHAR(100),PRIMARY KEY(`id`),INDEX SingleIdx(devicesn)) ENGINE = INNODB DEFAULT CHARSET = utf8; ";
                    try
                    {
                        conn.Execute(createSql);
                    }
                    catch (Exception ex)
                    {
                        AppLog.Error("创建表失败:" + ex.Message);
                    }
                }
            }
        }
        #endregion

        private DeviceInfo GetDeviceInfo(string DeviceNo)
        {
            //DeviceInfo device = new DeviceInfo();
            using (var conn = new SqlConnection(ConnectionString))
            {
                //新版本 
                string strSql = "select devicesn,groupid from device where deviceno=@deviceno";
                var query = conn.Query<DeviceInfo>(strSql, new { deviceno = DeviceNo }).FirstOrDefault();
                if (query == null)
                {
                    AppLog.Error($"设备为{DeviceNo}的设备不存在");
                    query = new DeviceInfo { IsExist = false };
                }
                else
                {
                    query.IsExist = true;
                }
                return query;
            }
        }
        private void AddHisData(string content, string groupId, string title, string devicesn, string DeviceNo)
        {
#if Debug
                      return;
#else
            #region mysql添加数据
            using (IDbConnection conn = new MySqlConnection(MySqlConnectionString))
            {
                //直接插入，数据库有约束，如果设备不存在，则不能加入
                string tableName = $"devhis_{DateTime.Now.ToString("yyyyMMdd")}";
                try
                {
                    string strSql = $"insert into {tableName} (dt,datacontent,datatitle,groupId,devicesn) values(@dt,@datacontent,@datatitle,@groupId,@devicesn);";
                    var query = conn.Execute(strSql, new { dt = DateTime.Now, datacontent = content, datatitle = title, groupId = groupId, devicesn = devicesn });
                    iSend++;
                    SetStatusTool();
                }
                catch (Exception ex)
                {
                    //写日志
                    AppLog.Error($"添加历史数据失败，错误原因->{ex.Message};错误数据为：设备序列号：{devicesn},设备编号:{DeviceNo},主题:{title},内容:{content},groupId:{groupId}");
                }
            }
            #endregion
#endif
        }
        private void AddWarnData(string message, string deviceno, string devicesn)
        {
#if Debug
            return;
#else
            using (IDbConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    Dictionary<string, object> dic = AnalyData(message);
                    foreach (var item in dic)
                    {
                        //先检查是否存在相同未处理的报警
                        string sql = "select count(1) from warn where code=@code and deviceno=@deviceno and state=0";
                        var query = conn.Query<int>(sql, new { code = item.Key, deviceno = deviceno }).FirstOrDefault();
                        if (query > 0)
                        {
                            return;//存在未处理的报警
                        }
                        sql = "insert into warn (code,dt,devicesn,deviceno,state) values(@code,@dt,@devicesn,@deviceno,@state)";
                        var r = conn.Execute(sql, new { code = item.Key, dt = DateTime.Now, devicesn = devicesn, deviceno = deviceno, state = 0 });
                        iSendAlarm++;
                        SetStatusTool();
                    }
                }
                catch (Exception ex)
                {
                    //写日志
                    AppLog.Error("添加报警失败，错误原因->" + ex.Message + "错误数据：" + deviceno + "->" + message);
                }
            }
#endif
        }
        private void InitMqtt()
        {
            string address = txtAddress.Text.Trim();
            string[] add = address.Split(':');
            if (add.Length == 1)
            {
                mqtt = new MqttClient(address);
            }
            else
            {
                mqtt = new MqttClient(add[0], int.Parse(add[1]), false,
                MqttSslProtocols.None,
                null,
                null);
            }
            mqtt.MqttMsgPublishReceived += Mqtt_MqttMsgPublishReceived;
            mqtt.ConnectionClosed += Mqtt_ConnectionClosed;
        }
        private void InitListView()
        {
            //设置行高
            ImageList h = new ImageList();
            h.ImageSize = new System.Drawing.Size(1, 25);
            lvMessage.SmallImageList = h;
            lvMessage.Columns.AddRange(new ColumnHeader[] {
                    new ColumnHeader(){Text="主题", TextAlign=HorizontalAlignment.Left,Width=150},
                    new ColumnHeader(){Text="消息",TextAlign=HorizontalAlignment.Left,Width=500},
                    new ColumnHeader(){Text="接收时间",TextAlign=HorizontalAlignment.Right,Width=150}
                    });
        }
        private void Mqtt_ConnectionClosed(object sender, EventArgs e)
        {
            //当断开服务器的连接时，启动自动连接线程
            bConnectServer = true;
            bool b = mqtt.IsConnected;
            AppLog.Error("断开服务器:" + b.ToString() + "---" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            ActConnetct.BeginInvoke(MonitorConnectCallBack, ActConnetct);
        }
        public Action ActConnetct;//监视服务器连接断开
        public void MonitorConnect()
        {
            mqtt.Connect(Guid.NewGuid().ToString(), "admin", "password", true, 60);
            if (mqtt.IsConnected)
            {
                string topic = "#";
                string[] top = new string[] { topic };
                byte[] byQos = new byte[] { 1 };
                mqtt.Subscribe(top, byQos);
                bConnectServer = false;
            }

        }
        //回调函数，连接Mqtt服务器
        public void MonitorConnectCallBack(IAsyncResult result)
        {
            AsyncResult _result = (AsyncResult)result;
            Action act = (Action)_result.AsyncDelegate;
            try
            {
                act.EndInvoke(_result);
                Thread.Sleep(1000 * 60);//1分钟后重连
                if (bConnectServer)
                {
                    //Thread.Sleep(2000);
                    result = act.BeginInvoke(MonitorConnectCallBack, act);
                }
            }
            catch (Exception ex)
            {
                Thread.Sleep(12000);
                AppLog.Error(ex.Message);
                act.BeginInvoke(MonitorConnectCallBack, act);
            }
        }

        private void Mqtt_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            //接收数据
            string topic = e.Topic;
            string message = System.Text.UTF8Encoding.UTF8.GetString(e.Message);
            //合法的主题包含3段，第一段为组织代码，第二段为设备编号，第三段为主题类型
            string[] topics = topic.Split('/');
            if (topics.Length < 3)
            {
                return;
            }
            try
            {
                string deviceNo = topics[1];

                switch (topics[2].ToLower())
                {
                    case "DevicePub"://处理设备上报数据
                    case "devicepub"://处理设备上报数据
                        Dictionary<string, object> dic = AnalyData(message);
                        #region 处理设备上报数据
                        if (!dic.ContainsKey("action"))   //数据包格式不正确
                        {
                            return;
                        }
                        iReceive++;
                        //判断是否是设备上报数据
                        if (dic["action"].ToString().ToLower() == "timing_upload")//设备正常上报数据
                        {
                            //判断设备上传的数据是否有遗言
                            int will = dic.ContainsKey("will") == false ? 0 : int.Parse(dic["will"].ToString());//设备上传的数据是否有遗言
                            //判断是否有分包数据
                            int totalpage = dic.ContainsKey("totalpage") == false ? 0 : int.Parse(dic["totalpage"].ToString());//包的序列号，防止不同的包混到一起
                            if (totalpage > 0)
                            {
                                int seq = dic.ContainsKey("seq") == false ? 0 : int.Parse(dic["seq"].ToString());//包的序列号，防止不同的包混到一起
                                string key = deviceNo + seq.ToString();//存到缓存的标示
                                bool isComplete = HandleMessage(dic);
                                if (isComplete)
                                {
                                    CacheData cd;
                                    //移除缓存
                                    lock (Pagelocker)
                                    {
                                        dicPage.TryRemove(key, out cd);
                                    }
                                    statusStrip1.Invoke(new MethodInvoker(() =>
                                    {
                                        tsl.Text = $"清理一条缓存，还有{dicPage.Count}条缓存";
                                    }));
                                    message = HandlePackage(cd);//组合消息
                                }
                                else
                                {
                                    return;//包不完整，不处理
                                }
                            }
                            if (dicOnLine.ContainsKey(deviceNo))//缓存中包含该设备
                            {
                                //更新设备在线时间和在线状态
                                lock (OnlineLocker)
                                {
                                    dicOnLine[deviceNo].Dt = DateTime.Now;
                                    dicOnLine[deviceNo].IsWill = will == 0 ? false : true;
                                    SetStatusTool();
                                    if (dicOnLine[deviceNo].SendTime.AddMinutes(iInterval) < DateTime.Now)//大于固定时间间隔，发送数据并更新发送数据时间
                                    {
                                        dicOnLine[deviceNo].SendTime = DateTime.Now;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                            }
                            else //缓存中不包含该设备则读取
                            {
                                var device = GetDeviceInfo(deviceNo);
                                if (!device.IsExist)
                                {
                                    //设备不存在，把设备的数据打印到info中
                                    //  AppLog.Info($"设备为{deviceNo}的设备不存在,本次收到的数据为{message}");
                                    return;
                                }
                                else
                                {

                                    dicOnLine.TryAdd(deviceNo, new OnlineData
                                    {
                                        Dt = DateTime.Now,
                                        IsWill = will == 0 ? false : true,
                                        DeviceSn = device.DeviceSn,
                                        DeviceNo = deviceNo,
                                        GroupId = device.GroupId,
                                        SendTime = DateTime.Now //设备在缓存中不存在需要设置发送时间
                                    });
                                }
                                //发送设备上线数据
                                SetOnlineData();
                                SetStatusTool();
                            }
                            //处理设备上线
                            HandleDeviceOnlineData(deviceNo, true, dicOnLine[deviceNo].GroupId, dicOnLine[deviceNo].DeviceSn, message, topic);
                            //iSend++;
                            AddHisData(message, dicOnLine[deviceNo].GroupId, topic, dicOnLine[deviceNo].DeviceSn, deviceNo);
                            //SetStatusTool();
                        }
                        else if (dic["action"].ToString().ToLower() == "alarm")//报警数据
                        {
                            if (!dicOnLine.ContainsKey(deviceNo))
                            {
                                return;//该设备没录入到系统中 
                            }
                            iAlarm++;
                            SetStatusTool();
                            string Mess = dic["error"].ToString();
                            AddWarnData(Mess, deviceNo, dicOnLine[deviceNo].DeviceSn);
                        }
                        else
                        {
                            iOther++;
                            return;//其它数据不处理
                        }
                        break;
                    #endregion
                    #region 报警主题暂时开通
                    /*
                case "DeviceAlarm"://处理报警数据
                    if (dic.ContainsKey("action") && dic["action"].ToString().ToLower() == "devicealarm")
                    {
                        if (!DicNo.ContainsKey(deviceNo))//设备编号不在缓存中，则不处理该报警数据
                        {
                            return;
                        }
                        iAlarm++;
                        string Mess = dic["error"].ToString();
                        //保存报警数据
                      //  AddWarnData(Message)
                    }
                    else
                    {
                        return;
                    }
                    break;
                    */
                    #endregion
                    case "offline"://设备离线
                        if (dicOnLine.ContainsKey(deviceNo))
                        {
                            OnlineData data;
                            lock (OnlineLocker)
                            {
                                //设备下线
                                dicOnLine.TryRemove(deviceNo, out data);
                                HandleDeviceOnlineData(deviceNo, false, data.GroupId, data.DeviceSn);
                                SetOnlineData();
                            }
                        }
                        break;
                    default://其它数据不处理
                        break;
                }
                //把接收的数据显示在列表中
                lvMessage.Invoke(new /*MethodInvoker*/ Action(() =>
                {
                    lvMessage.BeginUpdate();
                    if (lvMessage.Items.Count > 100)
                    {
                        lvMessage.Items.RemoveAt(lvMessage.Items.Count - 1);
                    }
                    ListViewItem lv = new ListViewItem();
                    lv.Text = e.Topic;
                    lv.SubItems.Add(message);
                    lv.SubItems.Add(DateTime.Now.ToLongTimeString());

                    //一定记得行数据创建完毕后添加到列表中
                    //lvContent.Items.Add(lv);
                    lvMessage.Items.Insert(0, lv);
                    lvMessage.EndUpdate();
                    //tsPublabel.Text = "接收到:" + iReceive + "条数据，发送:" + iSend + "条数据";
                }));
            }
            catch (Exception ex)
            {
                AppLog.Error($"数据解析错误:->{ ex.Message}: 错误主题为:{topic}:错误消息为=>{message}");
            }

        }

        private void SetOnlineData()
        {
            statusStrip1.Invoke(new Action(() =>
            {
                tsOnline.Text = $"现有{dicOnLine.Count}台设备在线";
            }));
        }

        private void SetStatusTool(/*string message, string topic*/)
        {
            statusStrip1.BeginInvoke(new Action(() =>
            {
                tsPublabel.Text = "接收到:" + iReceive + "条数据，发送:" + iSend + "条数据,收到报警数据:" + iAlarm + "条,已上传的报警数据:" + iSendAlarm + "条";
            }));
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            InitMqtt();
            try
            {
                if (!mqtt.IsConnected)
                {
                    mqtt.Connect(Guid.NewGuid().ToString(), "admin", "password", true, 60);
                    this.btnConnect.Enabled = false;
                    bConnectServer = false;
                }
            }
            catch (Exception ex)
            {
                AppLog.Error("连接代理服务器失败" + ex.Message + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //bConnectServer = true;
                this.btnConnect.Enabled = true;
            }
        }

        private void BtnSub_Click(object sender, EventArgs e)
        {
            string title = txtTitle.Text.Trim();
            if (mqtt.IsConnected)
            {
                mqtt.Subscribe(new[] { title }, new byte[] { 0 });
                btnSub.Enabled = false;
            }
            else
            {
                MessageBox.Show("请连接服务器");
            }
        }


        private void lvMessage_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.lvMessage.FocusedItem != null)//这个if必须的，不然会得到值但会报错  
            {
                //MessageBox.Show(this.listView1.FocusedItem.SubItems[0].Text);  
                //  this.textBox1.Text = this.lvContent.FocusedItem.SubItems[0].Text;//获得的listView的值显示在文本框里  
                string str = lvMessage.FocusedItem.SubItems[0].Text;
                string content = lvMessage.FocusedItem.SubItems[1].Text;
                string time = lvMessage.FocusedItem.SubItems[2].Text;
                if (fm == null)
                {
                    fm = new FrmMessage();
                    fm.FormClose = SetChildForm;
                    fm.Show();
                }
                else
                {
                    fm.BringToFront();
                }
                fm.Act(str, time, content);
            }
        }
        public void SetChildForm()
        {
            fm = null;
        }
        private void BtnClear_Click(object sender, EventArgs e)
        {
            lvMessage.Items.Clear();
        }

        public Dictionary<string, object> AnalyData(string strMessage)
        {
            try
            {
                Dictionary<string, object> dic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(strMessage);
                return dic;
            }
            catch (Exception ex)
            {
                //AppLog.Error("数据解析错误:->" + ex.Message + "错误数据为:" + strMessage);
                throw new Exception(ex.Message + strMessage);
            }
        }

        private void BtnSetInterval_Click(object sender, EventArgs e)
        {
            try
            {
                iInterval = Convert.ToInt32(txtInterval.Text.Trim());
            }
            catch
            {
                MessageBox.Show("时间间隔只能为整数，目前的间隔为:" + iInterval.ToString() + "分钟");
            }

        }

        private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            #region 结束线程
            if (thCache.IsAlive)
            {
                thCache.Abort();
            }
            if (thOnLine.IsAlive)
            {
                thOnLine.Abort();
            }
            #endregion
            if (mqtt != null && mqtt.IsConnected)
            {
                mqtt.Disconnect();
                Environment.Exit(0);
            }

        }

        #region 分离历史数据

        #endregion
    }

    #region 处理分包数据包所用数据
    public class CacheData
    {
        //public string Key { get; set; }//deviceno+seq
        public List<HandleData> Data { get; set; }
        public DateTime dt { get; set; }//10分钟以内有效

        public int Total { get; set; }    //包大小
        public string Action { get; set; }//包Action
        public string DeviceNo { get; set; }//设备编号
        public int Sequence { get; set; }//包的序号
        public override string ToString()
        {
            return string.Join(",", Data);//把数据列表转换为字符串
        }
    }
    public class SData
    {
        public string action { get; set; }//包Action
        public string uuid { get; set; }//设备编号
        public Dictionary<string, object> data { get; set; }
    }
    public class HandleData
    {
        public int Num { get; set; }//包号
        public string Message { get; set; }
        public override string ToString()
        {
            return $"当前包为{Num}:{Message};";
        }
    }
    #endregion
    #region 设备在线数据
    public class OnlineData
    {
        public DateTime Dt { get; set; }//记录设备上次在线时间
        public bool IsWill { get; set; }                      //设备消息是否有遗言

        public bool IsOnline { get; set; }//设备是否在线，用于确认设备是否在线

        public string DeviceNo { get; set; }
        public string DeviceSn { get; set; }
        public string GroupId { get; set; }
        public DateTime SendTime { get; set; }//记录上次发送数据的时间
    }
    /// <summary>
    /// 用来处理获取设备信息
    /// </summary>
    public class DeviceInfo
    {
        public bool IsExist { get; set; }//设备是否存在
        public string DeviceSn { get; set; }//设备编号
        public string GroupId { get; set; }//组织编号
    }
    #endregion
}

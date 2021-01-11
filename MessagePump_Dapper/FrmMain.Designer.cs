namespace MessagePump_Dapper
{
    partial class FrmMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.txtInterval = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnSetInterval = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnSub = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.txtAddress = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lvMessage = new System.Windows.Forms.ListView();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsPublabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsl = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsOnline = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsDevice = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.txtInterval);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.btnSetInterval);
            this.splitContainer1.Panel1.Controls.Add(this.btnClear);
            this.splitContainer1.Panel1.Controls.Add(this.btnSub);
            this.splitContainer1.Panel1.Controls.Add(this.btnConnect);
            this.splitContainer1.Panel1.Controls.Add(this.txtTitle);
            this.splitContainer1.Panel1.Controls.Add(this.txtAddress);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lvMessage);
            this.splitContainer1.Size = new System.Drawing.Size(823, 497);
            this.splitContainer1.SplitterDistance = 91;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 0;
            // 
            // txtInterval
            // 
            this.txtInterval.Location = new System.Drawing.Point(640, 10);
            this.txtInterval.Margin = new System.Windows.Forms.Padding(2);
            this.txtInterval.Name = "txtInterval";
            this.txtInterval.Size = new System.Drawing.Size(76, 21);
            this.txtInterval.TabIndex = 4;
            this.txtInterval.Text = "5";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(575, 18);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "时间间隔：";
            // 
            // btnSetInterval
            // 
            this.btnSetInterval.Location = new System.Drawing.Point(602, 55);
            this.btnSetInterval.Margin = new System.Windows.Forms.Padding(2);
            this.btnSetInterval.Name = "btnSetInterval";
            this.btnSetInterval.Size = new System.Drawing.Size(90, 28);
            this.btnSetInterval.TabIndex = 1;
            this.btnSetInterval.Text = "设置时间间隔";
            this.btnSetInterval.UseVisualStyleBackColor = true;
            this.btnSetInterval.Click += new System.EventHandler(this.BtnSetInterval_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(448, 55);
            this.btnClear.Margin = new System.Windows.Forms.Padding(2);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(79, 28);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "清空列表";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // btnSub
            // 
            this.btnSub.Location = new System.Drawing.Point(295, 55);
            this.btnSub.Margin = new System.Windows.Forms.Padding(2);
            this.btnSub.Name = "btnSub";
            this.btnSub.Size = new System.Drawing.Size(79, 28);
            this.btnSub.TabIndex = 2;
            this.btnSub.Text = "订阅主题";
            this.btnSub.UseVisualStyleBackColor = true;
            this.btnSub.Click += new System.EventHandler(this.BtnSub_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(141, 55);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(2);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(79, 28);
            this.btnConnect.TabIndex = 2;
            this.btnConnect.Text = "连接服务器";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
            // 
            // txtTitle
            // 
            this.txtTitle.Location = new System.Drawing.Point(396, 10);
            this.txtTitle.Margin = new System.Windows.Forms.Padding(2);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(128, 21);
            this.txtTitle.TabIndex = 1;
            this.txtTitle.Text = "#";
            // 
            // txtAddress
            // 
            this.txtAddress.Location = new System.Drawing.Point(188, 10);
            this.txtAddress.Margin = new System.Windows.Forms.Padding(2);
            this.txtAddress.Name = "txtAddress";
            this.txtAddress.Size = new System.Drawing.Size(135, 21);
            this.txtAddress.TabIndex = 1;
            this.txtAddress.Text = "www.hexucloud.com:18831";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(359, 18);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "主题：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(107, 18);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Broker地址：";
            // 
            // lvMessage
            // 
            this.lvMessage.BackColor = System.Drawing.SystemColors.Info;
            this.lvMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvMessage.FullRowSelect = true;
            this.lvMessage.GridLines = true;
            this.lvMessage.HideSelection = false;
            this.lvMessage.Location = new System.Drawing.Point(0, 0);
            this.lvMessage.Margin = new System.Windows.Forms.Padding(2);
            this.lvMessage.Name = "lvMessage";
            this.lvMessage.Size = new System.Drawing.Size(823, 403);
            this.lvMessage.TabIndex = 0;
            this.lvMessage.UseCompatibleStateImageBehavior = false;
            this.lvMessage.View = System.Windows.Forms.View.Details;
            this.lvMessage.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvMessage_MouseDoubleClick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsPublabel,
            this.tsl,
            this.tsOnline,
            this.tsDevice});
            this.statusStrip1.Location = new System.Drawing.Point(0, 471);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 10, 0);
            this.statusStrip1.Size = new System.Drawing.Size(823, 26);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsPublabel
            // 
            this.tsPublabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.tsPublabel.Name = "tsPublabel";
            this.tsPublabel.Size = new System.Drawing.Size(135, 21);
            this.tsPublabel.Text = "toolStripStatusLabel1";
            // 
            // tsl
            // 
            this.tsl.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.tsl.Name = "tsl";
            this.tsl.Size = new System.Drawing.Size(135, 21);
            this.tsl.Text = "toolStripStatusLabel1";
            this.tsl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tsOnline
            // 
            this.tsOnline.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.tsOnline.Name = "tsOnline";
            this.tsOnline.Size = new System.Drawing.Size(40, 21);
            this.tsOnline.Text = "fffffff";
            // 
            // tsDevice
            // 
            this.tsDevice.Name = "tsDevice";
            this.tsDevice.Size = new System.Drawing.Size(92, 21);
            this.tsDevice.Text = "缓存设备数量：";
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(823, 497);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "数据监控软件";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmMain_FormClosed);
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnSub;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.TextBox txtAddress;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView lvMessage;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tsPublabel;
        private System.Windows.Forms.TextBox txtInterval;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSetInterval;
        private System.Windows.Forms.ToolStripStatusLabel tsl;
        private System.Windows.Forms.ToolStripStatusLabel tsOnline;
        private System.Windows.Forms.ToolStripStatusLabel tsDevice;
    }
}


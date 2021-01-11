using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MessagePump_Dapper
{
    public partial class FrmMessage : Form
    {
        public FrmMessage()
        {
            InitializeComponent();
            Act = SetMessage;
        }
        public Action<string, string, string> Act;
        private void FrmMessage_Load(object sender, EventArgs e)
        {

        }
        public void SetMessage(string strTopic, string strTime, string strContent)
        {
            txtTitle.Text = strTopic;
            txtTime.Text = strTime;
            txtContent.Text = strContent;
        }
        public Action FormClose;
        private void FrmMessage_FormClosed(object sender, FormClosedEventArgs e)
        {
            FormClose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MultiClient;

namespace Client
{
    public partial class ChatApplicationFrom : Form
    {
        public ChatApplicationFrom()
        {
            InitializeComponent();
        }


        public void AddTextToMainChatBox(string msg)
        {
            SetText(msg,false); 
        }

        public void ClearTextFromMainChatBox()
        {
            SetText("",true);
        }

        delegate void SetTextCallback(string text , bool clear);

        private void SetText(string text, bool clear)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.MainChatBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text , clear});
            }
            else
            {
                if (clear)
                {
                    text = (text).Replace("\n", "\r\n");
                }
                else
                {
                    text = (this.MainChatBox.Text + "\n" + text).Replace("\n", "\r\n");
                }
                this.MainChatBox.Text = text;
            }
        }
        #region Empty functions
        private void loginBtn_Click(object sender, EventArgs e)
        {
            string phonenumber = this.phoneInput.Text.Trim();
            string password = this.passwordInput.Text.Trim();
            MultiClient.Client.TriggerLoginEvent(phonenumber, password);
        }

        private void openChatBtn_Click(object sender, EventArgs e)
        {
            MultiClient.Client.OpenChat(this.receiver.Text.Trim());
        }

        private void sendBtn_Click(object sender, EventArgs e)
        {
            MultiClient.Client.SendChatMessage(this.receiver.Text.Trim(), this.newMessageBox.Text.Trim());
        }

        private void MainChatBox_TextChanged(object sender, EventArgs e)
        {

        }
        private void LoginLabel_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
        #endregion
    }
}

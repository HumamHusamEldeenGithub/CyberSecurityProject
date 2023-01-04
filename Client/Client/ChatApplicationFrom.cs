using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MultiClient;
using static System.Net.Mime.MediaTypeNames;

namespace Client
{
    public partial class ChatApplicationFrom : Form
    {
        List<Tuple<TextBox, string>> messages;
        public ChatApplicationFrom()
        {
            InitializeComponent();

            messages = new List<Tuple<TextBox, string>>();
        }

        public void AddTextToMainChatBox(string msg)
        {
            AddInfoText(msg,false); 
        }

        public void ClearTextFromMainChatBox()
        {
            AddInfoText("",true);
        }

        delegate void SetMessageReceivedCallback(string id);
        public void SetMessageReceived(string id)
        {

            if (this.MainChatBox.InvokeRequired)
            {
                SetMessageReceivedCallback d = new SetMessageReceivedCallback(SetMessageReceived);
                this.Invoke(d, new object[] { id });
                return;
            }

            var textBox = messages.Find((x) => {
                
                return x.Item2 == id;
            })?.Item1;


            if (textBox != null)
            {
                textBox.Font = new Font(textBox.Font.FontFamily, (float)10.0 , FontStyle.Bold);
            }
            else
            {
                Debug.WriteLine("ID NOT FOUND " + id.ToString());
            }
        }
        delegate void AddMessageCallback(string message, string sender, string uuid, bool received, bool im_sender);
        public void AddMessage(string message,string sender, string uuid, bool received , bool im_sender)
        {

            if (this.MainChatBox.InvokeRequired)
            {
                AddMessageCallback d = new AddMessageCallback(AddMessage);
                this.Invoke(d, new object[] { message,sender, uuid, received , im_sender });
                return;
            }

            if (sender != null && sender != this.receiver.Text.Trim()) return;

            TextBox newTextBox = new TextBox();
            newTextBox.Text = message;
            newTextBox.ReadOnly = true;
            if (im_sender)
            {
                newTextBox.BackColor = ColorTranslator.FromHtml("#233D4D"); ; newTextBox.ForeColor = Color.White;
            }

            else
            {
                newTextBox.BackColor = ColorTranslator.FromHtml("#FCCA46"); ; newTextBox.ForeColor = Color.Black;
            }
                
            newTextBox.Size = new Size((int)(MainChatBox.Size.Width * 0.95f), newTextBox.Size.Height);

            if (received)
            {
                newTextBox.Font = new Font(newTextBox.Font, FontStyle.Bold);
            }

            messages.Add(new Tuple<TextBox, string>(newTextBox, uuid));

            this.MainChatBox.Controls.Add(newTextBox);
            MainChatBox.ScrollControlIntoView(newTextBox);
        }


        delegate void SetTextCallback(string text, bool clear);
        private void AddInfoText(string text, bool clear)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.


            if (this.MainChatBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(AddInfoText);
                this.Invoke(d, new object[] { text , clear});
            }
            else
            {
                if (clear)
                {
                    this.MainChatBox.Controls.Clear();
                }

                TextBox newTextBox = new TextBox();
                newTextBox.Text = text;
                newTextBox.ReadOnly= true;
                newTextBox.BackColor = ColorTranslator.FromHtml("#233D4D"); ; newTextBox.ForeColor = Color.White;
                newTextBox.Size = new Size((int)(MainChatBox.Size.Width * 0.95f), newTextBox.Size.Height);
                

                this.MainChatBox.Controls.Add(newTextBox);
                MainChatBox.ScrollControlIntoView(newTextBox);
            }
        }

        private void loginBtn_Click(object sender, EventArgs e)
        {
            string phonenumber = this.phoneInput.Text.Trim();
            string password = this.passwordInput.Text.Trim();
            MultiClient.Client.TriggerLoginEvent(phonenumber, password);
        }

        private void openChatBtn_Click(object sender, EventArgs e)
        {
            MultiClient.Client.DisplayChatMessages(this.receiver.Text.Trim());
        }

        private void sendBtn_Click(object sender, EventArgs e)
        {
            MultiClient.Client.SendChatMessage(this.receiver.Text.Trim(), this.newMessageBox.Text.Trim());
        }
        private void newMessageBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                MultiClient.Client.SendChatMessage(this.receiver.Text.Trim(), this.newMessageBox.Text.Trim());
                this.newMessageBox.Text = "";
            }
        }

        #region Empty functions

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

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void MainChatBox_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}

namespace Client
{
    partial class ChatApplicationFrom
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.phoneInput = new System.Windows.Forms.TextBox();
            this.PhoneLabel = new System.Windows.Forms.Label();
            this.passwordInput = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.receiver = new System.Windows.Forms.TextBox();
            this.sendBtn = new System.Windows.Forms.Button();
            this.newMessageBox = new System.Windows.Forms.TextBox();
            this.loginBtn = new System.Windows.Forms.Button();
            this.openChatBtn = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.MainChatBox = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // phoneInput
            // 
            this.phoneInput.Location = new System.Drawing.Point(163, 11);
            this.phoneInput.Margin = new System.Windows.Forms.Padding(4);
            this.phoneInput.Name = "phoneInput";
            this.phoneInput.Size = new System.Drawing.Size(292, 22);
            this.phoneInput.TabIndex = 1;
            // 
            // PhoneLabel
            // 
            this.PhoneLabel.AutoSize = true;
            this.PhoneLabel.Font = new System.Drawing.Font("Tahoma", 10F);
            this.PhoneLabel.Location = new System.Drawing.Point(13, 11);
            this.PhoneLabel.Margin = new System.Windows.Forms.Padding(13, 0, 13, 0);
            this.PhoneLabel.Name = "PhoneLabel";
            this.PhoneLabel.Size = new System.Drawing.Size(116, 21);
            this.PhoneLabel.TabIndex = 2;
            this.PhoneLabel.Text = "Phone number";
            this.PhoneLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.PhoneLabel.Click += new System.EventHandler(this.LoginLabel_Click);
            // 
            // passwordInput
            // 
            this.passwordInput.Location = new System.Drawing.Point(163, 43);
            this.passwordInput.Margin = new System.Windows.Forms.Padding(4);
            this.passwordInput.Name = "passwordInput";
            this.passwordInput.Size = new System.Drawing.Size(292, 22);
            this.passwordInput.TabIndex = 3;
            this.passwordInput.UseSystemPasswordChar = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 10F);
            this.label1.Location = new System.Drawing.Point(13, 43);
            this.label1.Margin = new System.Windows.Forms.Padding(13, 0, 13, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 21);
            this.label1.TabIndex = 4;
            this.label1.Text = "Password";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 10F);
            this.label2.Location = new System.Drawing.Point(13, 101);
            this.label2.Margin = new System.Windows.Forms.Padding(13, 0, 13, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 21);
            this.label2.TabIndex = 6;
            this.label2.Text = "Receiver";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // receiver
            // 
            this.receiver.Location = new System.Drawing.Point(163, 101);
            this.receiver.Margin = new System.Windows.Forms.Padding(4);
            this.receiver.Name = "receiver";
            this.receiver.Size = new System.Drawing.Size(292, 22);
            this.receiver.TabIndex = 7;
            // 
            // sendBtn
            // 
            this.sendBtn.Font = new System.Drawing.Font("Tahoma", 15F);
            this.sendBtn.Location = new System.Drawing.Point(551, 527);
            this.sendBtn.Margin = new System.Windows.Forms.Padding(4);
            this.sendBtn.Name = "sendBtn";
            this.sendBtn.Size = new System.Drawing.Size(133, 49);
            this.sendBtn.TabIndex = 8;
            this.sendBtn.Text = "Send";
            this.sendBtn.UseVisualStyleBackColor = true;
            this.sendBtn.Click += new System.EventHandler(this.sendBtn_Click);
            // 
            // newMessageBox
            // 
            this.newMessageBox.Location = new System.Drawing.Point(17, 544);
            this.newMessageBox.Margin = new System.Windows.Forms.Padding(4);
            this.newMessageBox.Multiline = true;
            this.newMessageBox.Name = "newMessageBox";
            this.newMessageBox.Size = new System.Drawing.Size(465, 24);
            this.newMessageBox.TabIndex = 9;
            this.newMessageBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.newMessageBox_KeyDown);
            // 
            // loginBtn
            // 
            this.loginBtn.Font = new System.Drawing.Font("Tahoma", 15F);
            this.loginBtn.Location = new System.Drawing.Point(492, 15);
            this.loginBtn.Margin = new System.Windows.Forms.Padding(4);
            this.loginBtn.Name = "loginBtn";
            this.loginBtn.Size = new System.Drawing.Size(133, 53);
            this.loginBtn.TabIndex = 11;
            this.loginBtn.Text = "Login";
            this.loginBtn.UseVisualStyleBackColor = true;
            this.loginBtn.Click += new System.EventHandler(this.loginBtn_Click);
            // 
            // openChatBtn
            // 
            this.openChatBtn.Font = new System.Drawing.Font("Tahoma", 10F);
            this.openChatBtn.Location = new System.Drawing.Point(492, 79);
            this.openChatBtn.Margin = new System.Windows.Forms.Padding(4);
            this.openChatBtn.Name = "openChatBtn";
            this.openChatBtn.Size = new System.Drawing.Size(133, 47);
            this.openChatBtn.TabIndex = 12;
            this.openChatBtn.Text = "Open Chat";
            this.openChatBtn.UseVisualStyleBackColor = true;
            this.openChatBtn.Click += new System.EventHandler(this.openChatBtn_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 524);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(124, 16);
            this.label3.TabIndex = 13;
            this.label3.Text = "Enter  a message ...";
            // 
            // MainChatBox
            // 
            this.MainChatBox.AutoScroll = true;
            this.MainChatBox.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.MainChatBox.Location = new System.Drawing.Point(17, 144);
            this.MainChatBox.Name = "MainChatBox";
            this.MainChatBox.Size = new System.Drawing.Size(667, 366);
            this.MainChatBox.TabIndex = 14;
            this.MainChatBox.WrapContents = false;
            this.MainChatBox.Paint += new System.Windows.Forms.PaintEventHandler(this.MainChatBox_Paint);
            // 
            // ChatApplicationFrom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(701, 604);
            this.Controls.Add(this.MainChatBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.openChatBtn);
            this.Controls.Add(this.loginBtn);
            this.Controls.Add(this.newMessageBox);
            this.Controls.Add(this.sendBtn);
            this.Controls.Add(this.receiver);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.passwordInput);
            this.Controls.Add(this.PhoneLabel);
            this.Controls.Add(this.phoneInput);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ChatApplicationFrom";
            this.Text = "Chat Application";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox phoneInput;
        private System.Windows.Forms.Label PhoneLabel;
        private System.Windows.Forms.TextBox passwordInput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox receiver;
        private System.Windows.Forms.Button sendBtn;
        private System.Windows.Forms.TextBox newMessageBox;
        private System.Windows.Forms.Button loginBtn;
        private System.Windows.Forms.Button openChatBtn;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.FlowLayoutPanel MainChatBox;
    }
}
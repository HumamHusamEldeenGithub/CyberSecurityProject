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
            this.MainChatBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.receiver = new System.Windows.Forms.TextBox();
            this.sendBtn = new System.Windows.Forms.Button();
            this.newMessageBox = new System.Windows.Forms.TextBox();
            this.loginBtn = new System.Windows.Forms.Button();
            this.openChatBtn = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // phoneInput
            // 
            this.phoneInput.Location = new System.Drawing.Point(122, 9);
            this.phoneInput.Name = "phoneInput";
            this.phoneInput.Size = new System.Drawing.Size(220, 20);
            this.phoneInput.TabIndex = 1;
            // 
            // PhoneLabel
            // 
            this.PhoneLabel.AutoSize = true;
            this.PhoneLabel.Font = new System.Drawing.Font("Tahoma", 10F);
            this.PhoneLabel.Location = new System.Drawing.Point(10, 9);
            this.PhoneLabel.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.PhoneLabel.Name = "PhoneLabel";
            this.PhoneLabel.Size = new System.Drawing.Size(99, 17);
            this.PhoneLabel.TabIndex = 2;
            this.PhoneLabel.Text = "Phone number";
            this.PhoneLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.PhoneLabel.Click += new System.EventHandler(this.LoginLabel_Click);
            // 
            // passwordInput
            // 
            this.passwordInput.Location = new System.Drawing.Point(122, 35);
            this.passwordInput.Name = "passwordInput";
            this.passwordInput.Size = new System.Drawing.Size(220, 20);
            this.passwordInput.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 10F);
            this.label1.Location = new System.Drawing.Point(10, 35);
            this.label1.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Password";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainChatBox
            // 
            this.MainChatBox.Font = new System.Drawing.Font("Tahoma", 10F);
            this.MainChatBox.Location = new System.Drawing.Point(13, 108);
            this.MainChatBox.MinimumSize = new System.Drawing.Size(500, 300);
            this.MainChatBox.Multiline = true;
            this.MainChatBox.Name = "MainChatBox";
            this.MainChatBox.ReadOnly = true;
            this.MainChatBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.MainChatBox.Size = new System.Drawing.Size(500, 300);
            this.MainChatBox.TabIndex = 5;
            this.MainChatBox.TextChanged += new System.EventHandler(this.MainChatBox_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 10F);
            this.label2.Location = new System.Drawing.Point(10, 82);
            this.label2.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 17);
            this.label2.TabIndex = 6;
            this.label2.Text = "Receiver";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // receiver
            // 
            this.receiver.Location = new System.Drawing.Point(122, 82);
            this.receiver.Name = "receiver";
            this.receiver.Size = new System.Drawing.Size(220, 20);
            this.receiver.TabIndex = 7;
            // 
            // sendBtn
            // 
            this.sendBtn.Font = new System.Drawing.Font("Tahoma", 15F);
            this.sendBtn.Location = new System.Drawing.Point(413, 428);
            this.sendBtn.Name = "sendBtn";
            this.sendBtn.Size = new System.Drawing.Size(100, 40);
            this.sendBtn.TabIndex = 8;
            this.sendBtn.Text = "Send";
            this.sendBtn.UseVisualStyleBackColor = true;
            this.sendBtn.Click += new System.EventHandler(this.sendBtn_Click);
            // 
            // newMessageBox
            // 
            this.newMessageBox.Location = new System.Drawing.Point(13, 442);
            this.newMessageBox.Multiline = true;
            this.newMessageBox.Name = "newMessageBox";
            this.newMessageBox.Size = new System.Drawing.Size(350, 20);
            this.newMessageBox.TabIndex = 9;
            // 
            // loginBtn
            // 
            this.loginBtn.Font = new System.Drawing.Font("Tahoma", 15F);
            this.loginBtn.Location = new System.Drawing.Point(369, 12);
            this.loginBtn.Name = "loginBtn";
            this.loginBtn.Size = new System.Drawing.Size(100, 43);
            this.loginBtn.TabIndex = 11;
            this.loginBtn.Text = "Login";
            this.loginBtn.UseVisualStyleBackColor = true;
            this.loginBtn.Click += new System.EventHandler(this.loginBtn_Click);
            // 
            // openChatBtn
            // 
            this.openChatBtn.Font = new System.Drawing.Font("Tahoma", 10F);
            this.openChatBtn.Location = new System.Drawing.Point(369, 64);
            this.openChatBtn.Name = "openChatBtn";
            this.openChatBtn.Size = new System.Drawing.Size(100, 38);
            this.openChatBtn.TabIndex = 12;
            this.openChatBtn.Text = "Open Chat";
            this.openChatBtn.UseVisualStyleBackColor = true;
            this.openChatBtn.Click += new System.EventHandler(this.openChatBtn_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 426);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Enter  a message ...";
            // 
            // ChatApplicationFrom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(526, 491);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.openChatBtn);
            this.Controls.Add(this.loginBtn);
            this.Controls.Add(this.newMessageBox);
            this.Controls.Add(this.sendBtn);
            this.Controls.Add(this.receiver);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.MainChatBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.passwordInput);
            this.Controls.Add(this.PhoneLabel);
            this.Controls.Add(this.phoneInput);
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
        private System.Windows.Forms.TextBox MainChatBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox receiver;
        private System.Windows.Forms.Button sendBtn;
        private System.Windows.Forms.TextBox newMessageBox;
        private System.Windows.Forms.Button loginBtn;
        private System.Windows.Forms.Button openChatBtn;
        private System.Windows.Forms.Label label3;
    }
}
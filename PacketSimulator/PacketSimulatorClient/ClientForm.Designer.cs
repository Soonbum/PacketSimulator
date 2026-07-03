namespace PacketSimulatorClient
{
    partial class ClientForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblPort = new Label();
            lblIP = new Label();
            txtPort = new TextBox();
            txtIP = new TextBox();
            grpEndianness = new GroupBox();
            rbBigEndian = new RadioButton();
            rbLittleEndian = new RadioButton();
            btnSend = new Button();
            txtPacket = new TextBox();
            grpEndianness.SuspendLayout();
            SuspendLayout();
            // 
            // lblPort
            // 
            lblPort.AutoSize = true;
            lblPort.Location = new Point(21, 45);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(29, 15);
            lblPort.TabIndex = 10;
            lblPort.Text = "Port";
            // 
            // lblIP
            // 
            lblIP.AutoSize = true;
            lblIP.Location = new Point(21, 19);
            lblIP.Name = "lblIP";
            lblIP.Size = new Size(17, 15);
            lblIP.TabIndex = 9;
            lblIP.Text = "IP";
            // 
            // txtPort
            // 
            txtPort.Location = new Point(75, 42);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(100, 23);
            txtPort.TabIndex = 8;
            txtPort.Text = "55555";
            // 
            // txtIP
            // 
            txtIP.Location = new Point(75, 13);
            txtIP.Name = "txtIP";
            txtIP.Size = new Size(100, 23);
            txtIP.TabIndex = 7;
            txtIP.Text = "127.0.0.1";
            // 
            // grpEndianness
            // 
            grpEndianness.Controls.Add(rbBigEndian);
            grpEndianness.Controls.Add(rbLittleEndian);
            grpEndianness.Location = new Point(201, 12);
            grpEndianness.Name = "grpEndianness";
            grpEndianness.Size = new Size(141, 76);
            grpEndianness.TabIndex = 11;
            grpEndianness.TabStop = false;
            grpEndianness.Text = "엔디안";
            // 
            // rbBigEndian
            // 
            rbBigEndian.AutoSize = true;
            rbBigEndian.Location = new Point(10, 47);
            rbBigEndian.Name = "rbBigEndian";
            rbBigEndian.Size = new Size(73, 19);
            rbBigEndian.TabIndex = 1;
            rbBigEndian.Text = "빅엔디안";
            rbBigEndian.UseVisualStyleBackColor = true;
            // 
            // rbLittleEndian
            // 
            rbLittleEndian.AutoSize = true;
            rbLittleEndian.Checked = true;
            rbLittleEndian.Location = new Point(10, 22);
            rbLittleEndian.Name = "rbLittleEndian";
            rbLittleEndian.Size = new Size(85, 19);
            rbLittleEndian.TabIndex = 0;
            rbLittleEndian.TabStop = true;
            rbLittleEndian.Text = "리틀엔디안";
            rbLittleEndian.UseVisualStyleBackColor = true;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(201, 94);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(141, 28);
            btnSend.TabIndex = 12;
            btnSend.Text = "패킷 보내기";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // txtPacket
            // 
            txtPacket.Location = new Point(21, 137);
            txtPacket.Name = "txtPacket";
            txtPacket.Size = new Size(321, 23);
            txtPacket.TabIndex = 13;
            // 
            // ClientForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(370, 184);
            Controls.Add(txtPacket);
            Controls.Add(btnSend);
            Controls.Add(grpEndianness);
            Controls.Add(lblPort);
            Controls.Add(lblIP);
            Controls.Add(txtPort);
            Controls.Add(txtIP);
            Name = "ClientForm";
            Text = "Client";
            grpEndianness.ResumeLayout(false);
            grpEndianness.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblPort;
        private Label lblIP;
        private TextBox txtPort;
        private TextBox txtIP;
        private GroupBox grpEndianness;
        private RadioButton rbBigEndian;
        private RadioButton rbLittleEndian;
        private Button btnSend;
        private TextBox txtPacket;
    }
}

namespace PacketSimulatorServer
{
    partial class ServerForm
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
            txtIP = new TextBox();
            txtPort = new TextBox();
            txtStartByte = new TextBox();
            txtOffset = new TextBox();
            txtReadRange = new TextBox();
            lblIP = new Label();
            lblPort = new Label();
            lblStartByte = new Label();
            lblOffset = new Label();
            lblReadRange = new Label();
            grpEndianness = new GroupBox();
            rbBigEndian = new RadioButton();
            rbLittleEndian = new RadioButton();
            chkStart = new CheckBox();
            pnlPacketBytes = new FlowLayoutPanel();
            lblQueueCount = new Label();
            grpEndianness.SuspendLayout();
            SuspendLayout();
            // 
            // txtIP
            // 
            txtIP.Location = new Point(199, 12);
            txtIP.Name = "txtIP";
            txtIP.Size = new Size(100, 23);
            txtIP.TabIndex = 0;
            txtIP.Text = "127.0.0.1";
            // 
            // txtPort
            // 
            txtPort.Location = new Point(199, 41);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(100, 23);
            txtPort.TabIndex = 1;
            txtPort.Text = "55555";
            // 
            // txtStartByte
            // 
            txtStartByte.Location = new Point(199, 70);
            txtStartByte.Name = "txtStartByte";
            txtStartByte.Size = new Size(100, 23);
            txtStartByte.TabIndex = 2;
            txtStartByte.Text = "D1";
            // 
            // txtOffset
            // 
            txtOffset.Location = new Point(199, 99);
            txtOffset.Name = "txtOffset";
            txtOffset.Size = new Size(100, 23);
            txtOffset.TabIndex = 3;
            txtOffset.Text = "1";
            // 
            // txtReadRange
            // 
            txtReadRange.Location = new Point(199, 128);
            txtReadRange.Name = "txtReadRange";
            txtReadRange.Size = new Size(100, 23);
            txtReadRange.TabIndex = 4;
            txtReadRange.Text = "1";
            // 
            // lblIP
            // 
            lblIP.AutoSize = true;
            lblIP.Location = new Point(23, 18);
            lblIP.Name = "lblIP";
            lblIP.Size = new Size(17, 15);
            lblIP.TabIndex = 5;
            lblIP.Text = "IP";
            // 
            // lblPort
            // 
            lblPort.AutoSize = true;
            lblPort.Location = new Point(23, 44);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(29, 15);
            lblPort.TabIndex = 6;
            lblPort.Text = "Port";
            // 
            // lblStartByte
            // 
            lblStartByte.AutoSize = true;
            lblStartByte.Location = new Point(23, 73);
            lblStartByte.Name = "lblStartByte";
            lblStartByte.Size = new Size(58, 15);
            lblStartByte.TabIndex = 7;
            lblStartByte.Text = "시작 Byte";
            // 
            // lblOffset
            // 
            lblOffset.AutoSize = true;
            lblOffset.Location = new Point(23, 102);
            lblOffset.Name = "lblOffset";
            lblOffset.Size = new Size(163, 15);
            lblOffset.TabIndex = 8;
            lblOffset.Text = "길이값 나오는 바이트 오프셋";
            // 
            // lblReadRange
            // 
            lblReadRange.AutoSize = true;
            lblReadRange.Location = new Point(23, 131);
            lblReadRange.Name = "lblReadRange";
            lblReadRange.Size = new Size(162, 15);
            lblReadRange.TabIndex = 9;
            lblReadRange.Text = "길이값 사이즈 (1/2/4바이트)";
            // 
            // grpEndianness
            // 
            grpEndianness.Controls.Add(rbBigEndian);
            grpEndianness.Controls.Add(rbLittleEndian);
            grpEndianness.Location = new Point(318, 12);
            grpEndianness.Name = "grpEndianness";
            grpEndianness.Size = new Size(141, 76);
            grpEndianness.TabIndex = 10;
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
            // chkStart
            // 
            chkStart.Appearance = Appearance.Button;
            chkStart.Location = new Point(318, 94);
            chkStart.Name = "chkStart";
            chkStart.Size = new Size(141, 31);
            chkStart.TabIndex = 11;
            chkStart.Text = "패킷 Reading";
            chkStart.UseVisualStyleBackColor = true;
            chkStart.CheckedChanged += chkStart_CheckedChanged;
            // 
            // pnlPacketBytes
            // 
            pnlPacketBytes.BorderStyle = BorderStyle.FixedSingle;
            pnlPacketBytes.Location = new Point(23, 166);
            pnlPacketBytes.Name = "pnlPacketBytes";
            pnlPacketBytes.Size = new Size(436, 56);
            pnlPacketBytes.TabIndex = 12;
            // 
            // lblQueueCount
            // 
            lblQueueCount.AutoSize = true;
            lblQueueCount.Location = new Point(318, 136);
            lblQueueCount.Name = "lblQueueCount";
            lblQueueCount.Size = new Size(115, 15);
            lblQueueCount.TabIndex = 13;
            lblQueueCount.Text = "대기 중인 패킷: _ 개";
            // 
            // ServerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(487, 237);
            Controls.Add(lblQueueCount);
            Controls.Add(pnlPacketBytes);
            Controls.Add(chkStart);
            Controls.Add(grpEndianness);
            Controls.Add(lblReadRange);
            Controls.Add(lblOffset);
            Controls.Add(lblStartByte);
            Controls.Add(lblPort);
            Controls.Add(lblIP);
            Controls.Add(txtReadRange);
            Controls.Add(txtOffset);
            Controls.Add(txtStartByte);
            Controls.Add(txtPort);
            Controls.Add(txtIP);
            Name = "ServerForm";
            Text = "Server";
            grpEndianness.ResumeLayout(false);
            grpEndianness.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtIP;
        private TextBox txtPort;
        private TextBox txtStartByte;
        private TextBox txtOffset;
        private TextBox txtReadRange;
        private Label lblIP;
        private Label lblPort;
        private Label lblStartByte;
        private Label lblOffset;
        private Label lblReadRange;
        private GroupBox grpEndianness;
        private RadioButton rbBigEndian;
        private RadioButton rbLittleEndian;
        private CheckBox chkStart;
        private FlowLayoutPanel pnlPacketBytes;
        private Label lblQueueCount;
    }
}

namespace WinFormsApp_Draft
{
    partial class MainForm
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
            components = new System.ComponentModel.Container();
            MotorPorts = new ListBox();
            SelectedMotorPort = new TextBox();
            MotorBaudRate = new ListBox();
            SelectedMotorBR = new TextBox();
            MotorSerialSwitch = new Button();
            MotorConnection = new TextBox();
            Response = new TextBox();
            SpinTimer = new System.Windows.Forms.Timer(components);
            ResetMotor = new Button();
            label1 = new Label();
            Position = new TextBox();
            Update = new CheckBox();
            ClearPos = new Button();
            label3 = new Label();
            SpinDuration = new TextBox();
            label4 = new Label();
            AccSpeed = new TextBox();
            FreeStop = new Button();
            ForceStop = new Button();
            SendRequest = new Button();
            label2 = new Label();
            SpinSpeed = new TextBox();
            SpinCoater = new GroupBox();
            AutoRead = new CheckBox();
            FilePath = new TextBox();
            AutoRun = new GroupBox();
            SheetName = new TextBox();
            ShowData = new RichTextBox();
            ControlPanel = new Panel();
            OpenArmForm = new Button();
            OpenCoaterForm = new Button();
            SpinCoater.SuspendLayout();
            AutoRun.SuspendLayout();
            SuspendLayout();
            // 
            // MotorPorts
            // 
            MotorPorts.FormattingEnabled = true;
            MotorPorts.ItemHeight = 17;
            MotorPorts.Location = new Point(9, 68);
            MotorPorts.Margin = new Padding(2, 3, 2, 3);
            MotorPorts.Name = "MotorPorts";
            MotorPorts.Size = new Size(96, 55);
            MotorPorts.TabIndex = 0;
            MotorPorts.SelectedIndexChanged += MotorPorts_SelectedIndexChanged;
            // 
            // SelectedMotorPort
            // 
            SelectedMotorPort.Location = new Point(9, 40);
            SelectedMotorPort.Margin = new Padding(2, 3, 2, 3);
            SelectedMotorPort.Name = "SelectedMotorPort";
            SelectedMotorPort.ReadOnly = true;
            SelectedMotorPort.Size = new Size(98, 23);
            SelectedMotorPort.TabIndex = 1;
            // 
            // MotorBaudRate
            // 
            MotorBaudRate.FormattingEnabled = true;
            MotorBaudRate.ItemHeight = 17;
            MotorBaudRate.Location = new Point(123, 68);
            MotorBaudRate.Margin = new Padding(2, 3, 2, 3);
            MotorBaudRate.Name = "MotorBaudRate";
            MotorBaudRate.Size = new Size(98, 55);
            MotorBaudRate.TabIndex = 2;
            MotorBaudRate.SelectedIndexChanged += MotorBaudRate_SelectedIndexChanged;
            // 
            // SelectedMotorBR
            // 
            SelectedMotorBR.Location = new Point(123, 41);
            SelectedMotorBR.Margin = new Padding(2, 3, 2, 3);
            SelectedMotorBR.Name = "SelectedMotorBR";
            SelectedMotorBR.ReadOnly = true;
            SelectedMotorBR.Size = new Size(98, 23);
            SelectedMotorBR.TabIndex = 3;
            // 
            // MotorSerialSwitch
            // 
            MotorSerialSwitch.Location = new Point(9, 10);
            MotorSerialSwitch.Margin = new Padding(2, 3, 2, 3);
            MotorSerialSwitch.Name = "MotorSerialSwitch";
            MotorSerialSwitch.Size = new Size(110, 25);
            MotorSerialSwitch.TabIndex = 6;
            MotorSerialSwitch.Text = "Connect Motor";
            MotorSerialSwitch.UseVisualStyleBackColor = true;
            MotorSerialSwitch.Click += MotorSerialSwitch_Click;
            // 
            // MotorConnection
            // 
            MotorConnection.AccessibleRole = AccessibleRole.WhiteSpace;
            MotorConnection.Location = new Point(128, 11);
            MotorConnection.Margin = new Padding(2, 3, 2, 3);
            MotorConnection.Name = "MotorConnection";
            MotorConnection.ReadOnly = true;
            MotorConnection.Size = new Size(82, 23);
            MotorConnection.TabIndex = 10;
            MotorConnection.Text = "closed";
            // 
            // Response
            // 
            Response.Location = new Point(260, 242);
            Response.Margin = new Padding(2, 3, 2, 3);
            Response.Name = "Response";
            Response.ReadOnly = true;
            Response.Size = new Size(421, 23);
            Response.TabIndex = 17;
            // 
            // ResetMotor
            // 
            ResetMotor.Location = new Point(259, 96);
            ResetMotor.Margin = new Padding(2, 3, 2, 3);
            ResetMotor.Name = "ResetMotor";
            ResetMotor.Size = new Size(73, 43);
            ResetMotor.TabIndex = 18;
            ResetMotor.Text = "reset motor";
            ResetMotor.UseVisualStyleBackColor = true;
            ResetMotor.Click += ResetMotor_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(142, 75);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(104, 17);
            label1.TabIndex = 14;
            label1.Text = "position: degree";
            // 
            // Position
            // 
            Position.Location = new Point(146, 95);
            Position.Margin = new Padding(2, 3, 2, 3);
            Position.Name = "Position";
            Position.ReadOnly = true;
            Position.Size = new Size(100, 23);
            Position.TabIndex = 13;
            // 
            // Update
            // 
            Update.AutoSize = true;
            Update.Location = new Point(166, 118);
            Update.Margin = new Padding(2, 3, 2, 3);
            Update.Name = "Update";
            Update.Size = new Size(68, 21);
            Update.TabIndex = 16;
            Update.Text = "update";
            Update.UseVisualStyleBackColor = true;
            Update.CheckedChanged += UpdatePosition_CheckedChanged;
            // 
            // ClearPos
            // 
            ClearPos.Location = new Point(166, 145);
            ClearPos.Margin = new Padding(2, 3, 2, 3);
            ClearPos.Name = "ClearPos";
            ClearPos.Size = new Size(73, 25);
            ClearPos.TabIndex = 15;
            ClearPos.Text = "clear";
            ClearPos.TextImageRelation = TextImageRelation.ImageAboveText;
            ClearPos.UseVisualStyleBackColor = true;
            ClearPos.Click += ClearPos_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(22, 81);
            label3.Margin = new Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new Size(99, 17);
            label3.TabIndex = 21;
            label3.Text = "spin duration(s)";
            // 
            // SpinDuration
            // 
            SpinDuration.Location = new Point(25, 101);
            SpinDuration.Margin = new Padding(2, 3, 2, 3);
            SpinDuration.Name = "SpinDuration";
            SpinDuration.Size = new Size(96, 23);
            SpinDuration.TabIndex = 20;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(17, 127);
            label4.Margin = new Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new Size(113, 17);
            label4.TabIndex = 23;
            label4.Text = "acc speed(RPM/s)";
            // 
            // AccSpeed
            // 
            AccSpeed.Location = new Point(25, 148);
            AccSpeed.Margin = new Padding(2, 3, 2, 3);
            AccSpeed.Name = "AccSpeed";
            AccSpeed.Size = new Size(96, 23);
            AccSpeed.TabIndex = 22;
            // 
            // FreeStop
            // 
            FreeStop.Location = new Point(294, 47);
            FreeStop.Margin = new Padding(2, 3, 2, 3);
            FreeStop.Name = "FreeStop";
            FreeStop.Size = new Size(73, 25);
            FreeStop.TabIndex = 12;
            FreeStop.Text = "free stop";
            FreeStop.UseVisualStyleBackColor = true;
            FreeStop.Click += FreeStop_Click;
            // 
            // ForceStop
            // 
            ForceStop.Location = new Point(205, 45);
            ForceStop.Margin = new Padding(2, 3, 2, 3);
            ForceStop.Name = "ForceStop";
            ForceStop.Size = new Size(85, 27);
            ForceStop.TabIndex = 11;
            ForceStop.Text = "force stop";
            ForceStop.UseVisualStyleBackColor = true;
            ForceStop.Click += ForceStop_Click;
            // 
            // SendRequest
            // 
            SendRequest.Location = new Point(128, 47);
            SendRequest.Margin = new Padding(2, 3, 2, 3);
            SendRequest.Name = "SendRequest";
            SendRequest.Size = new Size(73, 25);
            SendRequest.TabIndex = 9;
            SendRequest.Text = "send";
            SendRequest.UseVisualStyleBackColor = true;
            SendRequest.Click += SendRequest_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(17, 30);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(107, 17);
            label2.TabIndex = 19;
            label2.Text = "spin speed(RPM)";
            // 
            // SpinSpeed
            // 
            SpinSpeed.Location = new Point(25, 50);
            SpinSpeed.Margin = new Padding(2, 3, 2, 3);
            SpinSpeed.Name = "SpinSpeed";
            SpinSpeed.Size = new Size(98, 23);
            SpinSpeed.TabIndex = 25;
            // 
            // SpinCoater
            // 
            SpinCoater.Controls.Add(SpinSpeed);
            SpinCoater.Controls.Add(label2);
            SpinCoater.Controls.Add(SendRequest);
            SpinCoater.Controls.Add(ForceStop);
            SpinCoater.Controls.Add(FreeStop);
            SpinCoater.Controls.Add(AccSpeed);
            SpinCoater.Controls.Add(label4);
            SpinCoater.Controls.Add(SpinDuration);
            SpinCoater.Controls.Add(label3);
            SpinCoater.Controls.Add(ClearPos);
            SpinCoater.Controls.Add(Update);
            SpinCoater.Controls.Add(Position);
            SpinCoater.Controls.Add(label1);
            SpinCoater.Controls.Add(ResetMotor);
            SpinCoater.Location = new Point(314, 10);
            SpinCoater.Name = "SpinCoater";
            SpinCoater.Size = new Size(385, 188);
            SpinCoater.TabIndex = 46;
            SpinCoater.TabStop = false;
            SpinCoater.Text = "Spin Coater";
            // 
            // AutoRead
            // 
            AutoRead.AutoSize = true;
            AutoRead.Location = new Point(109, 22);
            AutoRead.Name = "AutoRead";
            AutoRead.Size = new Size(84, 21);
            AutoRead.TabIndex = 47;
            AutoRead.Text = "auto read";
            AutoRead.UseVisualStyleBackColor = true;
            AutoRead.CheckedChanged += AutoRead_CheckedChanged;
            // 
            // FilePath
            // 
            FilePath.Location = new Point(66, 49);
            FilePath.Name = "FilePath";
            FilePath.Size = new Size(174, 23);
            FilePath.TabIndex = 48;
            // 
            // AutoRun
            // 
            AutoRun.Controls.Add(SheetName);
            AutoRun.Controls.Add(ShowData);
            AutoRun.Controls.Add(FilePath);
            AutoRun.Controls.Add(AutoRead);
            AutoRun.Location = new Point(542, 290);
            AutoRun.Name = "AutoRun";
            AutoRun.Size = new Size(322, 274);
            AutoRun.TabIndex = 49;
            AutoRun.TabStop = false;
            AutoRun.Text = "Auto";
            // 
            // SheetName
            // 
            SheetName.Location = new Point(66, 86);
            SheetName.Name = "SheetName";
            SheetName.Size = new Size(174, 23);
            SheetName.TabIndex = 50;
            // 
            // ShowData
            // 
            ShowData.Location = new Point(59, 141);
            ShowData.Name = "ShowData";
            ShowData.Size = new Size(200, 104);
            ShowData.TabIndex = 49;
            ShowData.Text = "";
            // 
            // ControlPanel
            // 
            ControlPanel.BackColor = SystemColors.AppWorkspace;
            ControlPanel.Location = new Point(870, 45);
            ControlPanel.Name = "ControlPanel";
            ControlPanel.Size = new Size(575, 689);
            ControlPanel.TabIndex = 50;
            // 
            // OpenArmForm
            // 
            OpenArmForm.Location = new Point(977, 10);
            OpenArmForm.Name = "OpenArmForm";
            OpenArmForm.Size = new Size(75, 23);
            OpenArmForm.TabIndex = 51;
            OpenArmForm.Text = "arm";
            OpenArmForm.UseVisualStyleBackColor = true;
            OpenArmForm.Click += OpenArmForm_Click;
            // 
            // OpenCoaterForm
            // 
            OpenCoaterForm.Location = new Point(1058, 10);
            OpenCoaterForm.Name = "OpenCoaterForm";
            OpenCoaterForm.Size = new Size(75, 23);
            OpenCoaterForm.TabIndex = 52;
            OpenCoaterForm.Text = "coater";
            OpenCoaterForm.UseVisualStyleBackColor = true;
            OpenCoaterForm.Click += OpenCoaterForm_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1476, 758);
            Controls.Add(OpenCoaterForm);
            Controls.Add(OpenArmForm);
            Controls.Add(ControlPanel);
            Controls.Add(AutoRun);
            Controls.Add(SpinCoater);
            Controls.Add(Response);
            Controls.Add(MotorConnection);
            Controls.Add(MotorSerialSwitch);
            Controls.Add(SelectedMotorBR);
            Controls.Add(MotorBaudRate);
            Controls.Add(SelectedMotorPort);
            Controls.Add(MotorPorts);
            Margin = new Padding(2, 3, 2, 3);
            Name = "MainForm";
            Text = "Form1";
            //FormClosed += MainForm_FormClosed;
            Load += Form1_Load;
            SpinCoater.ResumeLayout(false);
            SpinCoater.PerformLayout();
            AutoRun.ResumeLayout(false);
            AutoRun.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListBox MotorPorts;
        private TextBox SelectedMotorPort;
        private ListBox MotorBaudRate;
        private TextBox SelectedMotorBR;
        private Button MotorSerialSwitch;
        private TextBox MotorConnection;
        private TextBox Response;
        private System.Windows.Forms.Timer SpinTimer;
        private Button ResetMotor;
        private Label label1;
        private TextBox Position;
        private CheckBox Update;
        private Button ClearPos;
        private Label label3;
        private TextBox SpinDuration;
        private Label label4;
        private TextBox AccSpeed;
        private Button FreeStop;
        private Button ForceStop;
        private Button SendRequest;
        private Label label2;
        private TextBox SpinSpeed;
        private GroupBox SpinCoater;
        private CheckBox AutoRead;
        private TextBox FilePath;
        private GroupBox AutoRun;
        private RichTextBox ShowData;
        private TextBox SheetName;
        private Panel ControlPanel;
        private Button OpenArmForm;
        private Button OpenCoaterForm;
    }
}

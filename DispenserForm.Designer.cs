namespace WinFormsApp_Draft
{
    partial class DispenserForm
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
            DispenserSerialSwitch = new Button();
            Response = new TextBox();
            SuspendLayout();
            // 
            // DispenserSerialSwitch
            // 
            DispenserSerialSwitch.Location = new Point(85, 69);
            DispenserSerialSwitch.Name = "DispenserSerialSwitch";
            DispenserSerialSwitch.Size = new Size(135, 23);
            DispenserSerialSwitch.TabIndex = 0;
            DispenserSerialSwitch.Text = "connect dispenser";
            DispenserSerialSwitch.UseVisualStyleBackColor = true;
            DispenserSerialSwitch.Click += DispenserSerialSwitch_Click;
            // 
            // Response
            // 
            Response.Location = new Point(65, 336);
            Response.Name = "Response";
            Response.ReadOnly = true;
            Response.Size = new Size(269, 23);
            Response.TabIndex = 1;
            // 
            // DispenserForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(714, 524);
            Controls.Add(Response);
            Controls.Add(DispenserSerialSwitch);
            Name = "DispenserForm";
            Text = "DispenserForm";
            Load += DispenserForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button DispenserSerialSwitch;
        private TextBox Response;
    }
}
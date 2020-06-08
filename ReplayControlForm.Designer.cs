namespace EDVRHUD
{
    partial class ReplayControlForm
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
            this.btnStartStop = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbRate = new System.Windows.Forms.TrackBar();
            this.lblRate = new System.Windows.Forms.Label();
            this.lblTimestamp = new System.Windows.Forms.Label();
            this.chkAutoPause = new System.Windows.Forms.CheckBox();
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.dtpStartTime = new System.Windows.Forms.DateTimePicker();
            ((System.ComponentModel.ISupportInitialize)(this.tbRate)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStartStop
            // 
            this.btnStartStop.Location = new System.Drawing.Point(352, 9);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(52, 25);
            this.btnStartStop.TabIndex = 0;
            this.btnStartStop.Text = "Start";
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Replay Rate";
            // 
            // tbRate
            // 
            this.tbRate.LargeChange = 100;
            this.tbRate.Location = new System.Drawing.Point(74, 38);
            this.tbRate.Maximum = 1000;
            this.tbRate.Minimum = 1;
            this.tbRate.Name = "tbRate";
            this.tbRate.Size = new System.Drawing.Size(269, 45);
            this.tbRate.SmallChange = 10;
            this.tbRate.TabIndex = 3;
            this.tbRate.TickFrequency = 10;
            this.tbRate.Value = 1;
            this.tbRate.ValueChanged += new System.EventHandler(this.tbRate_ValueChanged);
            // 
            // lblRate
            // 
            this.lblRate.AutoSize = true;
            this.lblRate.Location = new System.Drawing.Point(349, 43);
            this.lblRate.Name = "lblRate";
            this.lblRate.Size = new System.Drawing.Size(20, 13);
            this.lblRate.TabIndex = 4;
            this.lblRate.Text = "X1";
            // 
            // lblTimestamp
            // 
            this.lblTimestamp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblTimestamp.Location = new System.Drawing.Point(183, 10);
            this.lblTimestamp.Name = "lblTimestamp";
            this.lblTimestamp.Size = new System.Drawing.Size(163, 23);
            this.lblTimestamp.TabIndex = 5;
            this.lblTimestamp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chkAutoPause
            // 
            this.chkAutoPause.AutoSize = true;
            this.chkAutoPause.Location = new System.Drawing.Point(2, 87);
            this.chkAutoPause.Name = "chkAutoPause";
            this.chkAutoPause.Size = new System.Drawing.Size(153, 17);
            this.chkAutoPause.TabIndex = 6;
            this.chkAutoPause.Text = "Pause on next Star System";
            this.chkAutoPause.UseVisualStyleBackColor = true;
            this.chkAutoPause.CheckedChanged += new System.EventHandler(this.chkAutoPause_CheckedChanged);
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.CustomFormat = "yyyy-MM-dd";
            this.dtpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStartDate.Location = new System.Drawing.Point(2, 12);
            this.dtpStartDate.MaxDate = new System.DateTime(2100, 12, 31, 0, 0, 0, 0);
            this.dtpStartDate.MinDate = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.Size = new System.Drawing.Size(93, 20);
            this.dtpStartDate.TabIndex = 7;
            this.dtpStartDate.Value = new System.DateTime(2020, 1, 1, 0, 0, 0, 0);
            this.dtpStartDate.ValueChanged += new System.EventHandler(this.dtpStartDate_ValueChanged);
            // 
            // dtpStartTime
            // 
            this.dtpStartTime.CustomFormat = "HH:mm:ss";
            this.dtpStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtpStartTime.Location = new System.Drawing.Point(101, 12);
            this.dtpStartTime.MaxDate = new System.DateTime(2100, 12, 31, 0, 0, 0, 0);
            this.dtpStartTime.MinDate = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            this.dtpStartTime.Name = "dtpStartTime";
            this.dtpStartTime.ShowUpDown = true;
            this.dtpStartTime.Size = new System.Drawing.Size(76, 20);
            this.dtpStartTime.TabIndex = 8;
            this.dtpStartTime.Value = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            this.dtpStartTime.ValueChanged += new System.EventHandler(this.dtpStartTime_ValueChanged);
            // 
            // ReplayControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(410, 116);
            this.Controls.Add(this.dtpStartTime);
            this.Controls.Add(this.dtpStartDate);
            this.Controls.Add(this.chkAutoPause);
            this.Controls.Add(this.lblTimestamp);
            this.Controls.Add(this.lblRate);
            this.Controls.Add(this.tbRate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnStartStop);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ReplayControlForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Journal Replay Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ReplayControlForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.tbRate)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar tbRate;
        private System.Windows.Forms.Label lblRate;
        private System.Windows.Forms.Label lblTimestamp;
        private System.Windows.Forms.CheckBox chkAutoPause;
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.DateTimePicker dtpStartTime;
    }
}
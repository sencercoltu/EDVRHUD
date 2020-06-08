namespace EDVRHUD
{
    partial class SettingsForm
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
            this.btnApply = new System.Windows.Forms.Button();
            this.chkDisableVR = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkSignals = new System.Windows.Forms.CheckBox();
            this.chkNBS = new System.Windows.Forms.CheckBox();
            this.chkDSI = new System.Windows.Forms.CheckBox();
            this.cmbVoices = new System.Windows.Forms.ComboBox();
            this.chkVoiceEnable = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbVolume = new System.Windows.Forms.TrackBar();
            this.tbRate = new System.Windows.Forms.TrackBar();
            this.chkADS = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cmbScrollDown = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbScrollUp = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkAutoDethrottle = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbVolume)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRate)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(248, 306);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(75, 23);
            this.btnApply.TabIndex = 6;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // chkDisableVR
            // 
            this.chkDisableVR.AutoSize = true;
            this.chkDisableVR.Location = new System.Drawing.Point(7, 310);
            this.chkDisableVR.Name = "chkDisableVR";
            this.chkDisableVR.Size = new System.Drawing.Size(162, 17);
            this.chkDisableVR.TabIndex = 10;
            this.chkDisableVR.Text = "Disable VR (Requires restart)";
            this.chkDisableVR.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkSignals);
            this.groupBox1.Controls.Add(this.chkNBS);
            this.groupBox1.Controls.Add(this.chkDSI);
            this.groupBox1.Controls.Add(this.cmbVoices);
            this.groupBox1.Controls.Add(this.chkVoiceEnable);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.tbVolume);
            this.groupBox1.Controls.Add(this.tbRate);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(316, 162);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Voice";
            // 
            // chkSignals
            // 
            this.chkSignals.AutoSize = true;
            this.chkSignals.Location = new System.Drawing.Point(6, 129);
            this.chkSignals.Name = "chkSignals";
            this.chkSignals.Size = new System.Drawing.Size(107, 17);
            this.chkSignals.TabIndex = 15;
            this.chkSignals.Text = "Detected Signals";
            this.chkSignals.UseVisualStyleBackColor = true;
            // 
            // chkNBS
            // 
            this.chkNBS.AutoSize = true;
            this.chkNBS.Location = new System.Drawing.Point(168, 106);
            this.chkNBS.Name = "chkNBS";
            this.chkNBS.Size = new System.Drawing.Size(136, 17);
            this.chkNBS.TabIndex = 14;
            this.chkNBS.Text = "EDSM Nearby Systems";
            this.chkNBS.UseVisualStyleBackColor = true;
            // 
            // chkDSI
            // 
            this.chkDSI.AutoSize = true;
            this.chkDSI.Location = new System.Drawing.Point(6, 106);
            this.chkDSI.Name = "chkDSI";
            this.chkDSI.Size = new System.Drawing.Size(134, 17);
            this.chkDSI.TabIndex = 13;
            this.chkDSI.Text = "EDSM Dst System Info";
            this.chkDSI.UseVisualStyleBackColor = true;
            // 
            // cmbVoices
            // 
            this.cmbVoices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbVoices.FormattingEnabled = true;
            this.cmbVoices.Location = new System.Drawing.Point(129, 17);
            this.cmbVoices.Name = "cmbVoices";
            this.cmbVoices.Size = new System.Drawing.Size(175, 21);
            this.cmbVoices.TabIndex = 7;
            // 
            // chkVoiceEnable
            // 
            this.chkVoiceEnable.AutoSize = true;
            this.chkVoiceEnable.Location = new System.Drawing.Point(6, 19);
            this.chkVoiceEnable.Name = "chkVoiceEnable";
            this.chkVoiceEnable.Size = new System.Drawing.Size(95, 17);
            this.chkVoiceEnable.TabIndex = 6;
            this.chkVoiceEnable.Text = "Synth Enabled";
            this.chkVoiceEnable.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Volume";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Rate";
            // 
            // tbVolume
            // 
            this.tbVolume.Location = new System.Drawing.Point(42, 73);
            this.tbVolume.Maximum = 100;
            this.tbVolume.Name = "tbVolume";
            this.tbVolume.Size = new System.Drawing.Size(269, 45);
            this.tbVolume.TabIndex = 10;
            // 
            // tbRate
            // 
            this.tbRate.Location = new System.Drawing.Point(42, 42);
            this.tbRate.Minimum = -10;
            this.tbRate.Name = "tbRate";
            this.tbRate.Size = new System.Drawing.Size(269, 45);
            this.tbRate.TabIndex = 8;
            // 
            // chkADS
            // 
            this.chkADS.AutoSize = true;
            this.chkADS.Location = new System.Drawing.Point(7, 287);
            this.chkADS.Name = "chkADS";
            this.chkADS.Size = new System.Drawing.Size(172, 17);
            this.chkADS.TabIndex = 12;
            this.chkADS.Text = "Auto Discovery Scan on arrival";
            this.chkADS.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cmbScrollDown);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.cmbScrollUp);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(13, 181);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(315, 100);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Interaction";
            // 
            // cmbScrollDown
            // 
            this.cmbScrollDown.DisplayMember = "Display";
            this.cmbScrollDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScrollDown.DropDownWidth = 500;
            this.cmbScrollDown.FormattingEnabled = true;
            this.cmbScrollDown.Location = new System.Drawing.Point(73, 48);
            this.cmbScrollDown.Name = "cmbScrollDown";
            this.cmbScrollDown.Size = new System.Drawing.Size(230, 21);
            this.cmbScrollDown.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Scroll Down";
            // 
            // cmbScrollUp
            // 
            this.cmbScrollUp.DisplayMember = "Display";
            this.cmbScrollUp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScrollUp.DropDownWidth = 500;
            this.cmbScrollUp.FormattingEnabled = true;
            this.cmbScrollUp.Location = new System.Drawing.Point(73, 22);
            this.cmbScrollUp.Name = "cmbScrollUp";
            this.cmbScrollUp.Size = new System.Drawing.Size(230, 21);
            this.cmbScrollUp.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Scroll Up";
            // 
            // chkAutoDethrottle
            // 
            this.chkAutoDethrottle.AutoSize = true;
            this.chkAutoDethrottle.Location = new System.Drawing.Point(228, 287);
            this.chkAutoDethrottle.Name = "chkAutoDethrottle";
            this.chkAutoDethrottle.Size = new System.Drawing.Size(95, 17);
            this.chkAutoDethrottle.TabIndex = 14;
            this.chkAutoDethrottle.Text = "Auto dethrottle";
            this.chkAutoDethrottle.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(337, 363);
            this.Controls.Add(this.chkAutoDethrottle);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.chkADS);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.chkDisableVR);
            this.Controls.Add(this.btnApply);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EDVRHUD Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbVolume)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRate)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.CheckBox chkDisableVR;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cmbVoices;
        private System.Windows.Forms.CheckBox chkVoiceEnable;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar tbVolume;
        private System.Windows.Forms.TrackBar tbRate;
        private System.Windows.Forms.CheckBox chkADS;
        private System.Windows.Forms.CheckBox chkDSI;
        private System.Windows.Forms.CheckBox chkNBS;
        private System.Windows.Forms.CheckBox chkSignals;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbScrollUp;
        private System.Windows.Forms.ComboBox cmbScrollDown;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkAutoDethrottle;
    }
}
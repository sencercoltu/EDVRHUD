﻿namespace EDVRHUD
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
            this.chkVoiceEnable = new System.Windows.Forms.CheckBox();
            this.cmbVoices = new System.Windows.Forms.ComboBox();
            this.tbRate = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbVolume = new System.Windows.Forms.TrackBar();
            this.btnApply = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tbRate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbVolume)).BeginInit();
            this.SuspendLayout();
            // 
            // chkVoiceEnable
            // 
            this.chkVoiceEnable.AutoSize = true;
            this.chkVoiceEnable.Location = new System.Drawing.Point(13, 13);
            this.chkVoiceEnable.Name = "chkVoiceEnable";
            this.chkVoiceEnable.Size = new System.Drawing.Size(125, 17);
            this.chkVoiceEnable.TabIndex = 0;
            this.chkVoiceEnable.Text = "Voice Synth Enabled";
            this.chkVoiceEnable.UseVisualStyleBackColor = true;
            this.chkVoiceEnable.CheckedChanged += new System.EventHandler(this.chkVoiceEnable_CheckedChanged);
            // 
            // cmbVoices
            // 
            this.cmbVoices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbVoices.FormattingEnabled = true;
            this.cmbVoices.Location = new System.Drawing.Point(144, 11);
            this.cmbVoices.Name = "cmbVoices";
            this.cmbVoices.Size = new System.Drawing.Size(176, 21);
            this.cmbVoices.TabIndex = 1;
            // 
            // tbRate
            // 
            this.tbRate.Location = new System.Drawing.Point(49, 36);
            this.tbRate.Minimum = -10;
            this.tbRate.Name = "tbRate";
            this.tbRate.Size = new System.Drawing.Size(271, 45);
            this.tbRate.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Rate";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Volume";
            // 
            // tbVolume
            // 
            this.tbVolume.Location = new System.Drawing.Point(49, 78);
            this.tbVolume.Maximum = 100;
            this.tbVolume.Name = "tbVolume";
            this.tbVolume.Size = new System.Drawing.Size(271, 45);
            this.tbVolume.TabIndex = 4;
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(245, 134);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(75, 23);
            this.btnApply.TabIndex = 6;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(332, 169);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.tbVolume);
            this.Controls.Add(this.tbRate);
            this.Controls.Add(this.cmbVoices);
            this.Controls.Add(this.chkVoiceEnable);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EDVRHUD Settings";
            ((System.ComponentModel.ISupportInitialize)(this.tbRate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbVolume)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkVoiceEnable;
        private System.Windows.Forms.ComboBox cmbVoices;
        private System.Windows.Forms.TrackBar tbRate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar tbVolume;
        private System.Windows.Forms.Button btnApply;
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EDVRHUD
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            chkVoiceEnable.Checked = NotificationApp.Settings.VoiceEnable;
            cmbVoices.Items.AddRange(NotificationApp.Speech.GetInstalledVoices().Select(v => v.VoiceInfo.Name).ToArray());
            cmbVoices.SelectedItem = NotificationApp.Settings.Voice;
            tbRate.Value = NotificationApp.Settings.VoiceRate;
            tbVolume.Value = NotificationApp.Settings.VoiceVolume;
        }

        private void chkVoiceEnable_CheckedChanged(object sender, EventArgs e)
        {
            cmbVoices.Enabled = chkVoiceEnable.Checked;
            tbRate.Enabled = chkVoiceEnable.Checked;
            tbVolume.Enabled = chkVoiceEnable.Checked;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            NotificationApp.Settings.VoiceRate = tbRate.Value;
            NotificationApp.Settings.VoiceVolume = tbVolume.Value;
            NotificationApp.Settings.Voice = cmbVoices.SelectedItem.ToString();
            NotificationApp.Settings.VoiceEnable = chkVoiceEnable.Checked;
            NotificationApp.SaveSettings();
        }
    }
}

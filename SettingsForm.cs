using System;
using System.Data;
using System.Linq;
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
            chkDisableVR.Checked = !NotificationApp.Settings.UseOpenVR;
            chkADS.Checked = NotificationApp.Settings.AutoDiscoveryScan;
            chkNBS.Checked = NotificationApp.Settings.EDSMNearbySystems;
            chkDSI.Checked = NotificationApp.Settings.EDSMDestinationSystem;
            chkSignals.Checked = NotificationApp.Settings.Signals;
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
            NotificationApp.Settings.UseOpenVR = !chkDisableVR.Checked;
            NotificationApp.Settings.AutoDiscoveryScan = chkADS.Checked;
            NotificationApp.Settings.EDSMNearbySystems = chkNBS.Checked;
            NotificationApp.Settings.EDSMDestinationSystem = chkDSI.Checked;
            NotificationApp.Settings.Signals = chkSignals.Checked;
            NotificationApp.SaveSettings();
        }
    }
}

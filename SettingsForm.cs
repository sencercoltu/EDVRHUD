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
            tbJournalReplay.Value = NotificationApp.Settings.JournalReplaySpeed;
            chkFSDBreak.Checked = NotificationApp.Settings.BreakOnFSDJump;
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
            NotificationApp.Settings.JournalReplaySpeed = tbJournalReplay.Value;
            NotificationApp.Settings.BreakOnFSDJump = chkFSDBreak.Checked;
            NotificationApp.SaveSettings();
        }
    }
}

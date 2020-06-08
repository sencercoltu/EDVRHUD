using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Management.Instrumentation;
using System.Windows.Forms;

namespace EDVRHUD
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();

            var devices = EDCommon.DirectInput.GetDevices().Where(d => d.Type == SharpDX.DirectInput.DeviceType.FirstPerson || d.Type == SharpDX.DirectInput.DeviceType.Joystick || d.Type == SharpDX.DirectInput.DeviceType.Flight).ToList();            
            var itemList = new List<JoystickMapping>() { new JoystickMapping() };
            foreach (var device in devices)
            {
                var joy = new Joystick(EDCommon.DirectInput, device.InstanceGuid);

                foreach (var obj in joy.GetObjects())
                {
                    foreach (JoystickOffset e in Enum.GetValues(typeof(JoystickOffset)))
                        itemList.Add(new JoystickMapping()
                        {
                            InstanceName = joy.Information.InstanceName,
                            InstanceGuid = joy.Information.InstanceGuid,
                            ObjectName = e.ToString(),
                            ObjectOffset = (int)e
                        });
                }
            }
            cmbScrollUp.Items.AddRange(itemList.ToArray());
            cmbScrollDown.Items.AddRange(itemList.ToArray());


            chkVoiceEnable.Checked = EDCommon.Settings.VoiceEnable;
            cmbVoices.Items.AddRange(EDCommon.Speech.GetInstalledVoices().Select(v => v.VoiceInfo.Name).ToArray());
            cmbVoices.SelectedItem = EDCommon.Settings.Voice;
            tbRate.Value = EDCommon.Settings.VoiceRate;
            tbVolume.Value = EDCommon.Settings.VoiceVolume;
            chkDisableVR.Checked = !EDCommon.Settings.UseOpenVR;
            chkADS.Checked = EDCommon.Settings.AutoDiscoveryScan;
            chkNBS.Checked = EDCommon.Settings.EDSMNearbySystems;
            chkDSI.Checked = EDCommon.Settings.EDSMDestinationSystem;
            chkSignals.Checked = EDCommon.Settings.Signals;

            var idx = cmbScrollDown.FindStringExact(EDCommon.Settings.ScrollDown.Display);
            if (idx < 1) idx = 0; cmbScrollDown.SelectedIndex = idx;
            idx = cmbScrollUp.FindStringExact(EDCommon.Settings.ScrollUp.Display);
            if (idx < 1) idx = 0; cmbScrollUp.SelectedIndex = idx;

        }

        private void chkVoiceEnable_CheckedChanged(object sender, EventArgs e)
        {
            cmbVoices.Enabled = chkVoiceEnable.Checked;
            tbRate.Enabled = chkVoiceEnable.Checked;
            tbVolume.Enabled = chkVoiceEnable.Checked;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            EDCommon.Settings.VoiceRate = tbRate.Value;
            EDCommon.Settings.VoiceVolume = tbVolume.Value;
            EDCommon.Settings.Voice = cmbVoices.SelectedItem.ToString();
            EDCommon.Settings.VoiceEnable = chkVoiceEnable.Checked;
            EDCommon.Settings.UseOpenVR = !chkDisableVR.Checked;
            EDCommon.Settings.AutoDiscoveryScan = chkADS.Checked;
            EDCommon.Settings.EDSMNearbySystems = chkNBS.Checked;
            EDCommon.Settings.EDSMDestinationSystem = chkDSI.Checked;
            EDCommon.Settings.Signals = chkSignals.Checked;
            EDCommon.Settings.ScrollUp = cmbScrollUp.SelectedItem as JoystickMapping;
            EDCommon.Settings.ScrollDown = cmbScrollDown.SelectedItem as JoystickMapping;
            EDCommon.SaveSettings();
        }

    }
}

﻿using EDVRHUD.HUDs;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Valve.VR;

namespace EDVRHUD
{
    class NotificationApp : ApplicationContext
    {
        public class HudSettings
        {
            public bool VoiceEnable { get; set; } = true;
            public string Voice { get; set; } = "";
            public int VoiceRate { get; set; } = 4; //-10 to 10
            public int VoiceVolume { get; set; } = 100; //0 to 100
        }

        public static HudSettings Settings { get; set; } = new HudSettings();

        internal static SpeechSynthesizer Speech = new SpeechSynthesizer();

        public static void Talk(string s, bool async = true)
        {
            if (Settings.VoiceEnable)
            {
                if (async) Speech.SpeakAsync(s);
                else Speech.Speak(s);
            }
        }

        public static void Shutup()
        {
            Speech.SpeakAsyncCancelAll();
        }

        private NotifyIcon TrayIcon { get; set; }
        public static bool AppRunning { get; private set; } = true;
        private static JavaScriptSerializer Serializer { get; } = new JavaScriptSerializer();
        public static Font EDFont { get; private set; }

        public static SolidBrush DefaultBrush = new SolidBrush(Color.FromArgb(245, 178, 9));
        public static Color DefaultClearColor = Color.FromArgb(0, 0, 0, 0); // 0 olacak hepsi

        public static DeviceContext D3DDeviceContext { get; set; }
        public static SharpDX.Direct3D11.Device D3DDevice { get; set; }

        private List<HudPanel> HudPanels { get; } = new List<HudPanel>();

        private MenuItem PanelsMenu;
        private MenuItem SettingsMenu;

        public NotificationApp()
        {
            PanelsMenu = new MenuItem("Panels");
            SettingsMenu = new MenuItem("Settings...");
            SettingsMenu.Click += SettingsMenu_Click;
            TrayIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.Icon,
                ContextMenu = new ContextMenu(
                    new MenuItem[]
                    {
                        new MenuItem("OVRHUD") {Enabled = false },
                        new MenuItem("-"),
                        new MenuItem("Load HUD", LoadPanels),
                        new MenuItem("Save HUD", SavePanels),
                        new MenuItem("-"),
                        PanelsMenu,
                        new MenuItem("-"),
                        SettingsMenu,
                        new MenuItem("Exit", Exit)
                    }),
                Visible = true
            };
            AppRunning = true;
        }


        public static SettingsForm SettingsForm { get; set; } = null;

        private void SettingsMenu_Click(object sender, EventArgs e)
        {
            if (SettingsForm != null)
            {
                SettingsForm.Show();
                SettingsForm.Focus();
                return;
            }
            SettingsForm = new SettingsForm
            {
                Icon = Properties.Resources.Icon
            };
            SettingsForm.FormClosing += (s, a) =>
            {
                SettingsForm = null;
            };

            SettingsForm.Show();
        }

        public static void SaveSettings()
        {
            var s = Serializer.Serialize(Settings);
            var path = Environment.CurrentDirectory + "\\Settings.json";
            File.WriteAllText(path, s);
            ApplySettings();
        }

        private static void LoadSettings()
        {
            var path = Environment.CurrentDirectory + "\\Settings.json";
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                Settings = Serializer.Deserialize<HudSettings>(content);
            }
            if (string.IsNullOrEmpty(Settings.Voice))
            {
                var voice = Speech.GetInstalledVoices().FirstOrDefault(s => s.VoiceInfo.Gender == VoiceGender.Female);
                if (voice != null)
                    Settings.Voice = voice.VoiceInfo.Name;
            }
            ApplySettings();
        }

        private static void ApplySettings()
        {
            Speech.SelectVoice(Settings.Voice);
            Speech.Rate = Settings.VoiceRate;
            Speech.Volume = Settings.VoiceVolume;
        }

        private string CommanderName { get; set; }

        private void SavePanels(object sender, EventArgs e)
        {
            var saveData = new PanelSaveData();

            var list = new List<PanelSettings>();
            foreach (var panel in HudPanels)
            {
                list.Add(new PanelSettings()
                {
                    Height = panel.PanelSize.Height,
                    Width = panel.PanelSize.Width,
                    Type = panel.Type,
                    Position = panel.OverlayPosition
                });
            }
            saveData.panels = list.ToArray();

            var s = Serializer.Serialize(saveData);
            var path = Environment.CurrentDirectory + "\\Panels.json";
            File.WriteAllText(path, s);
        }

        private void LoadPanels(object sender, EventArgs e)
        {
            foreach (var panel in HudPanels)
            {
                panel.Dispose();
            }
            HudPanels.Clear();

            var path = Environment.CurrentDirectory + "\\Panels.json";
            var s = File.ReadAllText(path);
            var json = Serializer.Deserialize<PanelSaveData>(s);

            foreach (var panelData in json.panels)
            {
                var panel = HudPanel.Create(panelData);
                HudPanels.Add(panel);
                panel.Initialize();
                PanelsMenu.MenuItems.Add(new MenuItem(panel.Name, OnShowPanel) { Tag = panel });
            }
        }

        private void OnShowPanel(object sender, EventArgs e)
        {
            var panel = (sender as MenuItem).Tag as HudPanel;
            panel.ShowPreview();
        }

        private void Exit(object sender, EventArgs e)
        {
            TrayIcon.Visible = false;
            AppRunning = false;
            Application.Exit();
        }

        private HmdMatrix34_t DefaultOverlayPosition = new HmdMatrix34_t()
        {
            m0 = 1f,
            m1 = 0,
            m2 = 0,
            m3 = 0,
            m4 = 0,
            m5 = 1f,
            m6 = 0,
            m7 = 0f,
            m8 = 0,
            m9 = 0,
            m10 = 1f,
            m11 = -0.5f
        };

        public static bool SimulateNextSystem = false;

        public void Run()
        {
            LoadSettings();
            using (var fontCollection = new PrivateFontCollection())
            {
                unsafe
                {
                    fixed (byte* pFontData = Properties.Resources.EUROCAPS)
                    {
                        fontCollection.AddMemoryFont((IntPtr)pFontData, Properties.Resources.EUROCAPS.Length);
                    }
                }


                var family = fontCollection.Families.FirstOrDefault(f => f.Name.Equals("Euro Caps", StringComparison.InvariantCultureIgnoreCase));
                using (EDFont = new Font(family, 20))
                {
                    int adapterIndex = 0;

#if UseOpenVR
                    var refreshRate = 90;
                    var err = EVRInitError.None;

                    var vrSystem = OpenVR.Init(ref err, EVRApplicationType.VRApplication_Overlay);
                    if (err != EVRInitError.None || OpenVR.Compositor == null || OpenVR.Overlay == null)
                    {
                        AppRunning = false;
                        return;
                    }

                    var terr = ETrackedPropertyError.TrackedProp_Success;
                    refreshRate = (int)OpenVR.System.GetFloatTrackedDeviceProperty(0, ETrackedDeviceProperty.Prop_DisplayFrequency_Float, ref terr);

                    OpenVR.System.GetDXGIOutputInfo(ref adapterIndex);

                    var vrEvent = new VREvent_t();
                    var vrEventSize = (uint)Marshal.SizeOf(vrEvent);

#endif //UseOpenVR
                    using (var factory = new Factory4())
                    {
                        var adapter = factory.GetAdapter(adapterIndex);
                        D3DDevice = new SharpDX.Direct3D11.Device(adapter, DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug);
                        D3DDeviceContext = D3DDevice.ImmediateContext;

                        LoadPanels(null, null);

                        var journalPath = GetSavedGamesPath() + "\\Frontier Developments\\Elite Dangerous";

                        var journalReplayThread = new Thread(() =>
                        {
                            //var usedEvents = new[] { "StarClass", "JetConeBoost", "StartJump", "FSDTarget", "FSDJump", "DiscoveryScan", "FSSSignalDiscovered", "FSSDiscoveryScan", "FSSAllBodiesFound", "Scan" };

                            var journals = Directory.EnumerateFiles(journalPath, "Journal.????????????.??.log").OrderBy(f => f).ToList();
                            journals = journals.Skip(Math.Max(0, journals.Count() - 5)).ToList();
                            foreach (var item in journals)
                            {
                                if (!AppRunning)
                                    break;
                                var content = File.ReadAllText(item);
                                var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                                foreach (var line in lines)
                                {
                                    if (string.IsNullOrEmpty(line))
                                        continue;

                                    //if (!usedEvents.Any(e => line.Contains("\"" + e + "\"")))
                                    //    continue;

                                    if (line.Contains("\"event\":\"FSDJump\","))
                                    {
                                        while (true)
                                        {
                                            if (!AppRunning)
                                                return;
                                            if (SimulateNextSystem)
                                            {
                                                SimulateNextSystem = false;
                                                break;
                                            }
                                        }
                                        Thread.Sleep(100);
                                    }

                                    lock (ContentLock)
                                        CachedContent += line + Environment.NewLine;
                                    if (!AppRunning)
                                        break;
                                }
                            }
                        })
                        { IsBackground = true };
                        //journalReplayThread.Start();

                        var logWatcher = new FileSystemWatcher(journalPath, "Journal.????????????.??.log")
                        {
                            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName
                        };
                        logWatcher.Changed += JournalChanged;
                        logWatcher.Created += JournalChanged;

                        if (!journalReplayThread.IsAlive)
                        {
                            //load last FSDJump 
                            var files = Directory.EnumerateFiles(journalPath, "Journal.????????????.??.log").OrderByDescending(f => f).ToList();
                            var jumpFound = false;
                            while (true)
                            {
                                LastJournalFile = files[0];
                                files.RemoveAt(0);

                                LastJournalPosition = 0;
                                ReadJournal();
                                lock (ContentLock)
                                {
                                    var lines = CachedContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);                                    
                                    CachedContent = "";
                                    for (var i = lines.Length; i > 0; i--)
                                    {
                                        var line = lines[i - 1];
                                        CachedContent = line + Environment.NewLine + CachedContent;
                                        if (line.Contains("\"event\":\"StartJump\","))
                                        {
                                            jumpFound = true;
                                            break;
                                        }
                                    }

                                    if (jumpFound)
                                    {
                                        //get name
                                        for (var i=0; i<lines.Length; i++)
                                        {
                                            var json = Serializer.Deserialize<Dictionary<string, object>>(lines[i]);
                                            if (json.GetProperty("event", "") == "Commander")
                                            {
                                                CommanderName = json.GetProperty("Name", "Unknown");
                                                break;
                                            }
                                        }
                                        
                                    }

                                }
                                if (jumpFound)
                                    break;
                                else
                                    lock (ContentLock)
                                        CachedContent = "";
                            }
                            logWatcher.EnableRaisingEvents = true;
                        }

                        var HudIsHidden = false;

                        var HideEvents = new[] { "Shutdown", "LaunchSRV" };
                        var ShowEvents = new[] { "DockSRV" };

                        Talk("Welcome Commander " + CommanderName +". This is the Explorers Virtual Reality Heads Up Display Extension for Elite Dangerous.", false);


                        while (AppRunning)
                        {
#if UseOpenVR
                            while (vrSystem.PollNextEvent(ref vrEvent, vrEventSize))
                            {
                                if (vrEvent.eventType == (int)EVREventType.VREvent_Quit)
                                {
                                    AppRunning = false;
                                    TrayIcon.Visible = false;
                                    continue;
                                }
                            }
#endif //UseOpenVR
                            if (!string.IsNullOrEmpty(CachedContent))
                            {
                                string[] lines;
                                lock (ContentLock)
                                {
                                    lines = CachedContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                                    CachedContent = lines.LastOrDefault();
                                }

                                foreach (var line in lines)
                                {
                                    if (!string.IsNullOrEmpty(line))
                                    {
                                        //System.Diagnostics.Debug.WriteLine(line);
                                        var entry = Serializer.Deserialize<Dictionary<string, object>>(line);
                                        {
                                            if (!entry.GetProperty("timestamp", "", out var timestamp))
                                                continue;

                                            if (!DateTime.TryParse(timestamp.ToString(), out var ts))
                                                continue;

                                            ts = ts.ToUniversalTime();

                                            if (ts < LastJournalTimeStamp)
                                                continue;

                                            LastJournalTimeStamp = ts;
                                            //Debug.WriteLine("TS: " + timestamp);

                                            var eventtype = entry.GetProperty("event", "");
                                            if (string.IsNullOrEmpty(eventtype))
                                                continue;

                                            foreach (var p in HudPanels)
                                                p.JournalUpdate(eventtype, entry);

                                            if (ShowEvents.Contains(eventtype))
                                            {
                                                foreach (var p in HudPanels)
                                                {
                                                    p.ShowPanel(true);
                                                    p.RefreshUpdate();
                                                }
                                            }

                                            else if (HideEvents.Contains(eventtype))
                                            {
                                                foreach (var p in HudPanels)
                                                    p.ShowPanel(false);
                                            }
                                        }
                                    }
                                }
                            }

                            bool anyUpdate = false;
                            foreach (var p in HudPanels)
                            {
                                if (p.PanelUpdated > 0)
                                {
                                    anyUpdate = true;
                                    p.CapturePanel();
                                    p.SendOverlay();
                                }
                            }
                            if (!HudIsHidden && Native.IsKeyDown(Keys.Pause))
                            {
                                HudIsHidden = true;
                                foreach (var p in HudPanels.Where(x => x is JumpInfoPanel))
                                    p.ShowPanel(false);
                            }
                            else if (HudIsHidden && !Native.IsKeyDown(Keys.Pause))
                            {
                                HudIsHidden = false;
                                foreach (var p in HudPanels.Where(x => x is JumpInfoPanel))
                                {
                                    p.ShowPanel(true);
                                    p.RefreshUpdate();
                                }
                            }

                            if (!anyUpdate)
                                Thread.Sleep(100);
                            else
                                Thread.Sleep(1);

                            Application.DoEvents();

                        }
                        if (journalReplayThread.IsAlive)
                            journalReplayThread.Join();
                        journalReplayThread = null;


                        logWatcher.EnableRaisingEvents = false;
                        logWatcher.Dispose();
                        logWatcher = null;

                        foreach (var p in HudPanels)
                            p.Dispose();
                        HudPanels.Clear();
                    }
#if UseOpenVR
                    OpenVR.Shutdown();
#endif //UseOpenVR
                }

                EDFont.Dispose();
            }
            Shutup();
            Talk("Farewell Commander " + CommanderName + ".", false);
            Speech.Dispose();
        }

        private string LastJournalFile { get; set; }
        private long LastJournalPosition { get; set; }
        private DateTime LastJournalTimeStamp { get; set; } = DateTime.MinValue;
        private string CachedContent { get; set; }
        private object ContentLock = new object();


        private void JournalChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                if (LastJournalFile != e.FullPath)
                {
                    LastJournalPosition = 0;
                    LastJournalFile = e.FullPath;
                    lock (ContentLock)
                        CachedContent = "";
                }
                ReadJournal();
            }
        }

        private void ReadJournal()
        {
            using (var fs = File.Open(LastJournalFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fs.Seek(LastJournalPosition, SeekOrigin.Begin);
                var bytes = new byte[fs.Length - fs.Position];
                fs.Read(bytes, 0, bytes.Length);
                LastJournalPosition = fs.Length;
                var strcontent = Encoding.UTF8.GetString(bytes);
                lock (ContentLock)
                    CachedContent += strcontent;
            }

        }

        public static string GetSavedGamesPath()
        {
            if (Environment.OSVersion.Version.Major < 6) throw new NotSupportedException();
            IntPtr pathPtr = IntPtr.Zero;
            try
            {
                Native.SHGetKnownFolderPath(ref Native.FolderSavedGames, 0, IntPtr.Zero, out pathPtr);
                return Marshal.PtrToStringUni(pathPtr);
            }
            finally
            {
                Marshal.FreeCoTaskMem(pathPtr);
            }
        }

    }
}
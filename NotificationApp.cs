using EDVRHUD.HUDs;
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
            public bool UseOpenVR { get; set; } = true;
        }

        public static bool InitialLoad = false;

        public static HudSettings Settings { get; set; } = new HudSettings();

        internal static SpeechSynthesizer Speech = new SpeechSynthesizer();

        public static void Talk(string s, bool async = true)
        {
            if (InitialLoad)
                return;

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

        private static List<HudPanel> HudPanels { get; } = new List<HudPanel>();

        private static MenuItem PanelsMenu;
        private MenuItem SettingsMenu;
        public static MenuItem ReplayMenu;

        public static string EDJournalPath { get; private set; }
        private static FileSystemWatcher EDLogWatcher;
        private FileSystemWatcher EDStatusWatcher;
        //private Thread EDJournalReplayThread;
        //public static bool ReplayNextSystem = false;

        private static Form ReplayForm = null;

        public NotificationApp()
        {
            PanelsMenu = new MenuItem("Panels");
            SettingsMenu = new MenuItem("Settings...");
            SettingsMenu.Click += SettingsMenu_Click;
            ReplayMenu = new MenuItem("Journal Replay...");
            ReplayMenu.Click += ReplayMenu_Click;

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
                        ReplayMenu,
                        SettingsMenu,
                        new MenuItem("Exit", Exit)
                    }),
                Visible = true
            };
            AppRunning = true;
        }

        private void ReplayMenu_Click(object sender, EventArgs e)
        {
            if (ReplayForm == null)
            {
                Talk("Stopping journal listening.", false);
                StopJournalListening();
                ReplayMenu.Enabled = false;
                ReplayForm = new ReplayControlForm();
                ReplayForm.Icon = Properties.Resources.Icon;
                ReplayForm.FormClosed += (s, a) =>
                {
                    if (AppRunning)
                    {
                        Talk("Resuming journal listening.", false);
                        LoadPanels(null, null);
                        ReplayMenu.Enabled = true;
                        StartJournalListening();
                    }
                };
            }
            LoadPanels(null, null);
            ReplayForm.Show();
            ReplayForm.Focus();

            ////select file
            //string[] filenames = new string[0];
            //using (var ofd = new OpenFileDialog())
            //{
            //    ofd.Multiselect = true;
            //    ofd.InitialDirectory = EDJournalPath;
            //    ofd.Filter = "Elite Dangerous Journal|Journal.????????????.??.log";
            //    ofd.Title = "Please select one or more journals to replay";
            //    ofd.DefaultExt = ".log";
            //    if (ofd.ShowDialog() == DialogResult.OK)
            //    {
            //        filenames = ofd.FileNames.OrderBy(f => f).ToArray();
            //    }
            //}

            //if (filenames.Length == 0)
            //    return;

            //EDLogWatcher.EnableRaisingEvents = false;
            //ReplayMenu.Enabled = false;

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
            string content;
            if (File.Exists(path))
                content = File.ReadAllText(path);
            else
                content = Encoding.UTF8.GetString(Properties.Resources.Settings);

            Settings = Serializer.Deserialize<HudSettings>(content);

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

        private static string CommanderName { get; set; }

        private void SavePanels(object sender, EventArgs e)
        {
            var saveData = new PanelSaveData();

            var list = new List<PanelSettings>();
            foreach (var panel in HudPanels)
            {
                list.Add(panel.Settings);
            }
            saveData.panels = list.ToArray();

            var s = Serializer.Serialize(saveData);
            var path = Environment.CurrentDirectory + "\\Panels.json";
            File.WriteAllText(path, s);
        }

        private static void LoadPanels(object sender, EventArgs e)
        {
            foreach (var panel in HudPanels)
            {
                panel.Dispose();
            }
            HudPanels.Clear();

            var path = Environment.CurrentDirectory + "\\Panels.json";
            string content;
            if (File.Exists(path))
                content = File.ReadAllText(path);
            else
                content = Encoding.UTF8.GetString(Properties.Resources.Panels);
            var json = Serializer.Deserialize<PanelSaveData>(content);
            foreach (var panelData in json.panels)
            {
                var panel = HudPanel.Create(panelData);
                HudPanels.Add(panel);
                panel.Initialize();
                PanelsMenu.MenuItems.Add(new MenuItem(panel.Name, OnShowPanel) { Tag = panel });
            }
        }

        private static void OnShowPanel(object sender, EventArgs e)
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

        public void Run()
        {
            EDJournalPath = GetSavedGamesPath() + "\\Frontier Developments\\Elite Dangerous";
            LoadSettings();
            GetCommander();
            if (Settings.VoiceEnable)
                Speech.Speak("Welcome Commander " + CommanderName + ".");

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
                    var vrEvent = new VREvent_t();
                    var vrEventSize = (uint)Marshal.SizeOf(vrEvent);
                    var refreshRate = 90;
                    var err = EVRInitError.None;
                    CVRSystem vrSystem = null;
                    var poseArray = new TrackedDevicePose_t[1] { new TrackedDevicePose_t() }; //hmd only
                    var intersectionParams = new VROverlayIntersectionParams_t();
                    intersectionParams.eOrigin = ETrackingUniverseOrigin.TrackingUniverseSeated;

                    if (Settings.UseOpenVR)
                    {
                        vrSystem = OpenVR.Init(ref err, EVRApplicationType.VRApplication_Overlay);
                        if (err != EVRInitError.None || OpenVR.Compositor == null || OpenVR.Overlay == null)
                        {
                            AppRunning = false;
                            return;
                        }

                        var terr = ETrackedPropertyError.TrackedProp_Success;
                        refreshRate = (int)OpenVR.System.GetFloatTrackedDeviceProperty(0, ETrackedDeviceProperty.Prop_DisplayFrequency_Float, ref terr);

                        OpenVR.System.GetDXGIOutputInfo(ref adapterIndex);
                    }

                    using (var factory = new Factory4())
                    {
                        var adapter = factory.GetAdapter(adapterIndex);
                        D3DDevice = new SharpDX.Direct3D11.Device(adapter, DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug);
                        D3DDeviceContext = D3DDevice.ImmediateContext;

                        LoadPanels(null, null);

                        EDStatusWatcher = new FileSystemWatcher(EDJournalPath, "Status.json")
                        {
                            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName
                        };
                        EDStatusWatcher.Changed += StatusChanged;
                        EDStatusWatcher.Created += StatusChanged;
                        EDStatusWatcher.EnableRaisingEvents = true;
                        ReadStatus();

                        StartJournalListening();

                        while (AppRunning)
                        {
                            if (InitialLoad)
                            {
                                lock (ContentLock)
                                {
                                    if (string.IsNullOrEmpty(CachedContent))
                                        InitialLoad = false;
                                }
                            }

                            bool intersect = false;
                            if (vrSystem != null)
                            {
                                while (vrSystem.PollNextEvent(ref vrEvent, vrEventSize))
                                {
                                    if (vrEvent.eventType == (int)EVREventType.VREvent_Quit)
                                    {
                                        AppRunning = false;
                                        TrayIcon.Visible = false;
                                        continue;
                                    }
                                }

                                //need to find a better way to calculate intersection parameters
                                vrSystem.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseSeated, 0, poseArray);
                                var m = new Matrix(
                                    poseArray[0].mDeviceToAbsoluteTracking.m0, poseArray[0].mDeviceToAbsoluteTracking.m4, poseArray[0].mDeviceToAbsoluteTracking.m8, 0,
                                    poseArray[0].mDeviceToAbsoluteTracking.m1, poseArray[0].mDeviceToAbsoluteTracking.m5, poseArray[0].mDeviceToAbsoluteTracking.m9, 0,
                                    poseArray[0].mDeviceToAbsoluteTracking.m2, poseArray[0].mDeviceToAbsoluteTracking.m6, poseArray[0].mDeviceToAbsoluteTracking.m10, 0,
                                    poseArray[0].mDeviceToAbsoluteTracking.m3, poseArray[0].mDeviceToAbsoluteTracking.m7, poseArray[0].mDeviceToAbsoluteTracking.m11, 1);
                                m.Decompose(out var s, out var r, out var t);

                                var dir = Vector3.Transform(t, r);

                                //var u = new Vector3(r.X, r.Y, r.Z);
                                //var sx = r.W;
                                //var dir = 2.0f * Vector3.Dot(u, t) * u + (sx * sx - Vector3.Dot(u, u)) * t + 2.0f * s * Vector3.Cross(u, t);

                                intersectionParams.vSource.v0 = (float)t.X;
                                intersectionParams.vSource.v1 = (float)t.Y;
                                intersectionParams.vSource.v2 = -(float)t.Z;

                                intersectionParams.vDirection.v0 = (float)dir.X;
                                intersectionParams.vDirection.v1 = (float)dir.Y;
                                intersectionParams.vDirection.v2 = -(float)dir.Z;

                                foreach (var p in HudPanels)
                                    intersect |= p.LookAt(ref intersectionParams);


                            }

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
                                    if (!string.IsNullOrWhiteSpace(line))
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
                                        }
                                    }
                                }
                            }

                            bool anyUpdate = false;
                            bool sendStatusFocus = false;
                            if (GUIFocus != PrevGUIFocus)
                            {
                                PrevGUIFocus = GUIFocus;
                                sendStatusFocus = true;
                            }
                            if (EDFlags != PrevEDFlags)
                            {
                                PrevEDFlags = EDFlags;
                                sendStatusFocus = true;
                            }

                            foreach (var p in HudPanels)
                            {
                                if (sendStatusFocus)
                                {
                                    p.EDStatusChanged(EDFlags, GUIFocus);
                                }
                                if (p.PanelUpdated > 0)
                                {
                                    anyUpdate = true;
                                    p.CapturePanel();
                                    p.SendOverlay();
                                }
                            }

                            if (!anyUpdate && !sendStatusFocus && !intersect)
                                Thread.Sleep(100);
                            else
                                Thread.Sleep(1);

                            Application.DoEvents();

                        }
                        //if (EDJournalReplayThread != null && EDJournalReplayThread.IsAlive)
                        //    EDJournalReplayThread.Join();
                        //EDJournalReplayThread = null;

                        StopJournalListening();

                        if (ReplayForm != null)
                        {
                            ReplayForm.Close();
                            ReplayForm.Dispose();
                            ReplayForm = null;
                        }

                        EDStatusWatcher.EnableRaisingEvents = false;
                        EDStatusWatcher.Dispose();
                        EDStatusWatcher = null;

                        foreach (var p in HudPanels)
                            p.Dispose();
                        HudPanels.Clear();
                    }

                    if (vrSystem != null)
                        OpenVR.Shutdown();
                }

                EDFont.Dispose();
            }
            Shutup();
            Talk("Farewell Commander " + CommanderName + ".", false);
            Speech.Dispose();
        }

        private static void StopJournalListening()
        {
            if (EDLogWatcher != null)
            {
                EDLogWatcher.EnableRaisingEvents = false;
                EDLogWatcher.Dispose();
                EDLogWatcher = null;
            }
        }

        private static void StartJournalListening()
        {
            LastJournalFile = "";
            LastJournalPosition = 0;
            LastJournalTimeStamp = DateTime.MinValue;
            PrevGUIFocus = GUIFocus.Initial;
            PrevEDFlags = StatusFlags.Initial;


            EDLogWatcher = new FileSystemWatcher(EDJournalPath, "Journal.????????????.??.log")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName
            };
            EDLogWatcher.Changed += JournalChanged;
            EDLogWatcher.Created += JournalChanged;

            InitialLoad = true;

            //load last FSDJump 
            var skippedFiles = new List<string>();
            var files = Directory.EnumerateFiles(EDJournalPath, "Journal.????????????.??.log").OrderByDescending(f => f).ToList();
            var jumpFound = false;

            while (true)
            {
                LastJournalFile = files[0];
                files.RemoveAt(0);

                LastJournalPosition = 0;
                LastJournalTimeStamp = DateTime.MinValue;
                ReadJournal();
                lock (ContentLock)
                {
                    var lines = CachedContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    CachedContent = "";
                    for (var i = lines.Length; i > 0; i--)
                    {
                        var line = lines[i - 1];
                        if (string.IsNullOrEmpty(line))
                            continue;
                        CachedContent = line + Environment.NewLine + CachedContent;
                        if (line.Contains("\"event\":\"StartJump\", \"JumpType\":\"Hyperspace\""))
                        {
                            jumpFound = true;
                            break;
                        }
                    }
                }
                if (jumpFound)
                {
                    if (skippedFiles.Count > 0)
                    {
                        files = skippedFiles;
                    }
                    else
                        break;
                }
                else
                {
                    lock (ContentLock)
                        CachedContent = "";
                    skippedFiles.Add(LastJournalFile);
                }
            }
            EDLogWatcher.EnableRaisingEvents = true;
        }

        private static string LastJournalFile { get; set; }
        private static long LastJournalPosition { get; set; }
        public static DateTime LastJournalTimeStamp { get; set; } = DateTime.MinValue;
        internal static string CachedContent { get; set; }
        internal static object ContentLock = new object();

        private static GUIFocus GUIFocus = GUIFocus.None;
        private static GUIFocus PrevGUIFocus = GUIFocus.Initial;

        private static StatusFlags EDFlags = StatusFlags.None;
        private static StatusFlags PrevEDFlags = StatusFlags.Initial;

        private void StatusChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created)
            {
                //Debug.WriteLine("Status file changed.");
                ReadStatus();
            }
        }

        private static void JournalChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created)
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

        private void ReadStatus()
        {
            try
            {
                using (var fs = File.Open(EDJournalPath + "\\Status.json", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, bytes.Length);
                    fs.Close();
                    var strcontent = Encoding.UTF8.GetString(bytes);
                    if (string.IsNullOrWhiteSpace(strcontent))
                        return;
                    var status = Serializer.Deserialize<Dictionary<string, object>>(strcontent);
                    GUIFocus = (GUIFocus)status.GetProperty("GuiFocus", 0);
                    EDFlags = (StatusFlags)status.GetProperty("Flags", 0L);
                    //if (EDFlags == StatusFlags.None)
                    //    return;
                    Debug.WriteLine("GUIFocus: " + GUIFocus + " ED Status: " + EDFlags.ToString());
                }
            }
            catch
            {

            }
        }

        private static void ReadJournal()
        {
            try
            {
                using (var fs = File.Open(LastJournalFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fs.Seek(LastJournalPosition, SeekOrigin.Begin);
                    var len = fs.Length;
                    var bytes = new byte[len - fs.Position];
                    fs.Read(bytes, 0, bytes.Length);
                    fs.Close();
                    LastJournalPosition = len;
                    var strcontent = Encoding.UTF8.GetString(bytes);
                    lock (ContentLock)
                        CachedContent += strcontent;
                }
            }
            catch
            {

            }
        }

        private static void GetCommander()
        {
            var files = Directory.EnumerateFiles(EDJournalPath, "Journal.????????????.??.log").OrderByDescending(f => f).ToList();
            if (files.Count > 0)
            {
                var filename = files[0];
                string strcontent = "";
                using (var fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    var len = fs.Length;
                    var bytes = new byte[len];
                    fs.Read(bytes, 0, bytes.Length);
                    fs.Close();
                    strcontent = Encoding.UTF8.GetString(bytes);
                }

                var lines = strcontent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                //get name
                for (var i = 0; i < lines.Length; i++)
                {
                    var json = Serializer.Deserialize<Dictionary<string, object>>(lines[i]);
                    if (json.GetProperty("event", "") == "Commander")
                    {
                        CommanderName = json.GetProperty("Name", "Unknown");
                        return;
                    }
                }
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

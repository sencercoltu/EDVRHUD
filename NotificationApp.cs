using LiteDB;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Valve.VR;
using System.Resources;
using SharpDX.DirectInput;
using System.Diagnostics;

namespace EDVRHUD
{
    class NotificationApp : ApplicationContext
    {

        private NotifyIcon TrayIcon { get; set; }
        public static bool AppRunning { get; private set; } = true;

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
        //private FileSystemWatcher EDStatusWatcher;
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
                EDCommon.Talk("Stopping journal listening.", false);
                StopJournalListening();
                EDCommon.TravelMapData.Clear();
                ReplayMenu.Enabled = false;
                ReplayForm = new ReplayControlForm();
                ReplayForm.Icon = Properties.Resources.Icon;
                ReplayForm.FormClosed += (s, a) =>
                {
                    if (AppRunning)
                    {
                        EDCommon.Talk("Resuming journal listening.", false);
                        LoadPanels(null, null);
                        ReplayMenu.Enabled = true;
                        StartJournalListening();
                        ReplayForm = null;
                    }
                };
            }
            LoadPanels(null, null);
            if (ReplayForm != null)
            {
                ReplayForm.Show();
                ReplayForm.Focus();
            }
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

            var s = EDCommon.Serializer.Serialize(saveData);
            var path = Environment.CurrentDirectory + "\\Panels.json";
            File.WriteAllText(path, s);
        }

        private static void LoadPanels(object sender, EventArgs e)
        {
            foreach (MenuItem panel in PanelsMenu.MenuItems)
            {
                var p = panel.Tag as HudPanel;
                p.Dispose();
            }
            HudPanels.Clear();
            PanelsMenu.MenuItems.Clear();

            var path = Environment.CurrentDirectory + "\\Panels.json";
            string content;
            if (File.Exists(path))
                content = File.ReadAllText(path);
            else
                content = Encoding.UTF8.GetString(Properties.Resources.Panels);
            var json = EDCommon.Serializer.Deserialize<PanelSaveData>(content);
            foreach (var panelData in json.panels)
            {
                if (!panelData.Enabled) continue;
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
            EDJournalPath = Path.Combine(GetSavedGamesPath(), "Frontier Developments", "Elite Dangerous");
            EDCommon.LoadSettings();


            EDCommon.DB = new LiteDatabase("Filename=EDVRHUD.db;Mode=Shared;");
            EDCommon.DBJournal = EDCommon.DB.GetCollection<Dictionary<string, object>>("Journal");
            EDCommon.DBSettings = EDCommon.DB.GetCollection<Dictionary<string, object>>("Settings");
            RebuildDB();

            //var reults = EDCommon.DBColl.Query()
            //    .Where(x => x["Radius"] != null && x["PlanetType"] != null)
            //    .OrderBy(x => x["Radius"])
            //    //.Select(x => new { BodyName=x["BodyName"], Radius=x["Radius"]})                
            //    .ToList();

            GetCommander();
            if (EDCommon.Settings.VoiceEnable)
                EDCommon.Speech.Speak("Welcome Commander " + CommanderName + ".");

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

                    if (EDCommon.Settings.UseOpenVR)
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

                        //EDStatusWatcher = new FileSystemWatcher(EDJournalPath, "Status.json")
                        //{
                        //    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName
                        //};
                        //EDStatusWatcher.Changed += StatusChanged;
                        //EDStatusWatcher.EnableRaisingEvents = true;
                        ReadStatus();

                        StartJournalListening();



                        HudPanel focusedPanel = null;
                        while (AppRunning)
                        {
                            if (EDCommon.InitialLoad)
                            {
                                lock (ContentLock)
                                {
                                    if (string.IsNullOrEmpty(CachedContent))
                                        EDCommon.InitialLoad = false;
                                }
                            }
                            else if (ReplayForm == null)
                            {
                                ReadStatus();
                                ReadJournal();
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

                                //TODO: need to find a better way to calculate intersection parameters
                                vrSystem.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseSeated, 0, poseArray);
                                var m = new Matrix(
                                    poseArray[0].mDeviceToAbsoluteTracking.m0, poseArray[0].mDeviceToAbsoluteTracking.m4, poseArray[0].mDeviceToAbsoluteTracking.m8, 0,
                                    poseArray[0].mDeviceToAbsoluteTracking.m1, poseArray[0].mDeviceToAbsoluteTracking.m5, poseArray[0].mDeviceToAbsoluteTracking.m9, 0,
                                    poseArray[0].mDeviceToAbsoluteTracking.m2, poseArray[0].mDeviceToAbsoluteTracking.m6, poseArray[0].mDeviceToAbsoluteTracking.m10, 0,
                                    poseArray[0].mDeviceToAbsoluteTracking.m3, poseArray[0].mDeviceToAbsoluteTracking.m7, poseArray[0].mDeviceToAbsoluteTracking.m11, 0);
                                m.Decompose(out var s, out var r, out var t);

                                var dir = Vector3.Transform(t, r);

                                intersectionParams.vSource.v0 = (float)t.X;
                                intersectionParams.vSource.v1 = (float)t.Y;
                                intersectionParams.vSource.v2 = -(float)t.Z;

                                intersectionParams.vDirection.v0 = (float)dir.X;
                                intersectionParams.vDirection.v1 = (float)dir.Y;
                                intersectionParams.vDirection.v2 = -(float)dir.Z;

                                
                                HudPanel newFocusedPanel = null;
                                foreach (var p in HudPanels)
                                {                                    
                                    intersect |= p.LookAt(ref intersectionParams);
                                    if (p.HasFocus)
                                        newFocusedPanel = p;
                                }

                                if (focusedPanel != newFocusedPanel)
                                {
                                    newFocusedPanel?.Redraw();
                                    focusedPanel?.Redraw();
                                    focusedPanel = newFocusedPanel;
                                }

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
                                        var entry = EDCommon.Serializer.Deserialize<Dictionary<string, object>>(line);

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

                                        var isReplay = entry.GetProperty("IsReplay", false);

                                        //foreach (var p in HudPanels)
                                        //    p.OnJournalUpdate(eventtype, entry);
                                        HudPanel.JournalUpdate(eventtype, entry, HudPanels);

                                        //isreplay is used for scans
                                        if (!isReplay)
                                        {
                                            var id = new BsonValue(EDCommon.DBIDForEntry(ts, eventtype, entry).ToByteArray());
                                            EDCommon.DBJournal.Upsert(id, entry);
                                        }
                                    }
                                }
                            }

                            bool anyUpdate = false;
                            bool sendStatusFocus = false;
                            if (GUIFocus != PrevGUIFocus || EDFlags != PrevEDFlags)
                            {
                                PrevGUIFocus = GUIFocus;
                                PrevEDFlags = EDFlags;
                                sendStatusFocus = true;
                            }

                            foreach (var p in HudPanels)
                            {
                                p.RedrawIfNeeded();

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

                            Application.DoEvents();

                            if (focusedPanel != null)
                            {
                                //Debug.WriteLine("Focused panel:" + focusedPanel.Name);
                                var upScrollState = false;
                                var downScrollState = false;
                                lock (EDCommon.InputDevices)
                                {
                                    foreach (var joystick in EDCommon.InputDevices)
                                    {
                                        joystick.Poll();
                                        var datas = joystick.GetBufferedData();
                                        foreach (var state in datas)
                                        {
                                            if (EDCommon.Settings.ScrollUp.ObjectOffset == state.RawOffset)
                                                upScrollState = true;
                                            if (EDCommon.Settings.ScrollDown.ObjectOffset == state.RawOffset)
                                                downScrollState = true;
                                        }
                                    }
                                }
                                if (upScrollState != EDCommon.Settings.ScrollUp.LastState)
                                {
                                    EDCommon.Settings.ScrollUp.LastState = upScrollState;
                                    if (upScrollState)
                                        focusedPanel.OnScroll(new MouseEventArgs(MouseButtons.None, 0, 0, 0, 120));
                                }
                                if (downScrollState != EDCommon.Settings.ScrollDown.LastState)
                                {
                                    EDCommon.Settings.ScrollDown.LastState = downScrollState;
                                    if (downScrollState)
                                        focusedPanel.OnScroll(new MouseEventArgs(MouseButtons.None, 0, 0, 0, -120));
                                }

                            }

                            if (!anyUpdate && !sendStatusFocus && !intersect)
                                Thread.Sleep(100);
                            else
                                Thread.Sleep(1);



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

                        //EDStatusWatcher.EnableRaisingEvents = false;
                        //EDStatusWatcher.Dispose();
                        //EDStatusWatcher = null;

                        foreach (var p in HudPanels)
                            p.Dispose();
                        HudPanels.Clear();
                    }

                    if (vrSystem != null)
                        OpenVR.Shutdown();
                }

                EDFont.Dispose();
            }

            if (CurrentStream != null)
                CurrentStream.Dispose();
            CurrentStream = null;

            EDCommon.ClearInputDevices();

            EDCommon.Shutup();
            EDCommon.Talk("Farewell Commander " + CommanderName + ".", false);
            EDCommon.Speech.Dispose();

            EDCommon.DB.Dispose();
        }

        private void RebuildDB()
        {
            EDCommon.DBJournal.EnsureIndex("SystemAddress");
            EDCommon.DBJournal.EnsureIndex("event");
            EDCommon.DBJournal.EnsureIndex("timestamp");

            var settings = EDCommon.DBSettings.Query().FirstOrDefault() ?? new Dictionary<string, object>();
            var lastJournalFile = settings?.GetProperty("LastJournalFile", "");
            var lastJournalSize = 0L;
            //var reults = EDCommon.DBColl.Query()
            //    .Where(x => x["Radius"] != null && x["PlanetType"] != null)
            //    .OrderBy(x => x["Radius"])
            //    //.Select(x => new { BodyName=x["BodyName"], Radius=x["Radius"]})                
            //    .ToList();


            //read all logs and write to db                
            var logs = Directory.EnumerateFiles(EDJournalPath, "Journal.????????????.??.log").OrderBy(f => f);
            if (string.IsNullOrEmpty(lastJournalFile))
                TrayIcon.ShowBalloonTip(2000, "Importing journals", "Total of " + logs.Count() + " logs found.", ToolTipIcon.Info);
            var lastTs = DateTime.MinValue;
            var lastLog = "";
            foreach (var log in logs)
            {
                Application.DoEvents();
                if (log.CompareTo(lastJournalFile) < 0)
                    continue;
                lastLog = log;
                using (var fs = File.Open(log, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    lastJournalSize = fs.Length;
                    EDCommon.DB.BeginTrans();
                    var bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, bytes.Length);
                    fs.Close();
                    var strcontent = Encoding.UTF8.GetString(bytes);
                    var lines = strcontent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var entry = EDCommon.Serializer.Deserialize<Dictionary<string, object>>(line);
                        if (!entry.GetProperty("timestamp", "", out var timestamp))
                            continue;

                        if (!DateTime.TryParse(timestamp.ToString(), out var ts))
                            continue;

                        ts = ts.ToUniversalTime();

                        var eventtype = entry.GetProperty("event", "");
                        if (string.IsNullOrEmpty(eventtype))
                            continue;


                        var id = new BsonValue(EDCommon.DBIDForEntry(ts, eventtype, entry).ToByteArray());
                        EDCommon.DBJournal.Upsert(id, entry);
                    }
                    EDCommon.DB.Commit();
                }
            }

            settings["LastJournalFile"] = lastLog;
            settings["LastJournalPos"] = lastJournalSize;
            EDCommon.DBSettings.Upsert(new BsonValue(new ObjectId(0, 0, 0, 0).ToByteArray()), settings); //single item in coll
        }

        public static void RefreshScans(long systemAddress)
        {
            var scans = new Dictionary<string, Dictionary<string, object>>();

            var fssResult = EDCommon.DBJournal.Find(LiteDB.Query.And(LiteDB.Query.EQ("SystemAddress", new BsonValue(systemAddress)), LiteDB.Query.EQ("event", "FSSDiscoveryScan"))).LastOrDefault();

            var scansResult = EDCommon.DBJournal.Find(LiteDB.Query.And(LiteDB.Query.EQ("SystemAddress", new BsonValue(systemAddress)), LiteDB.Query.EQ("event", "Scan"), LiteDB.Query.Or(LiteDB.Query.Not("StarType", null), LiteDB.Query.Not("PlanetClass", null))));
            foreach (var scan in scansResult)
            {
                var bodyName = scan.GetProperty("BodyName", "");
                if (string.IsNullOrEmpty(bodyName))
                    continue;
                scans[bodyName] = scan;
                scan["WasDiscovered"] = true;
                scan["SkipValues"] = true;
            }

            var mapResult = EDCommon.DBJournal.Find(LiteDB.Query.And(LiteDB.Query.EQ("SystemAddress", new BsonValue(systemAddress)), LiteDB.Query.EQ("event", "SAAScanComplete")));
            foreach (var scan in mapResult)
            {
                var bodyName = scan.GetProperty("BodyName", "");
                if (string.IsNullOrEmpty(bodyName))
                    continue;
                scans.TryGetValue(bodyName, out var s);
                if (s == null)
                    continue;
                s["WasMapped"] = true;
                s["SkipValues"] = true;
            }


            var sb = new StringBuilder();
            if (fssResult == null)
            {
                fssResult = new Dictionary<string, object>
                {
                    ["event"] = "FSSDiscoveryScan",
                    ["Progress"] = 1,
                    ["BodyCount"] = scans.Count,
                    ["NonBodyCount"] = 0,
                    ["SystemAddress"] = systemAddress,
                    ["SystemName"] = scans.Any() ? scans.First().Value["StarSystem"] : ""
                };
            }
            fssResult.Remove("_id");
            fssResult["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            fssResult["IsReplay"] = true;
            sb.AppendLine(EDCommon.Serializer.Serialize(fssResult));

            foreach (var item in scans)
            {
                item.Value.Remove("_id");
                item.Value["IsReplay"] = true;
                item.Value["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                sb.AppendLine(EDCommon.Serializer.Serialize(item.Value));
            }
            lock (ContentLock)
                CachedContent += sb.ToString();
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
            CurrentJournalFile = "";
            LastJournalPosition = 0;
            LastJournalTimeStamp = DateTime.MinValue;
            PrevGUIFocus = GUIFocus.Initial;
            PrevEDFlags = StatusFlags.Initial;

            EDCommon.InitialLoad = true;
            EDCommon.TravelMapData.Clear();
            EDCommon.LoadTravelMap("2000-01-01T00:00:00Z");

            var startId = EDCommon.DBJournal.Find(LiteDB.Query.And(LiteDB.Query.EQ("event", "StartJump"), LiteDB.Query.EQ("JumpType", "Hyperspace")))
                .OrderByDescending(x => x["timestamp"])
                .Select(x => x["timestamp"])
                .FirstOrDefault();

            var latestEntries = EDCommon.DBJournal.Find(LiteDB.Query.GTE("timestamp", new BsonValue(startId)));

            var sb = new StringBuilder();
            foreach (var entry in latestEntries)
            {
                entry.Remove("_id");
                sb.AppendLine(EDCommon.Serializer.Serialize(entry));
            }
            CachedContent = sb.ToString();

            EDLogWatcher = new FileSystemWatcher(EDJournalPath, "Journal.????????????.??.log")
            {
                NotifyFilter = NotifyFilters.FileName
            };

            //EDLogWatcher.Changed += JournalChanged;
            EDLogWatcher.Created += JournalChanged;

            if (CurrentStream != null)
            {
                CurrentStream.Dispose();
                CurrentStream = null;
            }

            var settings = EDCommon.DBSettings.Query().FirstOrDefault() ?? new Dictionary<string, object>();
            var lastJournalFile = settings?.GetProperty("LastJournalFile", "");
            var lastJournalPosition = settings?.GetProperty("LastJournalPos", 0L);

            LastJournalPosition = lastJournalPosition.Value;
            CurrentStream = File.Open(lastJournalFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            CurrentStream.Seek(LastJournalPosition, SeekOrigin.Begin);

            EDLogWatcher.EnableRaisingEvents = true;
            //ReadJournal();
        }

        private static string NewJournalFile { get; set; }
        private static string CurrentJournalFile { get; set; }
        private static FileStream CurrentStream { get; set; }

        public static DateTime LastJournalTimeStamp { get; set; } = DateTime.MinValue;
        private static long LastJournalPosition { get; set; }
        internal static string CachedContent { get; set; }
        internal static object ContentLock = new object();

        public static GUIFocus GUIFocus { get; private set; } = GUIFocus.None;
        public static GUIFocus PrevGUIFocus { get; private set; } = GUIFocus.Initial;

        public static StatusFlags EDFlags { get; private set; } = StatusFlags.None;
        public static StatusFlags PrevEDFlags { get; private set; } = StatusFlags.Initial;

        //private void StatusChanged(object sender, FileSystemEventArgs e)
        //{
        //    if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created)
        //    {
        //        //Debug.WriteLine("Status file changed.");

        //    }
        //}

        private static void JournalChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                NewJournalFile = e.FullPath;
                //CurrentJournalFile = e.FullPath;
                //ReadJournal();
            }
        }

        private void ReadStatus()
        {
            try
            {
                using (var fs = File.Open(EDJournalPath + "\\Status.json", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] bytes = null;
                    if (fs.Length >= 0)
                    {
                        bytes = new byte[fs.Length];
                        fs.Read(bytes, 0, bytes.Length);
                        fs.Close();
                    }
                    var strcontent = Encoding.UTF8.GetString(bytes);
                    if (string.IsNullOrWhiteSpace(strcontent))
                        return;
                    var status = EDCommon.Serializer.Deserialize<Dictionary<string, object>>(strcontent);
                    GUIFocus = (GUIFocus)status.GetProperty("GuiFocus", 0);
                    EDFlags = (StatusFlags)status.GetProperty("Flags", 0L);
                    //if (EDFlags == StatusFlags.None)
                    //    return;
                    //Debug.WriteLine("GUIFocus: " + GUIFocus + " ED Status: " + EDFlags.ToString());
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
                if (!string.IsNullOrEmpty(CurrentJournalFile))
                {
                    if (CurrentStream != null)
                        CurrentStream.Dispose();
                    CurrentStream = File.Open(CurrentJournalFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    CurrentStream.Seek(0, SeekOrigin.Begin);
                    CurrentJournalFile = null;
                    LastJournalPosition = 0;
                }

                if (CurrentStream != null)
                {
                    if (CurrentStream.Length == LastJournalPosition)
                        return;
                    var len = CurrentStream.Length;
                    var bytes = new byte[len - CurrentStream.Position];
                    CurrentStream.Read(bytes, 0, bytes.Length);
                    LastJournalPosition = len;
                    var strcontent = Encoding.UTF8.GetString(bytes);
                    lock (ContentLock)
                        CachedContent += strcontent;
                }
                if (!string.IsNullOrEmpty(NewJournalFile))
                {
                    CurrentJournalFile = NewJournalFile;
                    NewJournalFile = null;
                    CurrentStream.Dispose();
                    CurrentStream = null;
                }
            }
            catch
            {

            }
        }

        private static void GetCommander()
        {
            //var reults = EDCommon.DBColl.Query()
            //    .Where(x => x["Radius"] != null && x["PlanetType"] != null)
            //    .OrderBy(x => x["Radius"])
            //    //.Select(x => new { BodyName=x["BodyName"], Radius=x["Radius"]})                
            //    .ToList();

            var result = EDCommon.DBJournal.Query()
                .Where(x => x["Commander"] != null)
                .OrderByDescending(x => x["timestamp"])
                .FirstOrDefault();

            if (result != null)
                CommanderName = result["Commander"].ToString();

            //var files = Directory.EnumerateFiles(EDJournalPath, "Journal.????????????.??.log").OrderByDescending(f => f).ToList();
            //while (files.Count > 0)
            //{
            //    var filename = files[0];
            //    string strcontent = "";
            //    using (var fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            //    {
            //        fs.Seek(0, SeekOrigin.Begin);
            //        var len = fs.Length;
            //        var bytes = new byte[len];
            //        fs.Read(bytes, 0, bytes.Length);
            //        fs.Close();
            //        strcontent = Encoding.UTF8.GetString(bytes);
            //    }

            //    var lines = strcontent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            //    //get name
            //    for (var i = 0; i < lines.Length; i++)
            //    {
            //        var json = Serializer.Deserialize<Dictionary<string, object>>(lines[i]);
            //        if (json.GetProperty("event", "") == "Commander")
            //        {
            //            CommanderName = json.GetProperty("Name", "Unknown");
            //            return;
            //        }
            //    }
            //    files.RemoveAt(0);
            //}
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

using EDVRHUD.HUDs;
using EDVRHUD.Properties;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Windows.Forms;
using Valve.VR;

namespace EDVRHUD
{
    public enum HudType
    {
        JumpInfo,
        Warning,
        ScanInfo
    }

    public struct PanelSettings
    {
        public HudType Type;
        public int Width;
        public int Height;
        public float Scale;
        public float UnfocusScale;
        public float Alpha;
        public float UnfocusAlpha;
        public HmdMatrix34_t Position;
    }

    public class PanelSaveData
    {
        public PanelSettings[] panels;
    }


    internal abstract class HudPanel : IDisposable
    {
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (OverlayHandle != 0)
                    {
                        OpenVR.Overlay?.ClearOverlayTexture(OverlayHandle);
                        OpenVR.Overlay?.DestroyOverlay(OverlayHandle);
                        OverlayHandle = 0;
                    }

                    if (HudPreview != null)
                    {
                        HudPreview.Close();
                        if (HudPreview != null) //formclose nulls HudPreview
                            HudPreview.Dispose();
                        HudPreview = null;
                    }

                    if (IntermediateBitmap != null)
                    {
                        IntermediateBitmap.Dispose();
                        IntermediateBitmap = null;
                    }

                    if (OffScreenPtr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(OffScreenPtr);
                        OffScreenPtr = IntPtr.Zero;
                    }
                }

                disposedValue = true;
            }
        }
        ~HudPanel()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        private Form HudPreview = null;

        private Bitmap IntermediateBitmap = null;

        public HmdMatrix34_t OverlayPosition { get { return Settings.Position; } }

        private ulong OverlayHandle = 0;

        public string Name { get; private set; }

        public Size PanelSize { get; private set; }

        public Size TextureSize { get; private set; }

        public HudType Type { get; private set; }

        private readonly DataBox[] TextureDataBox = new[] { new DataBox() };
        private Texture2D RenderTexture;
        private RenderTargetView RenderView;
        private Texture_t OVRTexture;
        public PanelSettings Settings;

        private IntPtr OffScreenPtr = IntPtr.Zero;

        public HudPanel(string name, PanelSettings settings) //HudType type, string name, Size size, HmdMatrix34_t position)
        {
            Settings = settings;
            Type = settings.Type;
            Name = name;
            PanelSize = new Size(settings.Width, settings.Height);
            TextureSize = new Size(settings.Width * 2, settings.Height * 2);
            OffScreenPtr = Marshal.AllocHGlobal(TextureSize.Width * TextureSize.Height * 4);

            var m = new Matrix()
            {
                M11 = Settings.Position.m0,
                M12 = Settings.Position.m1,
                M13 = Settings.Position.m2,
                M14 = Settings.Position.m3,

                M21 = Settings.Position.m4,
                M22 = Settings.Position.m5,
                M23 = Settings.Position.m6,
                M24 = Settings.Position.m7,

                M31 = Settings.Position.m8,
                M32 = Settings.Position.m9,
                M33 = Settings.Position.m10,
                M34 = Settings.Position.m11,
            };

            //pitch ile roll ters olabilir
            Matrix.GetYawPitchRoll(ref m, out yaw, out pitch, out roll);

        }

        private bool _initialized = false;

        private bool PanelVisible { get; set; } = false;

        public virtual void Initialize()
        {
            if (_initialized)
                return;

            var hDesktopDC = Native.GetDC(Native.GetDesktopWindow());

            if (NotificationApp.Settings.UseOpenVR)
            {
                var overlayError = OpenVR.Overlay?.CreateOverlay("OVRHUD_" + Name.ToLowerInvariant().Replace(" ", "_"), Name, ref OverlayHandle);
                overlayError = OpenVR.Overlay?.SetOverlayFlag(OverlayHandle, VROverlayFlags.VisibleInDashboard, true);
                overlayError = OpenVR.Overlay?.SetOverlayFlag(OverlayHandle, VROverlayFlags.NoDashboardTab, true);
                overlayError = OpenVR.Overlay?.SetOverlayTransformAbsolute(OverlayHandle, ETrackingUniverseOrigin.TrackingUniverseSeated, ref Settings.Position);
                overlayError = OpenVR.Overlay?.SetOverlayWidthInMeters(OverlayHandle, (PanelSize.Width / 1000f) * Settings.Scale);
            }
            ShowPanel(true);

            var texture2dDescription = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.Write,
                Format = Format.R8G8B8A8_UNorm,
                Width = TextureSize.Width,
                Height = TextureSize.Height,
                MipLevels = 3,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default
            };

            RenderTexture = new Texture2D(NotificationApp.D3DDevice, texture2dDescription);
            RenderView = new RenderTargetView(NotificationApp.D3DDevice, RenderTexture);

            OVRTexture = new Texture_t
            {
                eColorSpace = EColorSpace.Auto,
                handle = RenderView.Resource.NativePointer,
                eType = ETextureType.DirectX
            };

            IntermediateBitmap = new Bitmap(TextureSize.Width, TextureSize.Height, PixelFormat.Format32bppArgb);
            _initialized = true;

            Redraw();
            CapturePanel();
            SendOverlay();

#if DEBUG
            ShowPreview();
#endif //DEBUG
        }

        public static HudPanel Create(PanelSettings settings)
        {
            switch (settings.Type)
            {
                case HudType.JumpInfo:
                    return new JumpInfoPanel(settings); // new Size(settings.Width, settings.Height), ref settings.Position);
                case HudType.ScanInfo:
                    return new ScanInfoPanel(settings); // new Size(settings.Width, settings.Height), ref settings.Position);
                case HudType.Warning:
                    return new WarningPanel(settings); // new Size(settings.Width, settings.Height), ref settings.Position);
                default:
                    return null;
            }

        }

        protected Graphics GetGraphics()
        {
            return Graphics.FromImage(IntermediateBitmap);
        }

        internal void CapturePanel()
        {
            PanelUpdated -= 1;
            if (HudPreview != null)
                HudPreview.Invalidate();

            var data = IntermediateBitmap.LockBits(new Rectangle(0, 0, IntermediateBitmap.Width, IntermediateBitmap.Height), ImageLockMode.ReadOnly, IntermediateBitmap.PixelFormat);
            unsafe
            {
                //swap byteorder for texture
                uint* byteData = (uint*)data.Scan0;
                uint* offScreenData = (uint*)OffScreenPtr;
                var len = IntermediateBitmap.Width * IntermediateBitmap.Height;
                for (int i = 0; i < len; i++)
                {
                    offScreenData[i] = (byteData[i] & 0x000000FF) << 16 | (byteData[i] & 0x0000FF00) | (byteData[i] & 0x00FF0000) >> 16 | (byteData[i] & 0xFF000000);
                }
            }
            IntermediateBitmap.UnlockBits(data);

            TextureDataBox[0].DataPointer = OffScreenPtr;
            TextureDataBox[0].RowPitch = data.Stride;
            NotificationApp.D3DDeviceContext.UpdateSubresource(TextureDataBox[0], RenderTexture);
            NotificationApp.D3DDeviceContext.Flush();
        }

        double yaw = 0;
        double roll = 0;
        double pitch = 0;

        protected virtual void StartModifyOverlay()
        {
            Debug.WriteLine("Start modify panel " + Name);
            ShowPanel(true);
        }

        protected virtual void EndModifyOverlay()
        {
            Debug.WriteLine("End modify panel " + Name);
            PanelUpdated = 2;
        }

        public void ModifyOverlayScale(int dx)
        {
            if (dx == 0) return;
            Settings.Scale += dx / 1000f;
            if (Settings.Scale < 0.01f) Settings.Scale = 0.01f;
            else if (Settings.Scale > 10f) Settings.Scale = 10f;
            var overlayError = OpenVR.Overlay.SetOverlayWidthInMeters(OverlayHandle, (PanelSize.Width / 1000f) * Settings.Scale);
        }

        public void ModifyOverlayTranslation(int dx, int dy)
        {
            if (dx == 0 && dy == 0) return;
            Debug.WriteLine("Modifiying panel " + Name + " " + dx + "," + dy);

            if (Native.IsKeyDown(Keys.LControlKey))
            {
                //Z pos
                Settings.Position.m11 += dy / 1000f;
            }
            else if (Native.IsKeyDown(Keys.LShiftKey))
            {
                //X and Y pos
                Settings.Position.m7 -= dy / 1000f;
                Settings.Position.m3 += dx / 1000f;
            }
            else if (Native.IsKeyDown(Keys.RControlKey))
            {
                //Z rot
                roll += dx / 1000.0;
                var q = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
                var m = q.ToMatrix();
                Settings.Position.m0 = (float)m.M11;
                Settings.Position.m1 = (float)m.M12;
                Settings.Position.m2 = (float)m.M13;

                Settings.Position.m4 = (float)m.M21;
                Settings.Position.m5 = (float)m.M22;
                Settings.Position.m6 = (float)m.M23;

                Settings.Position.m8 = (float)m.M31;
                Settings.Position.m9 = (float)m.M32;
                Settings.Position.m10 = (float)m.M33;
            }
            else if (Native.IsKeyDown(Keys.RShiftKey))
            {
                //X and Y rot
                yaw += dx / 1000.0;
                pitch += dy / 1000.0;
                var q = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
                var m = q.ToMatrix();
                Settings.Position.m0 = (float)m.M11;
                Settings.Position.m1 = (float)m.M12;
                Settings.Position.m2 = (float)m.M13;

                Settings.Position.m4 = (float)m.M21;
                Settings.Position.m5 = (float)m.M22;
                Settings.Position.m6 = (float)m.M23;

                Settings.Position.m8 = (float)m.M31;
                Settings.Position.m9 = (float)m.M32;
                Settings.Position.m10 = (float)m.M33;
            }

            PanelUpdated = 2;
        }

        public class DoubleBufferForm : Form
        {
            internal HudPanel Panel;
            internal Point InitialMousePos;
            internal bool Modifying = false;
            public DoubleBufferForm(HudPanel panel)
            {
                DoubleBuffered = true;
                Panel = panel;
            }
        }

        internal void ShowPreview()
        {
            if (HudPreview == null)
            {
                HudPreview = new DoubleBufferForm(this)
                {
                    Icon = Properties.Resources.Icon,
                    Text = Name,
                    ClientSize = new Size(PanelSize.Width, PanelSize.Height),
                    FormBorderStyle = FormBorderStyle.FixedSingle,
                    SizeGripStyle = SizeGripStyle.Hide,
                    MaximizeBox = false
                };

                HudPreview.FormClosing += (sender, args) =>
                {
                    HudPreview = null;
                };

                HudPreview.Paint += (sender, args) =>
                {
                    args.Graphics.Clear(NotificationApp.DefaultClearColor);
                    args.Graphics.DrawImage(IntermediateBitmap, HudPreview.ClientRectangle);
                    args.Graphics.Flush();
                };

                HudPreview.MouseDown += (sender, args) =>
                {
                    var frm = (sender as DoubleBufferForm);
                    frm.InitialMousePos = Cursor.Position;
                    StartModifyOverlay();
                    frm.Modifying = true;
                };

                HudPreview.MouseUp += (sender, args) =>
                {
                    var frm = (sender as DoubleBufferForm);
                    frm.InitialMousePos = Cursor.Position;
                    EndModifyOverlay();
                    frm.Modifying = false;
                };

                HudPreview.MouseMove += (sender, args) =>
                {
                    var frm = (sender as DoubleBufferForm);
                    if (!frm.Modifying)
                        return;
                    var initialPos = frm.InitialMousePos;
                    var mousePos = Cursor.Position;
                    if (args.Button == MouseButtons.Left)
                        frm.Panel.ModifyOverlayTranslation(mousePos.X - initialPos.X, mousePos.Y - initialPos.Y);
                    else if (args.Button == MouseButtons.Right)
                        frm.Panel.ModifyOverlayScale(mousePos.X - initialPos.X);
                    Cursor.Position = frm.InitialMousePos;
                };
            }
            HudPreview.Show();
            HudPreview.Focus();
        }

        internal void ShowPanel(bool show)
        {
            if (PanelVisible != show)
            {
                if (NotificationApp.Settings.UseOpenVR)
                {
                    if (PanelVisible)
                    {
                        var overlayError = OpenVR.Overlay?.HideOverlay(OverlayHandle);
                    }
                    else
                    {
                        var overlayError = OpenVR.Overlay?.ShowOverlay(OverlayHandle);
                        PanelUpdated = 2;
                    }
                }
                PanelVisible = show;
                //Debug.WriteLine("Panel " + Name + " visibility set to " + show);
            }
        }

        private VROverlayIntersectionResults_t IntersectionResult = new VROverlayIntersectionResults_t();

        private float CurrentAlpha;

        internal bool LookAt(ref VROverlayIntersectionParams_t intersectionParams)
        {
            if (Settings.Alpha == Settings.UnfocusAlpha)
                return false;

            if (NotificationApp.Settings.UseOpenVR)
            {
                if (OpenVR.Overlay.ComputeOverlayIntersection(OverlayHandle, ref intersectionParams, ref IntersectionResult))
                {
                    if (CurrentAlpha != Settings.Alpha)
                    {
                        CurrentAlpha += 0.005f;
                        if (CurrentAlpha > Settings.Alpha) CurrentAlpha = Settings.Alpha;
                        OpenVR.Overlay.SetOverlayAlpha(OverlayHandle, CurrentAlpha);
                        return true;
                    }
                    //Debug.WriteLine(Name + " intersect.");
                }
                else
                {
                    if (CurrentAlpha != Settings.UnfocusAlpha)
                    {
                        CurrentAlpha -= 0.005f;
                        if (CurrentAlpha < Settings.UnfocusAlpha) CurrentAlpha = Settings.UnfocusAlpha;
                        OpenVR.Overlay.SetOverlayAlpha(OverlayHandle, CurrentAlpha);
                        return true;
                    }
                }
            }
            return false;
        }

        internal void SendOverlay()
        {
            if (NotificationApp.Settings.UseOpenVR)
            {
                var overlayError = OpenVR.Overlay.SetOverlayTransformAbsolute(OverlayHandle, ETrackingUniverseOrigin.TrackingUniverseSeated, ref Settings.Position);
                overlayError = OpenVR.Overlay.SetOverlayTexture(OverlayHandle, ref OVRTexture);
            }
        }

        public int PanelUpdated { get; set; } = 0;

        public static Dictionary<string, object> EDSMSystemInfo { get; private set; } = new Dictionary<string, object>();
        public static Dictionary<string, object>[] EDSMNearbySystems { get; private set; } = new Dictionary<string, object>[0];


        public static void JournalUpdate(string eventType, Dictionary<string, object> entry, IEnumerable<HudPanel> panelList)
        {
            var replay = entry.GetProperty("IsReplay", false);
            switch (eventType)
            {
                case "StartJump":
                    //started jumping
                    {
                        if (NotificationApp.Settings.EDSMDestinationSystem && entry.GetProperty("JumpType", "") == "Hyperspace")
                        {

                            EDSMSystemInfo.Clear();
                            var systemAddress = entry.GetProperty("SystemAddress", 0UL);
                            if (systemAddress != 0)
                            {
                                EDCommon.RequestEDSMSystemInfo(systemAddress, d =>
                                {
                                    EDSMSystemInfo = d;
                                    if (d.Count > 0)
                                    {
                                        var commander = "an unknown commander";
                                        var bodies = d.GetProperty("bodies", null as ArrayList);
                                        if (bodies != null)
                                        {
                                            if (bodies.Count > 0 && (bodies[0] as IDictionary<string, object>).GetProperty("discovery", null as IDictionary<string, object>, out var disco))
                                            {
                                                var c = disco.GetProperty("commander", "");
                                                if (!string.IsNullOrWhiteSpace(c))
                                                    commander = "commander " + c;
                                            }
                                            NotificationApp.Talk("Destination system was previously discovered by " + commander + ".");
                                        }
                                    }
                                });
                            }
                        }
                    }
                    break;

                case "FSSDiscoveryScan":
                    //end jump
                    {
                        if (NotificationApp.Settings.EDSMNearbySystems)
                        {
                            EDSMNearbySystems = new Dictionary<string, object>[0];
                            var name = entry.GetProperty("SystemName", "");
                            if (!string.IsNullOrEmpty(name))
                            {
                                EDCommon.RequestEDSMNearbySystems(name, d =>
                                {
                                    EDSMNearbySystems = d;
                                    if (d.Length > 0)
                                        NotificationApp.Talk("There are " + d.Length + " previously discovered systems nearby.");
                                });
                            }
                        }
                        break;
                    }
                case "FSDJump":
                    {
                        if (!replay)
                        {
                            var addr = entry.GetProperty("SystemAddress", 0L);
                            NotificationApp.ReplayScans(addr);
                        }
                    }
                    break;
            }

            foreach (var panel in panelList)
            {
                panel.OnJournalUpdate(eventType, entry);
            }

        }

        public abstract void OnJournalUpdate(string eventType, Dictionary<string, object> entry);



        protected abstract void OnRedrawPanel();

        protected bool NeedsRedraw = false;

        protected void Redraw()
        {
            NeedsRedraw = true;
        }

        public void RedrawIfNeeded()
        {
            if (!NeedsRedraw)
                return;
            NeedsRedraw = false;
            OnRedrawPanel();
        }

        internal void RefreshUpdate()
        {
            PanelUpdated = 2;
        }

        internal void EDStatusChanged(StatusFlags status, GUIFocus guiFocus)
        {
            OnEDStatusChanged(status, guiFocus);
        }

        internal virtual void OnEDStatusChanged(StatusFlags status, GUIFocus guiFocus)
        {

        }
    }
}

using EDVRHUD.HUDs;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
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
#if UseOpenVR
                        OpenVR.Overlay.ClearOverlayTexture(OverlayHandle);
                        OpenVR.Overlay.DestroyOverlay(OverlayHandle);
#endif //UseOpenVR
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

        public HmdMatrix34_t OverlayPosition { get { return _overlayPosition; } }

        private HmdMatrix34_t _overlayPosition;

        private ulong OverlayHandle = 0;

        public string Name { get; private set; }

        public Size PanelSize { get; private set; }

        public Size TextureSize { get; private set; }

        public HudType Type { get; private set; }

        private readonly DataBox[] TextureDataBox = new[] { new DataBox() };
        private Texture2D RenderTexture;
        private RenderTargetView RenderView;
        private Texture_t OVRTexture;

        private IntPtr OffScreenPtr = IntPtr.Zero;

        public HudPanel(HudType type, string name, Size size, HmdMatrix34_t position)
        {
            _overlayPosition = position;
            Type = type;
            Name = name;
            PanelSize = new Size(size.Width, size.Height);
            TextureSize = new Size(size.Width * 2, size.Height * 2);
            OffScreenPtr = Marshal.AllocHGlobal(TextureSize.Width * TextureSize.Height * 4);

            var m = new Matrix()
            {
                M11 = position.m0,
                M12 = position.m1,
                M13 = position.m2,
                M14 = position.m3,

                M21 = position.m4,
                M22 = position.m5,
                M23 = position.m6,
                M24 = position.m7,

                M31 = position.m8,
                M32 = position.m9,
                M33 = position.m10,
                M34 = position.m11,
            };

            //pitch ile roll ters olabilir
            Matrix.GetYawPitchRoll(ref m, out yaw, out pitch, out roll);

        }

        private bool _initialized = false;

        private bool PanelVisible { get; set; }

        public virtual void Initialize()
        {
            if (_initialized)
                return;

            var hDesktopDC = Native.GetDC(Native.GetDesktopWindow());
#if UseOpenVR
            var overlayError = OpenVR.Overlay.CreateOverlay("OVRHUD_" + Name.ToLowerInvariant().Replace(" ", "_"), Name, ref OverlayHandle);
            overlayError = OpenVR.Overlay.SetOverlayFlag(OverlayHandle, VROverlayFlags.VisibleInDashboard, true);
            overlayError = OpenVR.Overlay.SetOverlayFlag(OverlayHandle, VROverlayFlags.NoDashboardTab, true);
            overlayError = OpenVR.Overlay.SetOverlayTransformAbsolute(OverlayHandle, ETrackingUniverseOrigin.TrackingUniverseSeated, ref _overlayPosition);
            overlayError = OpenVR.Overlay.SetOverlayWidthInMeters(OverlayHandle, PanelSize.Width / 1000f);
            ShowPanel(true);
#endif //UseOpenVR

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
                    return new JumpInfoPanel(new Size(settings.Width, settings.Height), ref settings.Position);
                case HudType.ScanInfo:
                    return new ScanInfoPanel(new Size(settings.Width, settings.Height), ref settings.Position);
                case HudType.Warning:
                    return new WarningPanel(new Size(settings.Width, settings.Height), ref settings.Position);
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
            ShowPanel(false);                
        }

        public void ModifyOverlay(int dx, int dy)
        {
            if (dx == 0 && dy == 0) return;
            Debug.WriteLine("Modifiying panel " + Name + " " +  dx + "," + dy);

            if (Native.IsKeyDown(Keys.LControlKey))
            {
                //Z pos
                _overlayPosition.m11 += dy / 1000f;
            }
            else if (Native.IsKeyDown(Keys.LShiftKey))
            {
                //X and Y pos
                _overlayPosition.m7 -= dy / 1000f;
                _overlayPosition.m3 += dx / 1000f;
            }
            else if (Native.IsKeyDown(Keys.LControlKey))
            {
                //Z rot
                roll += dx / 1000.0;
                var q = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
                var m = q.ToMatrix();
                _overlayPosition.m0 = (float)m.M11;
                _overlayPosition.m1 = (float)m.M12;
                _overlayPosition.m2 = (float)m.M13;

                _overlayPosition.m4 = (float)m.M21;
                _overlayPosition.m5 = (float)m.M22;
                _overlayPosition.m6 = (float)m.M23;

                _overlayPosition.m8 = (float)m.M31;
                _overlayPosition.m9 = (float)m.M32;
                _overlayPosition.m10 = (float)m.M33;
            }
            else if (Native.IsKeyDown(Keys.RShiftKey))
            {
                //X and Y rot
                yaw += dx / 1000.0;
                pitch += dy / 1000.0;
                var q = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
                var m = q.ToMatrix();
                _overlayPosition.m0 = (float)m.M11;
                _overlayPosition.m1 = (float)m.M12;
                _overlayPosition.m2 = (float)m.M13;

                _overlayPosition.m4 = (float)m.M21;
                _overlayPosition.m5 = (float)m.M22;
                _overlayPosition.m6 = (float)m.M23;

                _overlayPosition.m8 = (float)m.M31;
                _overlayPosition.m9 = (float)m.M32;
                _overlayPosition.m10 = (float)m.M33;
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

                HudPreview.KeyPress += (sender, args) =>
                {                    
                    NotificationApp.SimulateNextSystem = true;
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
                    frm.Panel.ModifyOverlay(mousePos.X - initialPos.X, mousePos.Y - initialPos.Y);
                    Cursor.Position = frm.InitialMousePos;
                };
            }
            HudPreview.Show();
            HudPreview.Focus();
        }

        internal void ShowPanel(bool show)
        {
#if UseOpenVR
            var overlayError =
                show ?
                OpenVR.Overlay.ShowOverlay(OverlayHandle) :
                OpenVR.Overlay.HideOverlay(OverlayHandle);
#endif //#if UseOpenVR
            PanelVisible = show;
        }
        
        internal void SendOverlay()
        {
#if UseOpenVR
            var overlayError = OpenVR.Overlay.SetOverlayTransformAbsolute(OverlayHandle, ETrackingUniverseOrigin.TrackingUniverseSeated, ref _overlayPosition);
            overlayError = OpenVR.Overlay.SetOverlayTexture(OverlayHandle, ref OVRTexture);
#endif //UseOpenVR
        }

        public int PanelUpdated { get; protected set; } = 0;

        public abstract void JournalUpdate(string eventType, Dictionary<string, object> entry);

        protected abstract void Redraw();

        internal void RefreshUpdate()
        {
            PanelUpdated = 2;
        }
    }
}

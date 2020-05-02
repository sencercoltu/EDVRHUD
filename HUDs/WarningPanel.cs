using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using Valve.VR;

namespace EDVRHUD.HUDs
{
    internal class WarningPanel : HudPanel
    {
        private string DangerousStarName = "";

        public WarningPanel(Size size, ref HmdMatrix34_t position) : base(HudType.Warning, "Warning", size, position)
        {
            ShowPanel(false);
        }

        public override void Initialize()
        {
            base.Initialize();
            ShowPanel(false);
        }

        protected override void StartModifyOverlay()
        {
            base.StartModifyOverlay();
            DangerousStarName = "Test Star";
            Redraw();
        }

        protected override void EndModifyOverlay()
        {
            base.EndModifyOverlay();
            DangerousStarName = "";
        }

        public override void JournalUpdate(string eventType, Dictionary<string, object> eventData)
        {
            switch (eventType)
            {
                case "StartJump":
                    //show warning if blackhole, neutronstar, white-dwarf etc
                    {
                        if (eventData.GetProperty("JumpType", "") == "Hyperspace")
                        {
                            var sc = eventData.GetProperty("StarClass", "");
                            if (!string.IsNullOrEmpty(sc))
                            {
                                if (EDCommon.StarLookup.TryGetValue(sc, out var star))
                                {
                                    if (star.Type == EDCommon.StarType.Dangerous)
                                    {
                                        DangerousStarName = star.PlainName;
                                        ShowPanel(true);
                                    }
                                    else
                                        DangerousStarName = "";
                                }
                                else
                                    DangerousStarName = "Unknown star (" + sc + ")";
                                Redraw();
                                //NotificationApp.Speech.SpeakAsyncCancelAll();
                                if (!string.IsNullOrEmpty(DangerousStarName))
                                    NotificationApp.Talk("Warning! Target is a " + DangerousStarName + ".");
                            }
                        }
                    }
                    break;
                case "FSDJump":
                    //end warning
                    {
                        DangerousStarName = "";
                        Redraw();
                        ShowPanel(false);
                    }
                    break;
            }
        }

        private Brush BackgroundBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0));
        private Brush WarningBrush = new SolidBrush(Color.FromArgb(255, 255, 0, 0));

        private StringFormat WarningFormat = new StringFormat
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Center
        };
        
        private int IconSize = 80;

        private Pen TopLinePen = new Pen(Color.FromArgb(255, 200, 0, 0), 2f);
        private Pen BottomLinePen = new Pen(Color.FromArgb(255, 255, 0, 0), 2f);


        protected override void Redraw()
        {
            var imgOrigin = new Point(0, (TextureSize.Height - IconSize) / 2);


            var rect = new Rectangle(IconSize, 0, TextureSize.Width - IconSize, TextureSize.Height);

            using (var g = GetGraphics())
            {
                g.Clear(NotificationApp.DefaultClearColor);
                g.FillRectangle(BackgroundBrush, 0, 0, TextureSize.Width, TextureSize.Height);

                if (!string.IsNullOrEmpty(DangerousStarName))
                {
                    g.DrawLine(TopLinePen, 0, 1, TextureSize.Width, 1);
                    g.DrawLine(BottomLinePen, 0, TextureSize.Height - 1, TextureSize.Width, TextureSize.Height - 1);

                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.TextRenderingHint = TextRenderingHint.AntiAlias;                    
                    g.DrawImage(Properties.Resources.Skull, imgOrigin.X, imgOrigin.Y, IconSize, IconSize);
                    var str = "WARNING!" + Environment.NewLine + "Approaching " + DangerousStarName + "." + Environment.NewLine + "Throttle down now.";
                    g.DrawString(str, NotificationApp.EDFont, WarningBrush, rect, WarningFormat);
                }
                g.Flush();
            }
            PanelUpdated = 2;
        }
    }
}

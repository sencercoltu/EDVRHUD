using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;

namespace EDVRHUD.HUDs
{
    internal class WarningPanel : HudPanel
    {
        private string DangerousStarName = "";

        public WarningPanel(PanelSettings settings) : base("Warning", settings)
        {
            ShowPanel(false);
            SubscribeEvents(
                "StartJump", 
                "FSDJump");
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
            ShowPanel(false);
        }

        public override void OnJournalUpdate(string eventType, Dictionary<string, object> eventData)
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
                                    if (star.Type == StarType.Dangerous)
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
                                    EDCommon.Talk("Warning! Destination is a " + DangerousStarName + ".");
                            }
                        }
                        else
                        {
                            DangerousStarName = "";
                            Redraw();
                            ShowPanel(false);
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
        private Pen TopLinePen = new Pen(Color.FromArgb(255, 200, 0, 0), 3f);
        private Pen BottomLinePen = new Pen(Color.FromArgb(255, 255, 0, 0), 3f);

        protected override void OnRedrawPanel()
        {
            //var imgOrigin = new Point(0, (TextureSize.Height - IconSize) / 2);


            var rect = new Rectangle(0, 0, TextureSize.Width, TextureSize.Height);

            using (var g = GetGraphics())
            {
                g.Clear(NotificationApp.DefaultClearColor);
                g.FillRectangle(BackgroundBrush, 0, 0, TextureSize.Width, TextureSize.Height);

                if (!string.IsNullOrEmpty(DangerousStarName))
                {
                    g.DrawLine(TopLinePen, 0, 1, TextureSize.Width, 1);
                    g.DrawLine(BottomLinePen, 0, TextureSize.Height - 2, TextureSize.Width, TextureSize.Height - 2);

                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.TextRenderingHint = TextRenderingHint.AntiAlias;                    
                    //g.DrawImage(Properties.Resources.Skull, imgOrigin.X, imgOrigin.Y, IconSize, IconSize);
                    var str = "WARNING!" + Environment.NewLine + "Approaching a " + DangerousStarName + "." + Environment.NewLine + "Throttle down advised.";
                    g.DrawString(str, NotificationApp.EDFont, WarningBrush, rect, WarningFormat);
                }
                g.Flush();
            }
            PanelUpdated = 2;
        }
    }
}

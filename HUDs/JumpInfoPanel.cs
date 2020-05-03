using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using Valve.VR;

namespace EDVRHUD.HUDs
{
    internal class JumpInfoPanel : HudPanel
    {
        //public JumpInfoPanel(Size size, ref HmdMatrix34_t position) : base(HudType.JumpInfo, "JumpInfo", size, position)
        //{            
        //}

        public JumpInfoPanel(PanelSettings settings) : base("JumpInfo", settings)
        {
        }

        private GUIFocus[] VisibleGuiFocus = new[] { GUIFocus.None, GUIFocus.InternalPanel, GUIFocus.ExternalPanel, GUIFocus.CommsPanel, GUIFocus.RolePanel };        

        internal override void EDStatusChanged(StatusFlags status, GUIFocus guiFocus)
        {
            if (VisibleGuiFocus.Contains(guiFocus) && status.HasFlag(StatusFlags.InMainShip))
                ShowPanel(true);
            else
                ShowPanel(false);
        }

        private string TargetStarType = "";
        private string TargetStarSystem = "";
        private int RemainingJumps = 0;
        private double JetConeBoost = 1;
        private bool ActiveRoute = false;
        public override void JournalUpdate(string eventType, Dictionary<string, object> eventData)
        {
            switch (eventType)
            {
                case "JetConeBoost":
                    {
                        if (eventData.GetProperty("BoostValue", 0.0, out var boost))
                        {
                            JetConeBoost = boost;
                            NotificationApp.Talk("FSD boosted by " + JetConeBoost.ToString("0.#") + " times.");
                            Redraw();
                        }
                    }
                    break;
                case "StartJump":
                    //started charging
                    {
                        if (eventData.GetProperty("JumpType", "") == "Hyperspace")
                        {
                            if (EDCommon.StarLookup.TryGetValue(eventData.GetProperty("StarClass", "Unknown"), out var star))
                                TargetStarType = star.TypeName;
                            TargetStarSystem = eventData.GetProperty("StarSystem", "Unknown");
                            if (RemainingJumps > 0) RemainingJumps--;
                            Redraw();
                        }
                    }
                    break;
                case "FSDTarget":
                    //started jumping
                    {
                        RemainingJumps = eventData.GetProperty("RemainingJumpsInRoute", 0);
                        if (RemainingJumps > 0)
                            ActiveRoute = true;
                        Redraw();
                    }
                    break;
                case "FSDJump":
                    //end jump
                    {
                        //reset jcb
                        //if (RemainingJumps == 1) RemainingJumps = 0;
                        NotificationApp.Shutup();                                     
                        JetConeBoost = 1;
                        Redraw();
                        if (RemainingJumps == 0 && ActiveRoute)
                        {
                            ActiveRoute = false;
                            NotificationApp.Talk("Reached destination " + TargetStarSystem);
                        }

                    }
                    break;
            }
        }

        private Brush BackgroundBrush = new SolidBrush(Color.FromArgb(5, 0, 0, 0));

        private Pen TopLinePen = new Pen(Color.FromArgb(255, 171, 80, 6), 3f);
        private Pen BottomLinePen = new Pen(Color.FromArgb(255, 194, 102, 7), 3f);

        private int IconSize = 40;
        protected override void Redraw()
        {
            var w = TextureSize.Width / 8;
            using (var g = GetGraphics())
            {
                g.Clear(NotificationApp.DefaultClearColor);
                g.FillRectangle(BackgroundBrush, 0, 0, TextureSize.Width, TextureSize.Height);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                g.DrawLine(TopLinePen, 0, 1, TextureSize.Width, 1);
                g.DrawLine(BottomLinePen, 0, TextureSize.Height - 2, TextureSize.Width, TextureSize.Height - 2);

                var x = 0;
                var y = 3;
                g.DrawImage(Properties.Resources.Star, x, y, IconSize, IconSize);
                x += IconSize;
                var str = " " + (string.IsNullOrEmpty(TargetStarType)? "?" : TargetStarType); 
                g.DrawString(str, NotificationApp.EDFont, NotificationApp.DefaultBrush, x, y + 4);
                
                
                x = w * 5;
                g.DrawImage(Properties.Resources.Jump, x, y, IconSize, IconSize);
                x += IconSize;
                str = "    " + RemainingJumps.ToString(); str = str.Substring(str.Length - 4);                                
                g.DrawString(str, NotificationApp.EDFont, NotificationApp.DefaultBrush, x, y + 4);
                x += w;

                g.DrawImage(JetConeBoost > 1? Properties.Resources.JetBoosted : Properties.Resources.JetBoost, x, y, IconSize, IconSize);
                x += IconSize;
                str = " " + JetConeBoost.ToString("N1", CultureInfo.InvariantCulture);
                g.DrawString(str, NotificationApp.EDFont, NotificationApp.DefaultBrush, x, y + 4);

                g.Flush();
            }
            PanelUpdated = 2;
        }
    }
}

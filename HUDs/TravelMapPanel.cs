using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Windows.Forms;
using Valve.VR;

namespace EDVRHUD.HUDs
{
    internal class TravelMapPanel : HudPanel
    {

        public TravelMapPanel(PanelSettings settings) : base("TravelMap", settings)
        {
            IsInteractive = true;
            SubscribeEvents(
                "FSDJump",
                "Location");
             MapShift = new PointF(0, 0);
        }

        private GUIFocus[] VisibleGuiFocus = new[] { /*GUIFocus.None, */ GUIFocus.InternalPanel /*, GUIFocus.ExternalPanel, GUIFocus.CommsPanel, GUIFocus.RolePanel*/ };

        internal override void OnEDStatusChanged(StatusFlags status, GUIFocus guiFocus)
        {
            if (VisibleGuiFocus.Contains(guiFocus) && status.HasFlag(StatusFlags.InMainShip))
                ShowPanel(true);
            else
                ShowPanel(false);
        }

        public override void OnJournalUpdate(string eventType, Dictionary<string, object> eventData)
        {
            switch (eventType)
            {
                case "FSDJump":
                case "Location":                    
                    {
                        EDCommon.AddTravelData(eventData);
                        Redraw();
                    }
                    break;
            }
        }

        private Brush BackgroundBrush = new SolidBrush(Color.FromArgb(5, 0, 0, 0));
        private Brush NonScoopableBrush = new SolidBrush(Color.FromArgb(255, 75, 0));
        
        private Pen TopLinePen = new Pen(Color.FromArgb(255, 171, 80, 6), 3f);
        private Pen BottomLinePen = new Pen(Color.FromArgb(255, 194, 102, 7), 3f);
        private Pen TravelPen = new Pen(Color.FromArgb(255, 255, 255, 255), 1f) { DashStyle = DashStyle.Dash, DashPattern = new float[] { 10.0F, 10.0F } };
        private Pen StarPen = new Pen(Color.FromArgb(255, 0, 0, 0), 3f);
        private Pen PositionPen = new Pen(Color.FromArgb(255, 0, 255, 0), 4f);
        private Pen ResetPen = new Pen(Color.FromArgb(255, 255, 0, 0), 3f);


        private float Zoom = 20.0f;
        private bool AutoTrack = true;

        private const float ZoomStep = 1.05f;

        public override void OnScroll(MouseEventArgs e)
        {
            Debug.WriteLine("scroll");
            Zoom *= Math.Sign(e.Delta) > 0 ? ZoomStep : 1f / ZoomStep;
            if (Zoom < 1)
                Zoom = 1;
            if (Zoom > 5000)
                Zoom = 5000;
            Redraw();
        }

        public override void OnClick(Point point)
        {
            //Debug.WriteLine("click");
        }

        private PointF MapShift;

        protected override void OnKeypress(KeyPressEventArgs e)
        {
            switch(e.KeyChar)
            {
                case 'w':
                case 'W':
                    MapShift.Y += 1000 / Zoom;
                    if (MapShift.Y > 50000) MapShift.Y = 50000;
                    break;
                case 's':
                case 'S':
                    MapShift.Y -= 1000 / Zoom;
                    if (MapShift.Y < -50000) MapShift.Y = -50000;
                    break;
                case 'a':
                case 'A':
                    MapShift.X -= 1000 / Zoom;
                    if (MapShift.X < -50000) MapShift.X = -50000;
                    break;
                case 'd':
                case 'D':
                    MapShift.X += 1000 / Zoom;
                    if (MapShift.X > 50000) MapShift.X = 50000; 
                    break;
            }
            Redraw();
        }

        protected override void OnRedrawPanel()
        {
            var w = TextureSize.Width / 2f;
            var h = TextureSize.Height / 2f;
            using (var g = GetGraphics())
            {
                g.Clear(Color.Black);
                //g.FillRectangle(BackgroundBrush, 0, 0, TextureSize.Width, TextureSize.Height);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                               

                var mapW = TextureSize.Width * Zoom;
                var mapH = TextureSize.Height * Zoom;


                var mapX = w - ((w + MapShift.X / 45000f * w) * Zoom);
                var mapY = h - ((h - (MapShift.Y - 25000f) / 45000f * h) * Zoom);

                g.DrawImage(Properties.Resources.Galaxy, mapX, mapY, mapW, mapH);

                PointF pt = new PointF(w, h);
                TravelInfo lastInfo = null;
                if (EDCommon.TravelMapData.Any())
                {
                    PointF lastLoc = pt;
                    foreach (var item in EDCommon.TravelMapData)
                    {
                        pt = new PointF(w + (item.Coords[0] - MapShift.X) / 45000f * w * Zoom, h - (item.Coords[2] - MapShift.Y) / 45000f * h * Zoom);
                        if (!item.IsReset)
                        {
                            g.DrawLine(TravelPen, lastLoc.X, lastLoc.Y, pt.X, pt.Y);
                            g.DrawEllipse(StarPen, pt.X - 1, pt.Y - 1, 5, 5);
                        }
                        else
                            g.DrawEllipse(ResetPen, pt.X - 2, pt.Y - 2, 5, 5);

                        lastLoc = pt;
                        lastInfo = item;
                    }
                    g.DrawEllipse(PositionPen, pt.X - 2, pt.Y - 2, 5, 5); //current loc
                    if (AutoTrack && lastInfo != null)
                    {
                        MapShift.X = lastInfo.Coords[0];
                        MapShift.Y = lastInfo.Coords[2];
                    }
                        
                }

                g.DrawLine(TopLinePen, 0, 1, TextureSize.Width, 1);
                g.DrawLine(BottomLinePen, 0, TextureSize.Height - 2, TextureSize.Width, TextureSize.Height - 2);

                g.Flush();
            }
            PanelUpdated = 2;
        }
    }
}

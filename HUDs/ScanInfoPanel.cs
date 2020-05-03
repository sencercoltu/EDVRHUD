using SharpDX.Direct3D11;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using Valve.VR;

namespace EDVRHUD.HUDs
{
    internal class BodyInfo
    {
        public string Name { get; set; }
        public int BodyID { get; set; }
        public string BodyType { get; set; }

        public string FullName { get { return BodyType + " " + Name; } }

        public string StarName { get; set; }

        public int StarSubClass { get; set; }
        public string StarLuminosity { get; set; }

        public bool IsStar { get; set; }

        public bool IsPlanet { get; set; }

        public bool Landable { get; set; }
        public bool PreviouslyDiscovered { get; set; }
        public bool PreviouslyMapped { get; set; }

        public bool SelfMapped { get; set; }
        public bool MapEfficency { get; set; }

        public string TerraformState { get; set; }
        public bool Terraformable { get { return !string.IsNullOrEmpty(TerraformState); } }
        public int Signals { get; set; }

        public double Mass { get; set; }

        public double Gravity { get; set; }

        public double Distance { get; set; }

        public string Atmosphere { get; set; } = "None";

        public bool HasAtmosphere { get { return Atmosphere != "None"; } }

        public double Temperature { get; set; }
        public double Pressure { get; set; }

        public double OrbitalPeriod { get; set; }
        public double RotationPeriod { get; set; }

        public double Radius { get; set; }

        public int GeoSignals { get; set; }

        public int BioSignals { get; set; }

    internal bool AlreadyScannedInternal { get; set; }
        public MaterialType MaterialTypes { get; set; } = MaterialType.None;

        public string Features { get; set; } = "";
        public int Rings { get; set; } = 0;
        public bool HasBasicJump { get { return (MaterialTypes & Material.BasicBoostMaterials) == Material.BasicBoostMaterials; } }
        public bool HasStandardJump { get { return (MaterialTypes & Material.StandardBoostMaterials) == Material.StandardBoostMaterials; } }
        public bool HasPremiumJump { get { return (MaterialTypes & Material.PremiumBoostMaterials) == Material.PremiumBoostMaterials; } }
        public bool HasAllSurfaceMaterials { get { return (MaterialTypes & Material.AllSurfaceMaterials) == Material.AllSurfaceMaterials; } }
        //public bool HasAllMaterials { get { return (MaterialTypes & EDCommon.Material.AllMaterials) == EDCommon.Material.AllMaterials; } }

        public double Value { get; set; }

        public Dictionary<MaterialType, double> MaterialPercents { get; } = new Dictionary<MaterialType, double>();
    }

    internal class ScanInfo
    {
        public MaterialType SystemMaterialTypes { get; set; } = MaterialType.None;
        public long SystemAddress { get; set; } = 0;

        public string SystemName { get; set; }
        public double Progress { get; set; } = 0;
        public int Signals { get; set; } = 0;
        public int BodyCount { get; set; } = 0;
        public bool Discovered { get; set; } = false;
        public bool WarnedValuable { get; set; } = false;

        public Dictionary<int, BodyInfo> Bodies { get; } = new Dictionary<int, BodyInfo>();

        public void Clear()
        {
            WarnedValuable = false;
            Discovered = false;
            BodyCount = 1; //at least one star
            Signals = 0;
            Progress = 0;
            SystemAddress = 0;
            SystemName = "";
            SystemMaterialTypes = MaterialType.None;
            Bodies.Clear();
        }

        public bool HasBasicJump { get { return (SystemMaterialTypes & Material.BasicBoostMaterials) == Material.BasicBoostMaterials; } }
        public bool HasStandardJump { get { return (SystemMaterialTypes & Material.StandardBoostMaterials) == Material.StandardBoostMaterials; } }
        public bool HasPremiumJump { get { return (SystemMaterialTypes & Material.PremiumBoostMaterials) == Material.PremiumBoostMaterials; } }
        public bool HasGoldMaterials { get { return (SystemMaterialTypes & Material.AllSurfaceMaterials) == Material.AllSurfaceMaterials; } }
        public bool HasAllMaterials { get { return (SystemMaterialTypes & Material.AllMaterials) == Material.AllMaterials; } }
    }

    internal class ScanInfoPanel : HudPanel
    {
        private ScanInfo Scan = new ScanInfo();

        private Font PanelFont;

        

        //public ScanInfoPanel(Size size, ref HmdMatrix34_t position) : base(HudType.ScanInfo, "ScanInfo", size, position)
        //{
        //    PanelFont = new Font(NotificationApp.EDFont.FontFamily, 18);
        //}

        public ScanInfoPanel(PanelSettings settings) : base("ScanInfo", settings)
        {
            PanelFont = new Font(NotificationApp.EDFont.FontFamily, 18);
        }

        internal override void EDStatusChanged(StatusFlags status, GUIFocus guiFocus)
        {            
            if (!Scan.WarnedValuable && status.HasFlag(StatusFlags.FsdCharging))
            {
                Scan.WarnedValuable = true;
                //warn if non mapped valuable planets left
                var cnt = Scan.Bodies.Count(p => !p.Value.SelfMapped && p.Value.Terraformable);
                if (cnt > 0)
                    NotificationApp.Talk("Leaving system with " + cnt + " unmapped valuable bod" + (cnt == 1? "y" : "ies") + ".");
            }

            if (guiFocus == GUIFocus.RolePanel || !status.HasFlag(StatusFlags.InMainShip))
                ShowPanel(false);
            else 
                ShowPanel(true);
        }


        private System.Timers.Timer SignalTimer = null;

        public override void JournalUpdate(string eventType, Dictionary<string, object> eventData)
        {
            switch (eventType)
            {
                case "DiscoveryScan":
                    {
                        //ignore
                    }
                    break;
                case "FSSSignalDiscovered":
                    {                        
                        Scan.Signals++;
                        //delay signal speech
                        if (SignalTimer != null)
                        {
                            SignalTimer.Stop();
                            SignalTimer.Dispose();
                            SignalTimer = null;
                        }
                        SignalTimer = new System.Timers.Timer(1000);
                        SignalTimer.AutoReset = false;
                        SignalTimer.Elapsed += (sender, args) =>
                        {
                            NotificationApp.Talk(Scan.Signals + " signal" + (Scan.Signals > 1 ? "s" : "") + " detected in system.");
                            SignalTimer.Dispose();
                            SignalTimer = null;
                        };
                        SignalTimer.Start();
                        
                        Redraw();
                    }
                    break;
                case "FSSDiscoveryScan":
                    {
                        if (eventData.GetProperty("BodyCount", 0, out var bc))
                            Scan.BodyCount = bc;
                        Scan.Progress = eventData.GetProperty("Progress", 0.0);
                        Redraw();
                    }
                    break;
                case "FSSAllBodiesFound":
                    {
                        if (eventData.GetProperty("Count", 0, out var bc))
                        {
                            Scan.BodyCount = bc;
                            Scan.Progress = 1;
                        }
                        Redraw();
                    }
                    break;
                case "SAASignalsFound":
                    {
                        var signals = (eventData.GetProperty("Signals", null as ArrayList) ?? new ArrayList()).ToArray();
                        if (eventData.GetProperty("BodyID", -1, out var bodyId))
                        {
                            if (!Scan.Bodies.TryGetValue(bodyId, out BodyInfo body))
                                return;
                            body.BioSignals = signals.Count(s => (s as Dictionary<string, object>).GetProperty("type","") == "$SAA_SignalType_Biological;");
                            body.GeoSignals = signals.Count(s => (s as Dictionary<string, object>).GetProperty("type", "") == "$SAA_SignalType_Geological;");
                            Redraw();

                            if (body.BioSignals > 0)
                                NotificationApp.Talk("Body " + body.Name + " has " + body.BioSignals + " biological signals.");
                            if (body.GeoSignals > 0)
                                NotificationApp.Talk("Body " + body.Name + " has " + body.BioSignals + " geological signals.");
                        }
                    }
                    break;
                case "SAAScanComplete":
                    {
                        if (eventData.GetProperty("BodyID", -1, out var bodyId))
                        {
                            if (!Scan.Bodies.TryGetValue(bodyId, out BodyInfo body))
                                return;
                            //"BodyID":3, "ProbesUsed":4, "EfficiencyTarget":6 }
                            var pu = eventData.GetProperty("ProbesUsed", 0);
                            var et = eventData.GetProperty("EfficiencyTarget", 0);
                            body.SelfMapped = true;
                            body.MapEfficency = pu <= et;
                            EDCommon.GetBodyValue(body);
                            if (body.IsPlanet && body.Value > 10000)
                            {
                                var val = ((int)(body.Value / 1000) * 1000).ToString("N0");
                                NotificationApp.Talk("Mapped " + body.Name + "." + body.TerraformState + " " + body.BodyType + ", " + val + " credits.");
                            }
                            Redraw();
                        }
                    }
                    break;
                case "Scan":
                    {
                        if (eventData.GetProperty("BodyID", -1, out var bodyId))
                        {
                            var bodyName = eventData.GetProperty("BodyName", "");
                            if (bodyName.EndsWith(" Ring")) return;
                            //skip asteriod belts                            
                            var parents = eventData.GetProperty("Parents", null as ArrayList) ?? new ArrayList();
                            foreach (Dictionary<string, object> parent in parents)
                            {
                                if (parent.ContainsKey("Ring"))
                                {
                                    //Debug.WriteLine("Skipping " + eventData.GetProperty("BodyName", ""));
                                    return;
                                }
                            }

                            var discovered = eventData.GetProperty("WasDiscovered", false);
                            var mapped = eventData.GetProperty("WasDiscovered", false);                            

                            var starType = eventData.GetProperty("StarType", "");

                            if (!Scan.Bodies.TryGetValue(bodyId, out BodyInfo body))
                            {
                                body = new BodyInfo
                                {
                                    BodyID = bodyId,
                                    Name = bodyName
                                };


                                //simplify body name
                                //if (string.IsNullOrEmpty(starType))
                                //{
                                    var starSystem = eventData.GetProperty("StarSystem", "");
                                    body.Name = body.Name.Substring(starSystem.Length).Trim();
                                //}

                                Scan.Bodies.Add(bodyId, body);
                                Scan.Progress = (double)Scan.Bodies.Count / (double)Scan.BodyCount;
                            }

                            body.PreviouslyDiscovered = discovered;
                            body.PreviouslyMapped = mapped;
                            body.Distance = eventData.GetProperty("DistanceFromArrivalLS", 0.0);

                            Scan.Discovered = Scan.Bodies.Any(b => b.Value.PreviouslyDiscovered);

                            if (!string.IsNullOrEmpty(starType))
                            {
                                body.IsStar = true;
                                body.BodyType = starType;
                                if (EDCommon.StarLookup.TryGetValue(starType, out var s))
                                    body.StarName = s.TypeName;
                                else
                                    body.StarName = starType;
                                body.StarSubClass = eventData.GetProperty("Subclass", 0);
                                body.StarLuminosity = eventData.GetProperty("Luminosity", "");
                                body.Mass = eventData.GetProperty("StellarMass", 0.0);
                            }
                            else
                            {
                                body.IsPlanet = true;
                                body.BodyType = eventData.GetProperty("PlanetClass", "");
                                body.Rings = (eventData.GetProperty("Rings", null as ArrayList) ?? new ArrayList()).Count;
                                body.Gravity = eventData.GetProperty("SurfaceGravity", 0.0) / 9.81; //G
                                body.Atmosphere = eventData.GetProperty("AtmosphereType", "None");
                                body.Temperature = eventData.GetProperty("SurfaceTemperature", 0.0);
                                body.Pressure = eventData.GetProperty("SurfacePressure", 0.0);
                                body.OrbitalPeriod = Math.Abs(eventData.GetProperty("OrbitalPeriod", 0.0) / 3600); //hours
                                body.RotationPeriod = Math.Abs(eventData.GetProperty("RotationPeriod", 0.0) / 3600); //hours
                                body.Radius = eventData.GetProperty("Radius", 0.0) / 1000; //km
                                body.Mass = eventData.GetProperty("MassEM", 0.0);
                                body.Landable = eventData.GetProperty("Landable", false);
                                body.TerraformState = eventData.GetProperty("TerraformState", "");
                                if (body.BodyType == "Earthlike body") body.TerraformState = " ";
                                var materials = eventData.GetProperty("Materials", null as ArrayList) ?? new ArrayList();
                                foreach (Dictionary<string, object> m in materials)
                                {
                                    var name = m.GetProperty("Name", "");
                                    var perc = m.GetProperty("Percent", 0.0);

                                    if (EDCommon.MaterialLookup.TryGetValue(name, out var material))
                                    {
                                        body.MaterialTypes |= material.MaterialType;
                                        body.MaterialPercents[material.MaterialType] = perc;
                                        Scan.SystemMaterialTypes |= material.MaterialType;
                                    }
                                }

                                var features = "";

                                if (body.Terraformable) features += " T ";
                                //if (body.HasAllMaterials) features += " A ";
                                if (body.HasAllSurfaceMaterials) features += " A ";
                                else
                                {
                                    if (body.HasPremiumJump) features += " P ";
                                    if (body.HasStandardJump) features += " S ";
                                    else if (body.HasBasicJump) features += " B ";//standard containd basic
                                }


                                if (body.HasAtmosphere && body.Landable) features += " LA ";
                                if (body.Terraformable && body.Landable) features += " LT ";
                                if (body.Landable)
                                {
                                    if (body.Gravity > 3) features += " HG ";
                                    else if (body.Gravity < 0.03) features += " LG ";
                                }
                                if (body.RotationPeriod <= 1) features += " FR ";
                                if (body.OrbitalPeriod <= 1) features += " FO ";
                                if (body.Radius <= 300) features += " SR ";
                                if (body.Rings > 2) features += " R" + body.Rings + " ";
                                body.Features = features;
                            }

                            EDCommon.GetBodyValue(body);
                            if (body.IsPlanet && !body.AlreadyScannedInternal)
                            {
                                body.AlreadyScannedInternal = true;                                
                                if (body.Value > 20000)
                                {
                                    var val = ((int)(body.Value / 1000) * 1000).ToString("N0");
                                    NotificationApp.Talk("Scanned " + body.Name + "." + body.TerraformState + " " + body.BodyType + ", " + val + " credits.");
                                }

                                var featureText = "";

                                if (body.Features.Contains(" LG "))
                                    featureText += "Body has gravity of " + body.Gravity.ToString("N2") + " G.";
                                else if (body.Features.Contains(" HG "))
                                    featureText += "Warning. Body has a gravity of " + body.Gravity.ToString("N2") + " G.";
                                if (body.Features.Contains(" FR "))
                                    featureText += "Body rotational period is " + body.RotationPeriod.ToString("N1") + " hours.";
                                if (body.Features.Contains(" FO "))
                                    featureText += "Body orbit period is " + body.RotationPeriod.ToString("N1") + " hours.";
                                if (body.Features.Contains(" SR "))
                                    featureText += "Body radius is " + body.RotationPeriod.ToString("N0") + " kilometers.";
                                if (body.Features.Contains(" LA "))
                                    featureText += "Jackpot! Body with atmosphere is landable.";
                                if (body.Features.Contains(" LT "))
                                    featureText += "Jackpot! Terraformable body is landable.";
                                if (body.Features.Contains(" A "))
                                    featureText += "Jackpot! Body has all surface materials.";
                                if (body.Features.Contains(" S "))
                                    featureText += "Body has all standard jumponium materials.";
                                if (body.Features.Contains(" P "))
                                    featureText += "Jackpot! Body has all premium jumponium materials.";
                                if (body.Rings > 2)
                                    featureText += "Body has " + body.Rings + " rings.";

                                if (!string.IsNullOrEmpty(featureText))                                    
                                    NotificationApp.Talk("Body " + body.Name + "." + body.TerraformState + " " + body.BodyType + " scan complete." + featureText);
                            }

                            Redraw();
                        }
                    }
                    break;
                case "FSDJump":
                    //end warning
                    {
                        if (SignalTimer != null)
                        {
                            SignalTimer.Stop();
                            SignalTimer.Dispose();
                            SignalTimer = null;
                        }

                        Scan.Clear();
                        Scan.SystemAddress = eventData.GetProperty("SystemAddress", 0L);
                        Scan.SystemName = eventData.GetProperty("StarSystem", "");
                        Redraw();
                    }
                    break;
            }
        }

        private readonly Brush BackgroundBrush = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
        private readonly Brush MappedBrush = new SolidBrush(Color.FromArgb(255, 250, 250, 50));
        private readonly Brush InterestBrush = new SolidBrush(Color.FromArgb(255, 0, 255, 0));
        private readonly Brush PreviouslyDiscoveredBrush = new SolidBrush(Color.FromArgb(125, 255, 100, 0));
        private readonly Brush PreviouslyMappedBrush = new SolidBrush(Color.FromArgb(255, 250, 30, 50));

        private Pen TopLinePen = new Pen(Color.FromArgb(255, 171, 80, 6), 3f);
        private Pen BottomLinePen = new Pen(Color.FromArgb(255, 194, 102, 7), 3f);


        private int IconSize = 40;
        private int TopPadding = 3;

        protected override void Redraw()
        {
            using (var g = GetGraphics())
            {
                g.Clear(NotificationApp.DefaultClearColor);
                g.FillRectangle(BackgroundBrush, 0, 0, TextureSize.Width, TextureSize.Height);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                var str = " " + Scan.Signals;

                var mats = "";
                if (Scan.HasStandardJump) mats += "S ";
                else if (Scan.HasBasicJump) mats += "B ";
                
                if (Scan.HasPremiumJump) mats += "P ";
                if (Scan.HasGoldMaterials) mats += "G ";
                if (Scan.HasAllMaterials) mats += "A ";


                if (!string.IsNullOrEmpty(mats))
                    str += "  [ " + mats + "]";

                var y = TopPadding + 3;

                g.DrawImage(Scan.Signals > 0 ? Properties.Resources.SignalAvail : Properties.Resources.Signal, 0, 3, IconSize, IconSize);
                g.DrawString(str, NotificationApp.EDFont, Scan.Signals > 0? InterestBrush : NotificationApp.DefaultBrush, IconSize, y);

                str = string.Format(CultureInfo.InvariantCulture, " {0,5:N0}", 100 * Scan.Progress) + " % - " + string.Format("{0,2}", Scan.Bodies.Count) + "/" + string.Format("{0,2}", Scan.BodyCount);
                var size = g.MeasureString(str, NotificationApp.EDFont);
                var percentStart = TextureSize.Width - size.Width;
                g.DrawImage(Properties.Resources.Scan, percentStart - IconSize, 3, IconSize, IconSize);
                g.DrawString(str, NotificationApp.EDFont, NotificationApp.DefaultBrush, percentStart, y);


                y += IconSize;
                var totalCredits = 0.0;

                foreach (var body in Scan.Bodies.Values
                    .OrderByDescending(p => p.Terraformable)
                    .ThenByDescending(p => p.Features.Length)
                    .ThenByDescending(p => p.Value))
                {
                    totalCredits += body.Value;
                    if (body.IsStar)
                        g.DrawImage(Properties.Resources.Star, 0, y, IconSize, IconSize);
                    else if (body.BodyType.ToLowerInvariant().Contains("earthlike body"))
                        g.DrawImage(Properties.Resources.EarthLike, 0, y, IconSize, IconSize);
                    else if (body.BodyType.ToLowerInvariant().Contains("gas giant"))
                        g.DrawImage(Properties.Resources.GasGiant, 0, y, IconSize, IconSize);
                    else
                        g.DrawImage(Properties.Resources.Planet, 0, y, IconSize, IconSize);

                    if (body.Rings > 0) //overlay ring
                        g.DrawImage(Properties.Resources.Ring, 0, y, IconSize, IconSize);

                    if (body.Landable) //overlay landable
                        g.DrawImage(Properties.Resources.Landable, 0, y, IconSize, IconSize);

                    Brush brush = NotificationApp.DefaultBrush;

                    if (body.IsStar)
                    {
                        str = ((body.Name + " " + body.StarName).Trim() + " (" + body.StarSubClass + body.StarLuminosity + ")").Trim();
                        if (body.PreviouslyDiscovered) brush = PreviouslyMappedBrush;
                    }
                    else
                    {
                        str = (body.Name + " - " + body.BodyType).Trim();
                        if (body.SelfMapped) 
                            brush = MappedBrush;
                        else if (body.PreviouslyMapped)
                            brush = PreviouslyMappedBrush;
                        else if (body.PreviouslyDiscovered)
                            brush = PreviouslyDiscoveredBrush;
                    }

                    g.DrawString(str, NotificationApp.EDFont, brush, IconSize, y + TopPadding);
                    size = g.MeasureString(str, NotificationApp.EDFont);

                    if (body.IsPlanet)
                    {
                        if (!string.IsNullOrEmpty(body.Features))
                        {
                            str = " [" + body.Features + "]";
                            g.DrawString(str, NotificationApp.EDFont, InterestBrush, IconSize + size.Width, y + TopPadding);
                        }
                    }

                    str = body.Distance.ToString("N0") + " Ls / " + body.Value.ToString("N0") + " cr";
                    size = g.MeasureString(str, NotificationApp.EDFont);
                    g.DrawString(str, NotificationApp.EDFont, brush, TextureSize.Width - size.Width, y + TopPadding);

                    y += IconSize;
                }

                var creditStart = (percentStart - IconSize) / 2;
                str = string.Format(" {0,11:N0} cr", totalCredits);
                size = g.MeasureString(str, NotificationApp.EDFont);
                g.DrawImage(Properties.Resources.Credit, creditStart, 3, IconSize, IconSize);
                g.DrawString(str, NotificationApp.EDFont, NotificationApp.DefaultBrush, creditStart + IconSize, TopPadding);

                g.DrawLine(TopLinePen, 0, 1, TextureSize.Width, 1);
                g.DrawLine(BottomLinePen, 0, TextureSize.Height - 2, TextureSize.Width, TextureSize.Height - 2);

                g.Flush();
            }
            PanelUpdated = 2;
        }
    }
}

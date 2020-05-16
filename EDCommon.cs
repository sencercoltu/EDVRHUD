using EDVRHUD.HUDs;
using LiteDB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace EDVRHUD
{
    [Flags]
    internal enum StatusFlags : long
    {
        Initial = -1,
        None = 0,
        Docked = 1 << 0, //(on a landing pad)
        Landed = 1 << 1, //(on planet surface)
        GearDown = 1 << 2, //Landing Gear Down
        ShieldsUp = 1 << 3, // Shields Up
        Supercruise = 1 << 4, // Supercruise
        FlightAssistOff = 1 << 5,
        HardpointsDeployed = 1 << 6,
        InWing = 1 << 7,
        LightsOn = 1 << 8,
        CargoScoopDeployed = 1 << 9,
        SilentRunning = 1 << 10,
        ScoopingFuel = 1 << 11,
        SrvHandbrake = 1 << 12,
        SrvTurret = 1 << 13,
        SrvUnderShip = 1 << 14,
        SrvDriveAssist = 1 << 15,
        FsdMassLocked = 1 << 16,
        FsdCharging = 1 << 17,
        FsdCooldown = 1 << 18,
        LowFuel = 1 << 19, // ( < 25% )
        OverHeating = 1 << 20, // ( > 100% )
        HasLatLong = 1 << 21,
        IsInDanger = 1 << 22,
        BeingInterdicted = 1 << 23,
        InMainShip = 1 << 24,
        InFighter = 1 << 25,
        InSRV = 1 << 26,
        HudAnalysisMode = 1 << 27,
        NightVision = 1 << 28,
        AltitudeFromAverageRadius = 1 << 29,
        FsdJump = 1 << 30,
        SrvHighBeam = 1 << 31
    }

    internal enum GUIFocus
    {
        Initial = -1,
        None = 0,
        InternalPanel = 1, //(right hand side)
        ExternalPanel = 2, //(left hand side)
        CommsPanel = 3, //(top)
        RolePanel = 4, // (bottom)
        StationServices = 5,
        GalaxyMap = 6,
        SystemMap = 7,
        Orrery = 8,
        FSS = 9,
        SAA = 10,
        Codex = 11
    }

    [Flags]
    internal enum MaterialType
    {
        None = 0,
        Antimony = 1 << 0, //s
        Arsenic = 1 << 1, //s
        Boron = 1 << 2, //a
        Cadmium = 1 << 3, //s
        Carbon = 1 << 4, //s
        Chromium = 1 << 5, //s
        Germanium = 1 << 6, //s
        Iron = 1 << 7, //s
        Lead = 1 << 8, //a
        Manganese = 1 << 9, //s
        Mercury = 1 << 10, //s
        Molybdenum = 1 << 11, //s
        Nickel = 1 << 12, //s
        Niobium = 1 << 13, //s
        Phosphorus = 1 << 14, //s
        Polonium = 1 << 15, //s
        Rhenium = 1 << 16, //a
        Ruthenium = 1 << 17, //s
        Selenium = 1 << 18, //s
        Sulphur = 1 << 19, //s
        Technetium = 1 << 20, //s
        Tellurium = 1 << 21, //s
        Tin = 1 << 22, //s
        Tungsten = 1 << 23, //s
        Vanadium = 1 << 24, //s 
        Yttrium = 1 << 25, //s
        Zinc = 1 << 26, //s
        Zirconium = 1 << 27, //s
        All = 0b1111111111111111111111111111
    }

    internal enum RarityType
    {
        VeryCommon,
        Common,
        Standard,
        Rare,
        VeryRare
    }

    internal enum StarType
    {
        Safe,
        Dangerous
    }


    internal class Material
    {
        public RarityType Rarity { get; private set; } = RarityType.VeryCommon;
        public MaterialType MaterialType { get; private set; } = MaterialType.None;

        public Material(RarityType rarity, MaterialType material)
        {
            MaterialType = material;
            Rarity = rarity;
        }

        public static readonly MaterialType BasicBoostMaterials =
            MaterialType.Carbon | MaterialType.Germanium | MaterialType.Vanadium;

        public static readonly MaterialType StandardBoostMaterials =
            MaterialType.Carbon | MaterialType.Germanium | MaterialType.Vanadium |
            MaterialType.Niobium | MaterialType.Cadmium;

        public static readonly MaterialType PremiumBoostMaterials =
            MaterialType.Carbon | MaterialType.Germanium | MaterialType.Arsenic |
            MaterialType.Niobium | MaterialType.Yttrium | MaterialType.Polonium;

        public static readonly MaterialType AllSurfaceMaterials =
            MaterialType.Antimony | MaterialType.Arsenic | MaterialType.Cadmium | MaterialType.Carbon |
            MaterialType.Chromium | MaterialType.Germanium | MaterialType.Iron | MaterialType.Manganese |
            MaterialType.Mercury | MaterialType.Molybdenum | MaterialType.Nickel | MaterialType.Niobium |
            MaterialType.Phosphorus | MaterialType.Polonium | MaterialType.Ruthenium | MaterialType.Selenium |
            MaterialType.Sulphur | MaterialType.Technetium | MaterialType.Tellurium | MaterialType.Tin |
            MaterialType.Tungsten | MaterialType.Vanadium | MaterialType.Yttrium | MaterialType.Zinc |
            MaterialType.Zirconium;

        public static readonly MaterialType AllMaterials = MaterialType.All;
    }

    internal class Star
    {
        public Star(string name, string variant, StarType starType, bool scoopable = true, double k = 1200)
        {
            Scoopable = scoopable;
            PlainName = name;
            Variant = variant;
            Type = starType;
            K = k;
        }
        public StarType Type { get; set; }
        public string TypeName { get { return (PlainName + " " + Variant).Trim(); } }

        public string PlainName { get; set; }
        public bool Scoopable { get; private set; }
        public string Variant { get; set; }
        public double K { get; set; }
    }

    internal class Planet
    {
        public string Name { get; set; }
        public double K { get; set; }
        public double TerraformableBonus { get; set; }
        public Planet(string name, double k = 300, double terraformableBonus = 93328)
        {
            Name = name;
            K = k;
            TerraformableBonus = terraformableBonus;
        }

    }


    internal static class EDCommon
    {
        public static LiteDatabase DB { get; set; } = null;
        public static ILiteCollection<Dictionary<string, object>> DBJournal { get; set; }
        public static ILiteCollection<Dictionary<string, object>> DBSettings { get; set; }
        //public static int DBSequence = DateTime.Now.Millisecond;
        public static DateTime DBEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static ObjectId DBIDForEntry(DateTime ts, string eventtype, Dictionary<string, object> entry)
        {
            int hashCode;
            switch (eventtype)
            {
                case "Scan":
                    hashCode = (eventtype + "#" + entry.GetProperty("BodyName", "NoName")).GetHashCode();
                    break;
                case "JetConeDamage":
                    hashCode = (eventtype + "#" + entry.GetProperty("Module", "")).GetHashCode();
                    break;
                case "ReceiveText":
                    hashCode = (eventtype + "#" + entry.GetProperty("Message", "")).GetHashCode();
                    break;
                case "ShipTargeted":
                    hashCode = (eventtype + "#" + entry.GetProperty("ScanStage", 0) + "#" + entry.GetProperty("Ship", "") + "#" + entry.GetProperty("PilotName", "")).GetHashCode();
                    break;
                case "MaterialCollected":
                case "FSDTarget":
                    hashCode = (eventtype + "#" + entry.GetProperty("Name", "")).GetHashCode();
                    break;
                case "UnderAttack":
                    hashCode = (eventtype + "#" + entry.GetProperty("Target", "")).GetHashCode();
                    break;
                case "Music":
                    hashCode = (eventtype + "#" + entry.GetProperty("MusicTrack", "")).GetHashCode();
                    break;
                case "FSSSignalDiscovered":
                    hashCode = (eventtype + "#" + entry.GetProperty("SignalName", "")).GetHashCode();
                    break;
                case "USSDrop":
                    hashCode = (eventtype + "#" + entry.GetProperty("USSType", "")).GetHashCode();
                    break;
                case "FuelScoop":
                    hashCode = (eventtype + "#" + entry.GetProperty("Total", 0.0)).GetHashCode();
                    break;
                case "HullDamage":
                    hashCode = (eventtype + "#" + entry.GetProperty("Health", 0.0)).GetHashCode();
                    break;
                case "CodexEntry":
                    hashCode = (eventtype + "#" + entry.GetProperty("EntryID", 0)).GetHashCode();
                    break;
                case "NpcCrewPaidWage":
                case "RedeemVoucher":
                    hashCode = (eventtype + "#" + entry.GetProperty("Amount", 0)).GetHashCode();
                    break;
                default:
                    hashCode = eventtype.GetHashCode();
                    break;
            }
            return new ObjectId((int)((ts - DBEpoch).TotalSeconds), hashCode, 0, 0);
        }

        //[DllImport("user32.dll", SetLastError = true)]
        //private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        //[DllImport("user32.dll")]
        //private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);


        //private static IntPtr _edWindowHandle = IntPtr.Zero;
        //public static IntPtr GetEDWindowHandle
        //{
        //    get
        //    {
        //        if (_edWindowHandle == IntPtr.Zero)
        //            _edWindowHandle = FindWindow("FrontierDevelopmentsAppWinClass", "Elite - Dangerous (CLIENT)");
        //        return _edWindowHandle;
        //    }
        //}


        private static bool GetPropertyInternal<T>(IDictionary<string, object> dict, string key, T defaultValue, out T result)
        {
            if (dict == null)
            {
                result = defaultValue;
                return false;
            }

            object obj = null;
            try
            {
                dict.TryGetValue(key, out obj);
                if (obj == null)
                {
                    result = defaultValue;
                    return false;
                }

                if (defaultValue is Enum)
                    result = (T)Enum.Parse(defaultValue.GetType(), obj.ToString(), true);
                else if (obj is T)
                    result = (T)obj;
                else if ((defaultValue is string) && (obj is string))
                    result = (T)obj;
                else if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var conv = TypeDescriptor.GetConverter(typeof(T));
                    result = (T)conv.ConvertFrom(obj);
                    //result = (T)obj;
                }
                else
                    result = (T)Convert.ChangeType(obj, typeof(T), CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                result = defaultValue;
                return false;
            }
        }

        public static bool GetProperty<T>(this IDictionary<string, object> dict, string key, T defaultValue, out T result)
        {
            return GetPropertyInternal(dict, key, defaultValue, out result);
        }

        public static T GetProperty<T>(this IDictionary<string, object> dict, string key, T defaultValue)
        {
            GetPropertyInternal(dict, key, defaultValue, out T result);
            return result;
        }

        public static void GetBodyValue(BodyInfo body)
        {
            double k = 300;
            double mass = body.Mass != 0 ? body.Mass : 1.0;


            if (body.IsStar)
            {
                if (StarLookup.TryGetValue(body.BodyType, out var s))
                    k = s.K;

                body.Value = k + (mass * k / 66.25);
            }
            else if (body.IsPlanet)
            {
                if (PlanetLookup.TryGetValue(body.BodyType, out var p))
                {
                    k = p.K;
                    if (body.Terraformable) k += p.TerraformableBonus;
                }

                double effmapped = 1.25;
                const double q = 0.56591828;
                double basevalue = Math.Max((k + (k * Math.Pow(mass, 0.2) * q)), 500);
                double firstdiscovery = 2.6;

                if (!body.PreviouslyDiscovered && !body.PreviouslyMapped && body.SelfMapped && body.MapEfficency)
                    body.Value = basevalue * firstdiscovery * 3.699622554 * effmapped; //FirstDiscovered FirstMapped Efficiently 
                else if (!body.PreviouslyDiscovered && !body.PreviouslyMapped && body.SelfMapped)
                    body.Value = basevalue * firstdiscovery * 3.699622554; //FirstDiscovered FirstMapped
                else if (body.PreviouslyDiscovered && !body.PreviouslyMapped && body.SelfMapped && body.MapEfficency)
                    body.Value = basevalue * 8.0956 * effmapped; //FirstMapped Efficiently
                else if (body.PreviouslyDiscovered && !body.PreviouslyMapped && body.SelfMapped)
                    body.Value = basevalue * 8.0956; //FirstMapped
                else if (!body.PreviouslyDiscovered)
                    body.Value = basevalue * firstdiscovery; //FirstDiscovered
                else
                    body.Value = basevalue; //Base

            }
        }

        private static Thread EDSMSystemRequestThread = null;
        private static Thread EDSMNearbySystemsThread = null;

        internal static void RequestEDSMNearbySystems(string systemName, Action<Dictionary<string, object>[]> newarbySystemsCallback)
        {
            if (EDSMNearbySystemsThread != null)
            {
                try
                {
                    EDSMNearbySystemsThread.Abort();
                }
                catch
                {

                }
                EDSMNearbySystemsThread = null;
            }

            EDSMNearbySystemsThread = new Thread(() =>
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.edsm.net/api-v1/sphere-systems?showPrimaryStar=1&radius=100&systemName=" + systemName);
                    request.Method = "GET";
                    request.Headers.Add("Accept-Encoding", "gzip,deflate");
                    request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    request.Timeout = 10000;
                    request.ContentType = "application/json; charset=utf-8";

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string data = "";
                        var dataStream = response.GetResponseStream();
                        var reader = new StreamReader(dataStream);
                        data = reader.ReadToEnd();
                        reader.Close();
                        dataStream.Close();

                        var serializer = new JavaScriptSerializer();
                        var dict = serializer.Deserialize<Dictionary<string, object>[]>(data);
                        newarbySystemsCallback?.Invoke(dict);
                    }
                }
                catch
                {

                }
                EDSMNearbySystemsThread = null;
            })
            { IsBackground = true };
            EDSMNearbySystemsThread.Start();
        }

        internal static void RequestEDSMSystemInfo(ulong systemAddress, Action<Dictionary<string, object>> systemInfoCallback)
        {
            if (EDSMSystemRequestThread != null)
            {
                try
                {
                    EDSMSystemRequestThread.Abort();
                }
                catch
                {

                }
                EDSMSystemRequestThread = null;
            }

            EDSMSystemRequestThread = new Thread(() =>
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.edsm.net/api-system-v1/bodies?systemId64=" + systemAddress);
                    request.Method = "GET";
                    request.Headers.Add("Accept-Encoding", "gzip,deflate");
                    request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    request.Timeout = 5000;
                    request.ContentType = "application/json; charset=utf-8";

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string data = "";
                        var dataStream = response.GetResponseStream();
                        var reader = new StreamReader(dataStream);
                        data = reader.ReadToEnd();
                        reader.Close();
                        dataStream.Close();

                        var serializer = new JavaScriptSerializer();
                        var dict = serializer.Deserialize<Dictionary<string, object>>(data);
                        systemInfoCallback?.Invoke(dict);
                    }
                }
                catch
                {

                }
                EDSMSystemRequestThread = null;
            })
            { IsBackground = true };
            EDSMSystemRequestThread.Start();
        }

        public static readonly Dictionary<string, Planet> PlanetLookup = new Dictionary<string, Planet>
        {
            ["Ammonia world"] = new Planet("Ammonia world", 96932),
            ["Earthlike body"] = new Planet("Earthlike body", 64831, 116295),
            ["Water world"] = new Planet("Water world", 64831, 116295),
            ["High metal content body"] = new Planet("High metal content body", 9654, 100677),
            ["Icy body"] = new Planet("Icy body"),
            ["Metal rich body"] = new Planet("Metal rich body", 21790, 65631),
            ["Rocky body"] = new Planet("Rocky body"),
            ["Rocky ice body"] = new Planet("Rocky ice body"),
            ["Sudarsky class I gas giant"] = new Planet("Sudarsky class I gas giant", 1656),
            ["Sudarsky class II gas giant"] = new Planet("Sudarsky class II gas giant", 9654, 100677),
            ["Sudarsky class III gas giant"] = new Planet("Sudarsky class III gas giant"),
            ["Sudarsky class IV gas giant"] = new Planet("Sudarsky class IV gas giant"),
            ["Sudarsky class V gas giant"] = new Planet("Sudarsky class V gas giant"),
            ["Gas giant with ammonia based life"] = new Planet("Gas giant with ammonia based life"),
            ["Gas giant with water based life"] = new Planet("Gas giant with water based life"),
            ["Helium rich gas giant"] = new Planet("Helium rich gas giant"),
            ["Helium gas giant"] = new Planet("Helium gas giant"),
            ["Water giant"] = new Planet("Water giant"),
            ["Water giant with life"] = new Planet("Water giant with life")
        };

        public static readonly Dictionary<string, Star> StarLookup = new Dictionary<string, Star>
        {
            ["O"] = new Star("Blue-White", "O", StarType.Safe),
            ["B"] = new Star("Blue-White", "B", StarType.Safe),
            ["B_BlueWhiteSuperGiant"] = new Star("Blue-White Super Giant", "B", StarType.Safe),
            ["A"] = new Star("Blue-White", "A", StarType.Safe),
            ["A_BlueWhiteSuperGiant"] = new Star("Blue-White Super Giant", "A", StarType.Safe),
            ["F"] = new Star("White", "F", StarType.Safe),
            ["F_WhiteSuperGiant"] = new Star("White Super Giant", "F", StarType.Safe),
            ["G"] = new Star("White-Yellow", "G", StarType.Safe),
            ["G_WhiteSuperGiant"] = new Star("White-Yellow Super Giant", "G", StarType.Safe),
            ["K"] = new Star("Yellow-Orange", "K", StarType.Safe),
            ["K_OrangeGiant"] = new Star("Yellow-Orange Giant", "K", StarType.Safe),
            ["M"] = new Star("Red Dwarf", "M", StarType.Safe),
            ["M_RedGiant"] = new Star("Red Giant", "M", StarType.Safe),
            ["M_RedSuperGiant"] = new Star("Red Super Giant", "M", StarType.Safe),
            ["L"] = new Star("Brown Dwarf", "L", StarType.Safe, false),
            ["T"] = new Star("Brown Dwarf", "T", StarType.Safe, false),
            ["Y"] = new Star("Brown Dwarf", "Y", StarType.Safe, false),
            ["TTS"] = new Star("T Tauri", "", StarType.Safe, false),
            ["AeBe"] = new Star("Herbig Ae/Be", "", StarType.Safe, false),
            ["W"] = new Star("Wolf-Rayet", "W", StarType.Safe, false),
            ["WN"] = new Star("Wolf-Rayet", "WN", StarType.Safe, false),
            ["WNC"] = new Star("Wolf-Rayet", "WNC", StarType.Safe, false),
            ["WC"] = new Star("Wolf-Rayet", "WC", StarType.Safe, false),
            ["WO"] = new Star("Wolf-Rayet", "WO", StarType.Safe, false),
            ["CS"] = new Star("Carbon Star", "WCS", StarType.Safe, false),
            ["C"] = new Star("Carbon Star", "C", StarType.Safe, false),
            ["CN"] = new Star("Carbon Star", "CN", StarType.Safe, false),
            ["CJ"] = new Star("Carbon Star", "CJ", StarType.Safe, false),
            ["CH"] = new Star("Carbon Star", "CH", StarType.Safe, false),
            ["CHd"] = new Star("Carbon Star", "CHd", StarType.Safe, false),
            ["MS"] = new Star("Carbon Star", "MS", StarType.Safe, false),
            ["S"] = new Star("Carbon Star", "S", StarType.Safe, false),
            ["D"] = new Star("White Dwarf", "D", StarType.Dangerous, false, 14057),
            ["DA"] = new Star("White Dwarf", "DA", StarType.Dangerous, false, 14057),
            ["DAB"] = new Star("White Dwarf", "DAB", StarType.Dangerous, false, 14057),
            ["DAO"] = new Star("White Dwarf", "DAO", StarType.Dangerous, false, 14057),
            ["DAZ"] = new Star("White Dwarf", "DAZ", StarType.Dangerous, false, 14057),
            ["DAV"] = new Star("White Dwarf", "DAV", StarType.Dangerous, false, 14057),
            ["DB"] = new Star("White Dwarf", "DB", StarType.Dangerous, false, 14057),
            ["DBZ"] = new Star("White Dwarf", "DBZ", StarType.Dangerous, false, 14057),
            ["DBV"] = new Star("White Dwarf", "DBV", StarType.Dangerous, false, 14057),
            ["DO"] = new Star("White Dwarf", "DO", StarType.Dangerous, false, 14057),
            ["DOV"] = new Star("White Dwarf", "DOV", StarType.Dangerous, false, 14057),
            ["DQ"] = new Star("White Dwarf", "DQ", StarType.Dangerous, false, 14057),
            ["DC"] = new Star("White Dwarf", "DC", StarType.Dangerous, false, 14057),
            ["DCV"] = new Star("White Dwarf", "DCV", StarType.Dangerous, false, 14057),
            ["DX"] = new Star("White Dwarf", "DX", StarType.Dangerous, false, 14057),
            ["N"] = new Star("Neutron Star", "", StarType.Dangerous, false, 22628),
            ["H"] = new Star("Black Hole", "", StarType.Dangerous, false, 22628),
            ["SupermassiveBlackHole"] = new Star("Supermassive Black Hole", "", StarType.Dangerous, false, 33.5678),
            ["Unknown"] = new Star("Unknown Star", "", StarType.Dangerous, false),
        };

        public static readonly Dictionary<string, Material> MaterialLookup = new Dictionary<string, Material>()
        {
            ["antimony"] = new Material(RarityType.Rare, MaterialType.Antimony),
            ["arsenic"] = new Material(RarityType.Common, MaterialType.Arsenic),
            ["boron"] = new Material(RarityType.VeryCommon, MaterialType.Boron),
            ["cadmium"] = new Material(RarityType.Standard, MaterialType.Cadmium),
            ["carbon"] = new Material(RarityType.VeryCommon, MaterialType.Carbon),
            ["chromium"] = new Material(RarityType.Common, MaterialType.Chromium),
            ["germanium"] = new Material(RarityType.Common, MaterialType.Germanium),
            ["iron"] = new Material(RarityType.VeryCommon, MaterialType.Iron),
            ["lead"] = new Material(RarityType.VeryCommon, MaterialType.Lead),
            ["manganese"] = new Material(RarityType.Common, MaterialType.Manganese),
            ["mercury"] = new Material(RarityType.Standard, MaterialType.Mercury),
            ["molybdenum"] = new Material(RarityType.Standard, MaterialType.Molybdenum),
            ["nickel"] = new Material(RarityType.VeryCommon, MaterialType.Nickel),
            ["niobium"] = new Material(RarityType.Standard, MaterialType.Niobium),
            ["phosphorus"] = new Material(RarityType.VeryCommon, MaterialType.Phosphorus),
            ["polonium"] = new Material(RarityType.Rare, MaterialType.Polonium),
            ["rhenium"] = new Material(RarityType.VeryCommon, MaterialType.Rhenium),
            ["ruthenium"] = new Material(RarityType.Rare, MaterialType.Ruthenium),
            ["selenium"] = new Material(RarityType.Rare, MaterialType.Selenium),
            ["sulphur"] = new Material(RarityType.VeryCommon, MaterialType.Sulphur),
            ["technetium"] = new Material(RarityType.Rare, MaterialType.Technetium),
            ["tellurium"] = new Material(RarityType.Rare, MaterialType.Tellurium),
            ["tin"] = new Material(RarityType.Standard, MaterialType.Tin),
            ["tungsten"] = new Material(RarityType.Standard, MaterialType.Tungsten),
            ["vanadium"] = new Material(RarityType.Common, MaterialType.Vanadium),
            ["yttrium"] = new Material(RarityType.Rare, MaterialType.Yttrium),
            ["zinc"] = new Material(RarityType.Common, MaterialType.Zinc),
            ["zirconium"] = new Material(RarityType.Common, MaterialType.Zirconium)
        };



        public static string FixBodyTypeSpelling(string name)
        {
            //gas giants
            return name
                .Replace(" I ", " 1 ")
                .Replace(" II ", " 2 ")
                .Replace(" III ", " 3 ")
                .Replace(" IV ", " 4 ")
                .Replace(" V ", " 5 ");
        }

        public static string FixBodyNameSpelling(string name)
        {
            //ABC 4
            //split letters only
            var fixedname = "";
            foreach (var letter in name)
            {
                fixedname += letter;
                if (char.IsLetter(letter))
                    fixedname += " ";
            }
            return fixedname
                .ToUpperInvariant()
                .Replace("A ", "eigh ");
        }
    }


}

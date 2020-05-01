using EDVRHUD.HUDs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDVRHUD
{
    internal static class EDCommon
    {
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

        [Flags]
        internal enum MaterialType
        {
            None = 0,
            Antimony = 0b0000000000000000000000000001, //s
            Arsenic = 0b0000000000000000000000000010, //s
            Boron = 0b0000000000000000000000000100, //a
            Cadmium = 0b0000000000000000000000001000, //s
            Carbon = 0b0000000000000000000000010000, //s
            Chromium = 0b0000000000000000000000100000, //s
            Germanium = 0b0000000000000000000001000000, //s
            Iron = 0b0000000000000000000010000000, //s
            Lead = 0b0000000000000000000100000000, //a
            Manganese = 0b0000000000000000001000000000, //s
            Mercury = 0b0000000000000000010000000000, //s
            Molybdenum = 0b0000000000000000100000000000, //s
            Nickel = 0b0000000000000001000000000000, //s
            Niobium = 0b0000000000000010000000000000, //s
            Phosphorus = 0b0000000000000100000000000000, //s
            Polonium = 0b0000000000001000000000000000, //s
            Rhenium = 0b0000000000010000000000000000, //a
            Ruthenium = 0b0000000000100000000000000000, //s
            Selenium = 0b0000000001000000000000000000, //s
            Sulphur = 0b0000000010000000000000000000, //s
            Technetium = 0b0000000100000000000000000000, //s
            Tellurium = 0b0000001000000000000000000000, //s
            Tin = 0b0000010000000000000000000000, //s
            Tungsten = 0b0000100000000000000000000000, //s
            Vanadium = 0b0001000000000000000000000000, //s 
            Yttrium = 0b0010000000000000000000000000, //s
            Zinc = 0b0100000000000000000000000000, //s
            Zirconium = 0b1000000000000000000000000000, //s
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
            public Star(string name, string variant, StarType starType, double k = 1200)
            {
                PlainName = name;
                Variant = variant;
                Type = starType;
                K = k;
            }
            public StarType Type { get; set; }
            public string TypeName { get { return (PlainName + " " + Variant).Trim(); } }

            public string PlainName { get; set; }

            public string Variant { get; set; }
            public double K { get; set; }
        }

        internal class Planet
        {
            public string Name { get; set; }
            public double K { get; set; }
            public double K_T { get; set; }

            public Planet(string name, double k = 300, double kt = 93328)
            {
                Name = name;
                K = k;
                K_T = kt;
            }

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
                    if (body.Terraformable) k += p.K_T;
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
            ["L"] = new Star("Brown Dwarf", "L", StarType.Safe),
            ["T"] = new Star("Brown Dwarf", "T", StarType.Safe),
            ["Y"] = new Star("Brown Dwarf", "Y", StarType.Safe),
            ["TTS"] = new Star("T Tauri", "", StarType.Safe),
            ["AeBe"] = new Star("Herbig Ae/Be", "", StarType.Safe),
            ["W"] = new Star("Wolf-Rayet", "W", StarType.Safe),
            ["WN"] = new Star("Wolf-Rayet", "WN", StarType.Safe),
            ["WNC"] = new Star("Wolf-Rayet", "WNC", StarType.Safe),
            ["WC"] = new Star("Wolf-Rayet", "WC", StarType.Safe),
            ["WO"] = new Star("Wolf-Rayet", "WO", StarType.Safe),
            ["CS"] = new Star("Carbon Star", "WCS", StarType.Safe),
            ["C"] = new Star("Carbon Star", "C", StarType.Safe),
            ["CN"] = new Star("Carbon Star", "CN", StarType.Safe),
            ["CJ"] = new Star("Carbon Star", "CJ", StarType.Safe),
            ["CH"] = new Star("Carbon Star", "CH", StarType.Safe),
            ["CHd"] = new Star("Carbon Star", "CHd", StarType.Safe),
            ["MS"] = new Star("Carbon Star", "MS", StarType.Safe),
            ["S"] = new Star("Carbon Star", "S", StarType.Safe),
            ["D"] = new Star("White Dwarf", "D", StarType.Dangerous, 14057),
            ["DA"] = new Star("White Dwarf", "DA", StarType.Dangerous, 14057),
            ["DAB"] = new Star("White Dwarf", "DAB", StarType.Dangerous, 14057),
            ["DAO"] = new Star("White Dwarf", "DAO", StarType.Dangerous, 14057),
            ["DAZ"] = new Star("White Dwarf", "DAZ", StarType.Dangerous, 14057),
            ["DAV"] = new Star("White Dwarf", "DAV", StarType.Dangerous, 14057),
            ["DB"] = new Star("White Dwarf", "DB", StarType.Dangerous, 14057),
            ["DBZ"] = new Star("White Dwarf", "DBZ", StarType.Dangerous, 14057),
            ["DBV"] = new Star("White Dwarf", "DBV", StarType.Dangerous, 14057),
            ["DO"] = new Star("White Dwarf", "DO", StarType.Dangerous, 14057),
            ["DOV"] = new Star("White Dwarf", "DOV", StarType.Dangerous, 14057),
            ["DQ"] = new Star("White Dwarf", "DQ", StarType.Dangerous, 14057),
            ["DC"] = new Star("White Dwarf", "DC", StarType.Dangerous, 14057),
            ["DCV"] = new Star("White Dwarf", "DCV", StarType.Dangerous, 14057),
            ["DX"] = new Star("White Dwarf", "DX", StarType.Dangerous, 14057),
            ["N"] = new Star("Neutron Star", "", StarType.Dangerous, 22628),
            ["H"] = new Star("Black Hole", "", StarType.Dangerous, 22628),
            ["SuperMassiveBlackHole"] = new Star("Supermassive Black Hole", "", StarType.Dangerous, 33.5678),
            ["Unknown"] = new Star("Unknown Star", "", StarType.Dangerous),
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
    }


}

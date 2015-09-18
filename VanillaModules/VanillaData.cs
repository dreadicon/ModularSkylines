using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSkylines.VanillaModules
{
    public static class VanillaData
    {
        public const int MaxSubServiceEnum = 17;
        public const byte OfficeMaxLevel = 3;
        public const byte GenericIndustryMaxLevel = 3;
        public const byte ResidentialMaxLevel = 5;
        public const byte CommercialMaxLevel = 3;
        public const string VanillaDensity1Ending = "Low";
        public const string VanillaDensity2Ending = "High";
        public const byte VanillaDensity1 = 1;
        public const byte VanillaDensity2 = 2;

        public static byte GetMaxLevel(ItemClass.Service service)
        {
            if((int)service <= ItemClass.PrivateServiceCount)
            {
                if (ItemClass.Service.Residential == service)
                {
                    return ResidentialMaxLevel - 1;
                }
                else if (ItemClass.Service.Commercial == service)
                {
                    return CommercialMaxLevel - 1;
                }
                else if (ItemClass.Service.Office == service)
                {
                    return OfficeMaxLevel - 1;
                }
            }
            return 0;
        }

        public static Dictionary<ItemClass.Service, byte> vanillaServiceMapping = new Dictionary<ItemClass.Service, byte>();
        public static Dictionary<ItemClass.SubService, byte> vanillaSubserviceMapping = new Dictionary<ItemClass.SubService, byte>();

        public static Dictionary<ItemClass.Service, string[]> VanillaServicesData = new Dictionary<ItemClass.Service, string[]>
        {
            {ItemClass.Service.Residential, new string[] {typeof(Growth).ToString()} },
            {ItemClass.Service.Commercial, new string[] { typeof(Growth).ToString() } },
            { ItemClass.Service.Industrial, new string[] { typeof(Growth).ToString() } },
            { ItemClass.Service.Office, new string[] { typeof(Growth).ToString() } },
            { ItemClass.Service.Electricity, new string[] {} },
            { ItemClass.Service.Education, new string[] {} },
            { ItemClass.Service.FireDepartment, new string[] {} },
            { ItemClass.Service.Garbage, new string[] {} },
            { ItemClass.Service.Government, new string[] {} },
            { ItemClass.Service.HealthCare, new string[] {} },
            { ItemClass.Service.Monument, new string[] {} },
            { ItemClass.Service.PoliceDepartment, new string[] {} },
            { ItemClass.Service.PublicTransport, new string[] { typeof(PublicBuilding).ToString() } },
            { ItemClass.Service.Water, new string[] {} },
            { ItemClass.Service.Beautification, new string[] {} },
        };

        //Subtypes only contain information if they have more than their main type
        public static Dictionary<ItemClass.SubService, string[]> VanillaSubservicesData = new Dictionary<ItemClass.SubService, string[]>
        {
            { ItemClass.SubService.ResidentialLow, new string[0] { } },
            { ItemClass.SubService.ResidentialHigh, new string[0] { } },
            { ItemClass.SubService.CommercialLow, new string[0] { } },
            { ItemClass.SubService.CommercialHigh, new string[0] { } },
            { ItemClass.SubService.IndustrialGeneric, new string[1] { typeof(PollutionData).ToString()}},
            { ItemClass.SubService.IndustrialForestry, new string[0] { }},
            { ItemClass.SubService.IndustrialFarming, new string[0] { }},
            { ItemClass.SubService.IndustrialOil, new string[1] { typeof(PollutionData).ToString() } },
            { ItemClass.SubService.IndustrialOre, new string[1] {typeof(PollutionData).ToString() }},
            { ItemClass.SubService.PublicTransportBus, new string[0] { }},
            { ItemClass.SubService.PublicTransportMetro, new string[0] { } },
            { ItemClass.SubService.PublicTransportPlane, new string[0] { }},
            { ItemClass.SubService.PublicTransportShip, new string[0] { }},
            { ItemClass.SubService.PublicTransportTaxi, new string[0] { }},
            { ItemClass.SubService.PublicTransportTrain, new string[0] { }},
        };

        public static Dictionary<ItemClass.SubService, ItemClass.Service> vanillaServiceCorralation = new Dictionary<ItemClass.SubService, ItemClass.Service>
        {
            { ItemClass.SubService.ResidentialLow, ItemClass.Service.Residential},
            { ItemClass.SubService.ResidentialHigh, ItemClass.Service.Residential},
            { ItemClass.SubService.CommercialLow, ItemClass.Service.Commercial},
            { ItemClass.SubService.CommercialHigh, ItemClass.Service.Commercial},
            { ItemClass.SubService.IndustrialGeneric, ItemClass.Service.Industrial},
            { ItemClass.SubService.IndustrialForestry, ItemClass.Service.Industrial},
            { ItemClass.SubService.IndustrialFarming, ItemClass.Service.Industrial},
            { ItemClass.SubService.IndustrialOil, ItemClass.Service.Industrial},
            { ItemClass.SubService.IndustrialOre, ItemClass.Service.Industrial},
            { ItemClass.SubService.PublicTransportBus, ItemClass.Service.PublicTransport},
            { ItemClass.SubService.PublicTransportMetro, ItemClass.Service.PublicTransport},
            { ItemClass.SubService.PublicTransportPlane, ItemClass.Service.PublicTransport},
            { ItemClass.SubService.PublicTransportShip, ItemClass.Service.PublicTransport},
            { ItemClass.SubService.PublicTransportTaxi, ItemClass.Service.PublicTransport},
            { ItemClass.SubService.PublicTransportTrain, ItemClass.Service.PublicTransport},
        };

        // Sets the number of workers (first value) and the distribution of education levels 0-3 (remaining four values)
        public static Dictionary<int, short[][]> LevelAutogenWorkers = new Dictionary<int, short[][]> {
            {(int)ItemClass.SubService.CommercialLow, new short[5][] {
                    new short[5] {50, 100, 0, 0, 0},
                    new short[5] { 75, 20, 60, 20, 0 },
                    new short[5] { 100, 5, 15, 30, 50 },
                    new short[5] { 100, 5, 15, 30, 50 },
                    new short[5] { 100, 5, 15, 30, 50 }
            }},
            {(int)ItemClass.SubService.CommercialHigh, new short[5][] {
                    new short[5] {75, 0, 40, 50, 10},
                    new short[5] { 100, 0, 20, 50, 30 },
                    new short[5] { 125, 0, 0, 40, 60 },
                    new short[5] { 125, 0, 0, 40, 60 },
                    new short[5] { 125, 0, 0, 40, 60 }
            }},
            {(int)ItemClass.SubService.None, new short[5][] {
                    new short[5] {50, 0, 40, 50, 10},
                    new short[5] { 110, 0, 20, 50, 30 },
                    new short[5] { 170, 0, 0, 40, 60 },
                    new short[5] { 170, 0, 0, 40, 60 },
                    new short[5] { 170, 0, 0, 40, 60 }
            }},
            {(int)ItemClass.SubService.IndustrialGeneric, new short[5][] {
                    new short[5] {100, 100, 0, 0, 0},
                    new short[5] { 150, 20, 60, 20, 0 },
                    new short[5] { 200, 5, 15, 30, 50 },
                    new short[5] { 200, 5, 15, 30, 50 },
                    new short[5] { 200, 5, 15, 30, 50 }
            }},
            {(int)ItemClass.SubService.IndustrialFarming, new short[1][] {new short[5] {100, 100, 0, 0, 0} } },
            {(int)ItemClass.SubService.IndustrialForestry, new short[1][] {new short[5] {100, 100, 0, 0, 0} } },
            {(int)ItemClass.SubService.IndustrialOre, new short[1][] {new short[5] {150, 20, 60, 20, 0} } },
            {(int)ItemClass.SubService.IndustrialOil, new short[1][] {new short[5] {150, 20, 60, 20, 0} } },
            };

        public static Dictionary<int, short[]> LevelAutogenResidents = new Dictionary<int, short[]>
        {
            {(int)ItemClass.SubService.ResidentialLow, new short[5] {20, 25, 30, 35, 40} },
            {(int)ItemClass.SubService.ResidentialHigh, new short[5] {60, 100, 130, 150, 160} }
        };

        public static Dictionary<int, short[]> LevelAutogenVisitplace = new Dictionary<int, short[]>
        {
            {(int)ItemClass.SubService.CommercialLow, new short[5] {90, 100, 110, 110, 110} },
            {(int)ItemClass.SubService.CommercialHigh, new short[5] {200, 300, 400, 400, 400}}
        };

        public static void InitializeVanillaBehaviors()
        {
            byte behaviorTypeCount = 0; //Note this is 0-indexed

            //Handle vanilla types, creating appropriate behavior for each.
            Type vanillaServiceType = typeof(ItemClass.Service);
            Type vanillaSubserviceType = typeof(ItemClass.SubService);

            SortedDictionary<int, string> serviceDict = new SortedDictionary<int, string>(Tools.EnumToDictionary<ItemClass.Service>());
            var subServiceDict = Tools.EnumToDictionary<ItemClass.SubService>();


            foreach (var entry in serviceDict)
            {
                List<BuildingBehaviorManager.Subtype> subtypes = new List<BuildingBehaviorManager.Subtype>();
                var behavior = new BuildingBehaviorManager.BehaviorType();
                behavior.defaultMaxDensity = 0;
                behavior.defaultMaxLevel = GetMaxLevel((ItemClass.Service)entry.Key);
                behavior.name = entry.Value;
                behavior.type = behaviorTypeCount;
                behavior.defaultDataModules = VanillaServicesData[(ItemClass.Service)entry.Key];
                foreach (var pair in subServiceDict)
                {
                    if (pair.Value.StartsWith(entry.Value))
                    {
                        // Handle vanilla density levels, which are currently exclusive of subservice types, so they go under default for services
                        if (pair.Value.EndsWith(VanillaDensity1Ending)) behavior.defaultMaxDensity = VanillaDensity1;
                        else if (pair.Value.EndsWith(VanillaDensity2Ending)) behavior.defaultMaxDensity = VanillaDensity2;
                        else
                        {
                            var subtype = new BuildingBehaviorManager.Subtype();

                            if (pair.Key == (int)ItemClass.SubService.IndustrialGeneric)
                                subtype.maxLevel = GenericIndustryMaxLevel - 1;
                            else
                                subtype.maxLevel = behavior.defaultMaxLevel;

                            subtype.maxDensity = behavior.defaultMaxDensity;
                            subtype.name = pair.Value;
                            subtype.defaultDataModules = behavior.defaultDataModules.Concat(VanillaSubservicesData[(ItemClass.SubService)pair.Key]).ToArray();
                            subtype.type = (byte)subtypes.Count;
                            subtypes.Add(subtype);
                            vanillaSubserviceMapping[(ItemClass.SubService)entry.Key] = (byte)subtypes.Count;
                        }
                    }
                }
                behavior.subtypes = subtypes.ToArray();
                BuildingBehaviorManager.behaviors.Insert(behaviorTypeCount, behavior);
                BuildingBehaviorManager.getBehaviorIDByName[behavior.name] = behaviorTypeCount;
                vanillaServiceMapping[(ItemClass.Service)entry.Key] = behaviorTypeCount;
                behaviorTypeCount++;
            }
        }
    }
}

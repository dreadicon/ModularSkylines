using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModularSkylines
{
    [BuildingModule]
    public static class VanillaPrivateBuildings
    {
        public delegate void GetWorkersDelegate(ushort buildingID, ref Building data, CoreAI core, out int level0, out int level1, out int level2, out int level3);

        public const int MaxSubServiceEnum = 17;
        public const int OfficeSubservice = 32;

        public enum Density
        {
            none = 0,
            light = 4,
            medium = 8,
            heavy = 12
        }

        public static Dictionary<ItemClass.SubService, int> VanillaSubserviceMapping = new Dictionary<ItemClass.SubService, int>
        {
            { ItemClass.SubService.CommercialLow, (int)Density.light},
            { ItemClass.SubService.CommercialHigh, (int)Density.heavy },
            { ItemClass.SubService.ResidentialLow, (int)Density.light },
            { ItemClass.SubService.ResidentialHigh, (int)Density.heavy },
            { ItemClass.SubService.None, (int)Density.none },
            { ItemClass.SubService.IndustrialGeneric, (int)Density.none },
            { ItemClass.SubService.IndustrialForestry, (int)Density.none },
            { ItemClass.SubService.IndustrialFarming, (int)Density.none },
            { ItemClass.SubService.IndustrialOil, (int)Density.none },
            { ItemClass.SubService.IndustrialOre, (int)Density.none }
        };

        // Sets the number of workers (first value) and the distribution of education levels 0-3 (remaining four values)
        public static Dictionary<int, short[][]> LevelAutogenWorkers = new Dictionary<int, short[][]> {
            {(int)ItemClass.SubService.CommercialLow, new short[3][] {
                    new short[5] {50, 100, 0, 0, 0},
                    new short[5] { 75, 20, 60, 20, 0 },
                    new short[5] { 100, 5, 15, 30, 50 }
            }},
            {(int)ItemClass.SubService.CommercialLow, new short[3][] {
                    new short[5] {75, 0, 40, 50, 10},
                    new short[5] { 100, 0, 20, 50, 30 },
                    new short[5] { 125, 0, 0, 40, 60 }
            }},
            {OfficeSubservice, new short[3][] {
                    new short[5] {50, 0, 40, 50, 10},
                    new short[5] { 110, 0, 20, 50, 30 },
                    new short[5] { 170, 0, 0, 40, 60 }
            }},
            {(int)ItemClass.SubService.IndustrialGeneric, new short[3][] {
                    new short[5] {100, 100, 0, 0, 0},
                    new short[5] { 150, 20, 60, 20, 0 },
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
            {(int)ItemClass.SubService.CommercialLow, new short[3] {90, 100, 110} },
            {(int)ItemClass.SubService.CommercialHigh, new short[3] {200, 300, 400}}
        };

        [BuildingModule]
        public static class VanillaBuildingInitialize
        {
            //Used to initialize a building's worker count. Vanilla function converted to be universal and compact.
            public static void SetWorkers(ushort buildingID, ref Building data, CoreAI core)
            {
                short[][] config;
                
                ItemClass @class = core.m_info.m_class;
                int subservice;
                if (@class.m_service == ItemClass.Service.Office) subservice = OfficeSubservice;
                else subservice = (int)@class.m_subService;
                
                if (LevelAutogenWorkers.TryGetValue(subservice, out config))
                {
                    int level = (int)@class.m_level;
                    if (config.GetLength(0) >= level)
                    {
                        int num = config[level][0];
                        int level0 = config[level][1];
                        int level1 = config[level][2];
                        int level2 = config[level][3];
                        int level3 = config[level][4];
                        if (num != 0)
                        {
                            var r = new Randomizer((int)buildingID);
                            num = Mathf.Max(200, data.Width * data.Length * num + r.Int32(100u)) / 100;
                            int num2 = level0 + level1 + level2 + level3;
                            if (num2 != 0)
                            {
                                level0 = (num * level0 + r.Int32((uint)num2)) / num2;
                                num -= level0;
                            }
                            num2 = level1 + level2 + level3;
                            if (num2 != 0)
                            {
                                level1 = (num * level1 + r.Int32((uint)num2)) / num2;
                                num -= level1;
                            }
                            num2 = level2 + level3;
                            if (num2 != 0)
                            {
                                level2 = (num * level2 + r.Int32((uint)num2)) / num2;
                                num -= level2;
                            }
                            level3 = num;
                            var workers = core.GetData<Workers>();
                            workers.ModifyWorkers((short)level0, (short)level1, (short)level2, (short)level3);
                            core.SetData<Workers>(workers);

                        }
                    }
                }
            }

            public static void SetHomes(ushort buildingID, ref Building data, CoreAI core)
            {
                short[] config;

                ItemClass @class = core.m_info.m_class;
                int subservice = (int)@class.m_subService;
                if (LevelAutogenResidents.TryGetValue(subservice, out config))
                {
                    int level = (int)@class.m_level;
                    if (config.GetLength(0) >= level)
                    {
                        var homes = core.GetData<Residents>();
                        Randomizer r = new Randomizer((int)buildingID);
                        homes.homeCount = (short)(Mathf.Max(100, data.Width * data.Length * config[level] + r.Int32(100u)) / 100);
                        core.SetData<Residents>(homes);
                    }
                }
            }

            public static void SetVisitors(ushort buildingID, ref Building data, CoreAI core)
            {
                short[] config;
                ItemClass @class = core.m_info.m_class;
                int subservice = (int)@class.m_subService;
                if (LevelAutogenVisitplace.TryGetValue(subservice, out config))
                {
                    int level = (int)@class.m_level;
                    if (config.GetLength(0) >= level)
                    {
                        var visitplaces = 
                    }
                }
        }

        [BuildingModule]
        public static class VanillaIndustrial
        {
        }
    }


    class VanillaCommercialBuildingInitialize : BuildingModule
    {

    }

    class VanillaResidentialBuildingInitialize : BuildingModule
    {
        private void initializeBuilding(ushort buildingID, ref Building data, CoreAI core)
        {
            int num;
            int num2;
            int num3;
            int num4;
            core.CalculateWorkplaceCount(new Randomizer((int)buildingID), data.Width, data.Length, out num, out num2, out num3, out num4);
            int workCount = num + num2 + num3 + num4;
            int homeCount = core.CalculateHomeCount(new Randomizer((int)buildingID), data.Width, data.Length);
            int visitCount = this.CalculateVisitplaceCount(new Randomizer((int)buildingID), data.Width, data.Length);
        }
    }

    class VanillaIndustrialBuildingInitialize : BuildingModule
    {
        private void initializeBuilding(ushort buildingID, ref Building data, CoreAI core)
        {
            int num;
            int num2;
            int num3;
            int num4;
            core.CalculateWorkplaceCount(new Randomizer((int)buildingID), data.Width, data.Length, out num, out num2, out num3, out num4);
            int workCount = num + num2 + num3 + num4;
            int homeCount = core.CalculateHomeCount(new Randomizer((int)buildingID), data.Width, data.Length);
            int visitCount = this.CalculateVisitplaceCount(new Randomizer((int)buildingID), data.Width, data.Length);
        }
    }
}

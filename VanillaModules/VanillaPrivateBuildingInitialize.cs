using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModularSkylines.VanillaModules
{
    [BuildingModule]
    public static class VanillaPrivateBuildings
    {
        public delegate void GetWorkersDelegate(ushort buildingID, ref Building data, CoreAI core, out int level0, out int level1, out int level2, out int level3);

        [BuildingModule]
        public static class VanillaBuildingInitialize
        {
            //Used to initialize a building's worker count. Vanilla function converted to be universal and compact.
            public static void SetWorkers(ushort buildingID, ref Building data, CoreAI core)
            {
                short[][] config;

                ItemClass @class = core.m_info.m_class;
                int subservice;
                if (@class.m_service == ItemClass.Service.Office) subservice = VanillaData.OfficeSubservice;
                else subservice = (int)@class.m_subService;

                if (VanillaData.LevelAutogenWorkers.TryGetValue(subservice, out config))
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
                            core.occupants.ModifyWorkers((short)level0, (short)level1, (short)level2, (short)level3);
                            core.occupantsUpdated = true;
                        }
                    }
                }
            }

            public static void SetHomes(ushort buildingID, ref Building data, CoreAI core)
            {
                short[] config;

                ItemClass @class = core.m_info.m_class;
                int subservice = (int)@class.m_subService;
                if (VanillaData.LevelAutogenResidents.TryGetValue(subservice, out config))
                {
                    int level = (int)@class.m_level;
                    if (config.GetLength(0) >= level)
                    {
                        Randomizer r = new Randomizer((int)buildingID);
                        core.occupants.maxHomeCount = (short)(Mathf.Max(100, data.Width * data.Length * config[level] + r.Int32(100u)) / 100);
                        core.occupantsUpdated = true;
                    }
                }
            }

            public static void SetVisitors(ushort buildingID, ref Building data, CoreAI core)
            {
                short[] config;
                ItemClass @class = core.m_info.m_class;
                int subservice = (int)@class.m_subService;
                if (VanillaData.LevelAutogenVisitplace.TryGetValue(subservice, out config))
                {
                    int level = (int)@class.m_level;
                    if (config.GetLength(0) >= level)
                    {
                        var visitplaces = config[level];
                        if (visitplaces > 0)
                        {
                            core.occupants.maxVisitors = config[level];
                            core.occupantsUpdated = true;
                        }
                    }
                }
            }
        }
    }
}

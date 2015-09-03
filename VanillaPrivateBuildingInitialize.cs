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
    public static class VanillaCommercial
    {
        public static void SetCommercialWorkerCount(ushort buildingID, ref Building data, Citizens citizens)
        {
            var r = new Randomizer((int)buildingID);
            
            int level0, level1, level2, level3;
            ItemClass @class = core.m_info.m_class;
            int num;
            if (@class.m_subService == ItemClass.SubService.CommercialLow)
            {
                if (@class.m_level == ItemClass.Level.Level1)
                {
                    num = 50;
                    level0 = 100;
                    level1 = 0;
                    level2 = 0;
                    level3 = 0;
                }
                else if (@class.m_level == ItemClass.Level.Level2)
                {
                    num = 75;
                    level0 = 20;
                    level1 = 60;
                    level2 = 20;
                    level3 = 0;
                }
                else
                {
                    num = 100;
                    level0 = 5;
                    level1 = 15;
                    level2 = 30;
                    level3 = 50;
                }
            }
            else if (@class.m_level == ItemClass.Level.Level1)
            {
                num = 75;
                level0 = 0;
                level1 = 40;
                level2 = 50;
                level3 = 10;
            }
            else if (@class.m_level == ItemClass.Level.Level2)
            {
                num = 100;
                level0 = 0;
                level1 = 20;
                level2 = 50;
                level3 = 30;
            }
            else
            {
                num = 125;
                level0 = 0;
                level1 = 0;
                level2 = 40;
                level3 = 60;
            }
            if (num != 0)
            {
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
            }
            
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

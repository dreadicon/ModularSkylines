using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColossalFramework;

namespace ModularSkylines
{
    class ManualWorkerCountModule : BuildingModule
    {
        [CustomizableProperty("Uneducated Workers", "Workers", 0)]
        public int m_workPlaceCount0 = 4;

        [CustomizableProperty("Educated Workers", "Workers", 1)]
        public int m_workPlaceCount1 = 16;

        [CustomizableProperty("Well Educated Workers", "Workers", 2)]
        public int m_workPlaceCount2 = 16;

        [CustomizableProperty("Highly Educated Workers", "Workers", 3)]
        public int m_workPlaceCount3 = 4;

        public override void OnCreateBuilding(ushort buildingID, ref Building data, CoreAI core)
        {
            int workCount = this.m_workPlaceCount0 + this.m_workPlaceCount1 + this.m_workPlaceCount2 + this.m_workPlaceCount3;
            Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, 0, workCount, 0, 0, 0);
        }

        public override void OnBuildingLoaded(ushort buildingID, ref Building data, uint version, CoreAI core)
        {
                int workCount = this.m_workPlaceCount0 + this.m_workPlaceCount1 + this.m_workPlaceCount2 +
                                this.m_workPlaceCount3;
        }
    }
}

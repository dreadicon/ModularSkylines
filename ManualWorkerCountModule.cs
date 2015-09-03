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
        public int m_workPlaceCount0 = 4;
        public int m_workPlaceCount1 = 16;
        public int m_workPlaceCount2 = 16;
        public int m_workPlaceCount3 = 4;

        public override void OnCreateBuilding(ushort buildingID, ref Building data, CoreAI core)
        {
            int workCount = this.m_workPlaceCount0 + this.m_workPlaceCount1 + this.m_workPlaceCount2 + this.m_workPlaceCount3;
            
        }

        public override void OnBuildingLoaded(ushort buildingID, ref Building data, uint version, CoreAI core)
        {
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSkylines
{
    
    public class Citizens
    {
        private static readonly Tourists DefaulTourists = new Tourists();
        private static readonly Workers DefaultWorkers = new Workers();

        public short homeCount = 0;

        public Workers GetWorkers()
        {
            if(workers == null) return DefaultWorkers;
            return workers;
        }

        public void AddWorkers()
        {
            if(this.workers == null) this.workers = new Workers();
        }

        public Tourists GetTourists()
        {
            if(tourists == null) return DefaulTourists;
            return tourists;
        }

        public void AddTourists()
        {
            if (this.tourists == null) this.tourists = new Tourists();
        }

        public Workers workers;
        public Tourists tourists;
    }
    
    public class Workers
    {
        public int totalWorkers => m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
        public short m_workPlaceCount0 = 0;
        public short m_workPlaceCount1 = 0;
        public short m_workPlaceCount2 = 0;
        public short m_workPlaceCount3 = 0;
    }

    public class Tourists
    {
        public int totalVisitors => m_visitPlaceCount0 + m_visitPlaceCount1 + m_visitPlaceCount2;
        public short m_attractivenessAccumulation = 0;
        public short m_visitPlaceCount0 = 0;
        public short m_visitPlaceCount1 = 0;
        public short m_visitPlaceCount2 = 0;
    }

    public class Education
    {
        public byte educationTier = 0;
        public short m_studentCount = 0;
        public float m_educationRadius = 0;
        public short m_educationAccumulation = 0;
    }

    public class Medical
    {
        public int m_healthCareAccumulation = 0;
        public float m_healthCareRadius = 0;
        public int m_ambulanceCount = 0;
        public int m_patientCapacity = 0;
        public int m_curingRate = 0;
    }

    public class WaterFacility
    {
        public int m_waterIntake = 0;
        public float m_maxWaterDistance = 0;
        public bool m_useGroundWater = false;
        public bool useGroundPollution = false;
        public int m_sewageOutlet = 0;
        public int m_outletPollution = 0;
    }

    public class Entertainment
    {
        public short m_entertainmentAccumulation = 0;
        public float m_entertainmentRadius = 0;
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSkylines
{
    public abstract class DataModule<T>
    {
        private static int HashCode = 0;
        private static bool initialized = false;

        public static int GetHandleHash
        {
            get
            {
                if(!initialized && HashCode == 0)
                {
                    HashCode = typeof(T).TypeHandle.GetHashCode();
                    initialized = true;
                }
                return HashCode;

            }
        }

        public override int GetHashCode()
        {
            return GetHandleHash;
        }
    }

    public struct CommonConsumption
    {
        public CommonConsumption(int electric = 0, int water = 0, int sewage = 0, int garbage = 0)
        {
            electricityConsumption = electric;
            waterConsumption = water;
            sewageAccumulation = sewage;
            garbageAccumulation = garbage;
        }
        public int electricityConsumption;
        public int waterConsumption;
        public int sewageAccumulation;
        public int garbageAccumulation;
    }

    public class Residents : DataModule<Residents>
    {
        public short homeCount = 0;
    }
    
    public class Workers : DataModule<Workers>
    {
        public void ModifyWorkers(short l0, short l1, short l2, short l3)
        {
            m_workPlaceCount0 += l0;
            m_workPlaceCount1 += l1;
            m_workPlaceCount2 += l2;
            m_workPlaceCount3 += l3;
        }
        public int totalWorkers => m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
        public short m_workPlaceCount0 = 0;
        public short m_workPlaceCount1 = 0;
        public short m_workPlaceCount2 = 0;
        public short m_workPlaceCount3 = 0;
    }

    public class Tourists : DataModule<Tourists>
    {
        public int totalVisitors => m_visitPlaceCount0 + m_visitPlaceCount1 + m_visitPlaceCount2;
        public short m_attractivenessAccumulation = 0;
        public short m_visitPlaceCount0 = 0;
        public short m_visitPlaceCount1 = 0;
        public short m_visitPlaceCount2 = 0;
    }

    public class Education : DataModule<Education>
    {
        public byte educationTier = 0;
        public short m_studentCount = 0;
        public float m_educationRadius = 0;
        public short m_educationAccumulation = 0;
    }

    public class Medical : DataModule<Medical>
    {
        public int m_healthCareAccumulation = 0;
        public float m_healthCareRadius = 0;
        public int m_ambulanceCount = 0;
        public int m_patientCapacity = 0;
        public int m_curingRate = 0;
    }

    public class WaterFacility : DataModule<WaterFacility>
    {
        public int m_waterIntake = 0;
        public float m_maxWaterDistance = 0;
        public bool m_useGroundWater = false;
        public bool useGroundPollution = false;
        public int m_sewageOutlet = 0;
        public int m_outletPollution = 0;
    }

    public class Entertainment : DataModule<Entertainment>
    {
        public short m_entertainmentAccumulation = 0;
        public float m_entertainmentRadius = 0;
    }

}

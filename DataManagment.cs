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
        public CommonConsumption(int electric = 0, int water = 0, int sewage = 0, int garbage = 0, int cost = 0, int income = 0)
        {
            electricityConsumption = electric;
            waterConsumption = water;
            sewageAccumulation = sewage;
            garbageAccumulation = garbage;
            publicCost = cost;
            publicIncome = income;
        }
        public int netIncome => publicIncome - publicCost;
        public int electricityConsumption;
        public int waterConsumption;
        public int sewageAccumulation;
        public int garbageAccumulation;
        public int publicIncome;
        public int publicCost;
    }

    //Being universal, I might make this a struct...
    public class Occupants : DataModule<Occupants>
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
        //public short homeCount = 0;
        public short maxHomeCount = 0;
        public short maxVisitors = 0;
    }


}

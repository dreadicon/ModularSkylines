using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSkylines
{

    public abstract class DataModule
    {
        public bool DataAltered = false;
        public abstract void GetDataEvent(CoreAI core);
        public T Initialize<T>(string module, CoreAI core, DataModuleEvent<T> mEvent = null, bool usePresetEvent = true, bool usePresetData = true) where T : DataModule<T>, new()
        {
            DataModule<T>.Preset preset;
            if (DataModule<T>.DataPreset.TryGetValue(module, out preset))
            {
                T newInstance = (usePresetData) ? (T)preset.Data.MemberwiseClone() : new T();
                if(usePresetEvent) preset.OnInitialize(core, newInstance);
                if (mEvent != null) mEvent(core, newInstance);
                return newInstance;
            }
            return new T();
        }
        //TODO: add proper load functionality to this, so it loads the module rather than re-initializing it.
        public T Load<T>(string module, CoreAI core, DataModuleEvent<T> mEvent = null, bool usePresetEvent = true, bool usePresetData = true) where T : DataModule<T>, new()
        {
            DataModule<T>.Preset preset;
            if (DataModule<T>.DataPreset.TryGetValue(module, out preset))
            {
                T newInstance = (usePresetData) ? (T)preset.Data.MemberwiseClone() : new T();
                if (usePresetEvent) preset.OnLoad(core, newInstance);
                if (mEvent != null) mEvent(core, newInstance);
                return newInstance;
            }
            return new T();
        }
    }

    public abstract class DataModule<T> : DataModule where T : DataModule<T>
    {
        private static int HashCode = 0;
        private static bool initialized = false;

        public struct Preset
        {
            public string name;
            public T Data;
            public DataModuleEvent<T> OnInitialize;
            public DataModuleEvent<T> OnLoad;
        }

        public static Dictionary<string, Preset> DataPreset;

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

        public DataModuleEvent<T> GetDataDelegate;

        public override void GetDataEvent(CoreAI core)
        {
            if(DataAltered)
                GetDataDelegate(core, (T)this);
        }
    }

    //This will differentiate modules with data that must be saved to disk, as most don't actually need saved.
    public abstract class PersistentDataModule : DataModule<PersistentDataModule>
    {

    }

    public struct CommonConsumption
    {
        public CommonConsumption(short electric = 0, short water = 0, short sewage = 0, short garbage = 0, int cost = 0, int income = 0)
        {
            electricityConsumption = electric;
            waterConsumption = water;
            sewageAccumulation = sewage;
            garbageAccumulation = garbage;
            publicCost = cost;
            publicIncome = income;
        }
        public int netIncome => publicIncome - publicCost;
        public short electricityConsumption;
        public short waterConsumption;
        public short sewageAccumulation;
        public short garbageAccumulation;
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

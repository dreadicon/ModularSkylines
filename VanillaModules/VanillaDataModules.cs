using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSkylines.VanillaModules
{
    public class PublicBuilding : DataModule<PublicBuilding>
    {
        // TODO: add safe getter.
        public ManualMilestone m_createPassMilestone;
        public MessageInfo m_onCompleteMessage;
        //Note: not sure what this does...
        [BitMask]
        public EventManager.EventType m_supportEvents;
    }

    public class Growth : DataModule<Growth>
    {
        public short m_maxLevel = -1;
        public int m_constructionTime = 0;
    }

    public class Noise : DataModule<Noise>
    {
        public float m_noiseRadius = 0;
        public short m_noiseAccumulation = 0;
    }

    //TODO: rework this to be supplemented by a dictionary, or split into multiple classes.
    public class ResourceData : DataModule<ResourceData>
    {
        public short[] capacity;
        public byte[] resources;
        public byte[] available;
        public short[] rate;
        public bool[] depletesPermanent;
        public bool[] depletesTemporary;
    }

    public class ElectricityData : DataModule<ElectricityData>
    {
        public short electricityRate;
        public short electricityCapacity;
        public float electricityToResourceRatio;
    }

    public class PollutionData : DataModule<PollutionData>
    {
        public short pollutionRate;
        public float pollutionRadius;
    }

    public class Tourists : DataModule<Tourists>
    {
        public int maxTourists => m_visitPlaceCount0 + m_visitPlaceCount1 + m_visitPlaceCount2;
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

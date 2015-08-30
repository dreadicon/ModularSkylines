using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ColossalFramework;

namespace ModularSkylines
{
    /// <summary>
    /// Unified Color behavior class for modular buildings. Unlike aspects such as power output or housing, only one color method 
    /// should be assigned per building. Therefore all individualized coloring methods inherit from this class, and the CoreAI
    /// stores a reference to a single ColorAI class from which it retrieves the appropriate GetColor method.
    /// 
    /// To use: inherit from this class and override GetColor. Public/Player BuildingGetColor generic methods provided within this class. 
    /// Your child ColorAI class will then be added to the list of choices in the asset editor automatically using the name string in this class.
    /// </summary>
    public class ColorAI
    {
        //reference to the core AI object, which inherits from CommonBuildingAI, and manages all other modules and effects.
        private CoreAI coreAI;

        public const string NAME = "Generic Coloring";

        //TODO: add logic for compiling a list of all child classes of this class to a list in Asset editor for choosing.

        public Color GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode)
        {
            return coreAI.BaseColor(buildingID, ref data, infoMode);
        }

        //GetColor method from PrivateBuildingAI
        public Color PrivateBuildingGetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode)
        {
            switch (infoMode)
            {
                case InfoManager.InfoMode.NoisePollution:
                    {
                        int num;
                        int num2;
                        coreAI.GetPollutionRates((!coreAI.GetShowConsumption(buildingID, ref data)) ? 0 : 40, out num, out num2);
                        if (num2 != 0)
                        {
                            return CommonBuildingAI.GetNoisePollutionColor((float)num2 * 0.25f);
                        }
                        return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    }
                case InfoManager.InfoMode.Pollution:
                    {
                        int num3;
                        int num4;
                        coreAI.GetPollutionRates((!coreAI.GetShowConsumption(buildingID, ref data)) ? 0 : 40, out num3, out num4);
                        if (num3 != 0)
                        {
                            return ColorUtils.LinearLerp(Singleton<InfoManager>.instance.m_properties.m_neutralColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor, Mathf.Clamp01((float)num3 * 0.01f));
                        }
                        return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    }
            }
            return coreAI.BaseColor(buildingID, ref data, infoMode);
        }

        public Color PlayerBuildingGetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode)
        {
            switch (infoMode)
            {
                case InfoManager.InfoMode.Electricity:
                    if (coreAI.m_electricityConsumption == 0)
                    {
                        return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    }
                    return coreAI.BaseColor(buildingID, ref data, infoMode);
                case InfoManager.InfoMode.Water:
                    if (coreAI.m_waterConsumption == 0 && coreAI.m_sewageAccumulation == 0)
                    {
                        return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    }
                    return coreAI.BaseColor(buildingID, ref data, infoMode);
                case InfoManager.InfoMode.CrimeRate:
                    if (data.m_citizenCount == 0)
                    {
                        return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    }
                    return coreAI.BaseColor(buildingID, ref data, infoMode);
                case InfoManager.InfoMode.Health:
                case InfoManager.InfoMode.Happiness:
                    break;
                default:
                    if (infoMode != InfoManager.InfoMode.Garbage)
                    {
                        if (infoMode != InfoManager.InfoMode.Entertainment)
                        {
                            return coreAI.BaseColor(buildingID, ref data, infoMode);
                        }
                    }
                    else
                    {
                        if (coreAI.m_garbageAccumulation == 0)
                        {
                            return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                        }
                        return coreAI.BaseColor(buildingID, ref data, infoMode);
                    }
                    break;
            }
            return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
        }
    }
}

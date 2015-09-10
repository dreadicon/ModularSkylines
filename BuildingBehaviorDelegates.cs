using System.Collections.Generic;
using ColossalFramework;
using System;
using UnityEngine;

namespace ModularSkylines
{
    public enum BuildingEvents
    {
        OnCreateBuilding,
        OnReleaseBuilding,
        OnBuildingLoaded,
        OnBuildingUpgraded,
        OnBuildingCompleted,
        OnSimulationStep,
        OnLevelUpCheck,
        OnActiveSimulationStep,
        GetNaturalResourceRate,
        GetEcconomyResourceRate,
        GetImmaterialResourceRate,
        GetElectricRate,
        GetWaterRate,
        GetGarbageRate,
        GetFireParameters,
        GetColor,
        CustomModule
    }

    public struct DelegateData
    {
        public Delegate delegateInstance;
        public Type delegateType;
        public Type moduleType;
        public BuildingEvents eventID;
        public string name => moduleType + "_" + eventID;

        public DelegateData(Delegate del, Type delType, Type module, BuildingEvents ID)
        {
            delegateInstance = del;
            moduleType = module;
            eventID = ID;
            delegateType = delType;
        }
    }

    public delegate void BuildingDelegate(ushort building, ref Building data, CoreAI core);
    public delegate void BuildingVersionDelegate(ushort building, ref Building data, uint version, CoreAI core);
    public delegate void BuildingColorDelegate(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, ref Color color);

    public delegate void SimulationDelegate(
        ushort building, ref Building data, ref Building.Frame frameData, CoreAI core);

}
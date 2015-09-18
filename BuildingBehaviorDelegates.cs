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
    public static Dictionary<BuildingEvents, RuntimeTypeHandle> EventMap = new Dictionary<BuildingEvents, RuntimeTypeHandle>
        {
            {BuildingEvents.OnCreateBuilding, typeof(BuildingDelegate).TypeHandle },
            {BuildingEvents.OnReleaseBuilding, typeof(BuildingDelegate).TypeHandle },
            {BuildingEvents.OnBuildingCompleted, typeof(BuildingDelegate).TypeHandle },
            {BuildingEvents.OnBuildingUpgraded, typeof(BuildingDelegate).TypeHandle },
            {BuildingEvents.OnBuildingLoaded, typeof(BuildingVersionDelegate).TypeHandle },
            {BuildingEvents.OnSimulationStep, typeof(BuildingDelegate).TypeHandle},

        };


    public class BehaviorConfig
    {
        public static Dictionary<string, List<BehaviorConfig>> Library;

        public string behavior;
        
        //Building Lifecycle events
        public BuildingDelegate OnCreateBuilding;
        public BuildingDelegate OnReleaseBuilding;
        public BuildingDelegate OnBuildingCompleted;
        public BuildingDelegate OnBuildingUpgraded;
        public BuildingVersionDelegate OnBuildingLoaded;

        //Building query events
        public BuildingCheckDelegate CheckUpgrade;
        public BuildingCheckDelegate CheckProblems;
        public BuildingColorDelegate GetColor;


        //Simulation events
        public SimulationDelegate OnSimulationStep;
        public SimulationDelegate OnActiveSimulationStep;

        //DataModule events
        //Note that each module a behavior touches can have a delegate for each of these.
        public Dictionary<RuntimeTypeHandle, object> OnGet;
        public Dictionary<RuntimeTypeHandle, object> OnCreate;
        public Dictionary<RuntimeTypeHandle, object> OnLoad;

        //
    }

    public struct TypeSaveInformation
    {
        public string typeName;
        public string typeNamespace;
        public string[] typeAliases;
    }
    
    public class BehaviorConfigAttribute : Attribute
    {

    }

    public enum ModuleEvents
    {
        OnCreate,
        OnLoad,
        OnGet,

    }

    public struct DelegateData
    {
        public Delegate delegateInstance;
        public Type delegateType;
        public Type moduleType;
        public BuildingEvents[] eventID;
        public string name => moduleType + "_" + eventID;

        public DelegateData(Delegate del, Type delType, Type module, BuildingEvents[] ID)
        {
            delegateInstance = del;
            moduleType = module;
            eventID = ID;
            delegateType = delType;
        }
    }

    public delegate void DataModuleEvent<T>(CoreAI core, T data) where T : DataModule<T>;
    public delegate void BuildingDelegate(ushort building, ref Building data, CoreAI core);
    public delegate bool BuildingCheckDelegate(ushort building, ref Building data, CoreAI core);
    public delegate void BuildingVersionDelegate(ushort building, ref Building data, uint version, CoreAI core);
    public delegate void BuildingColorDelegate(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, ref Color color);

    public delegate void SimulationDelegate(
        ushort building, ref Building data, ref Building.Frame frameData, CoreAI core);

}
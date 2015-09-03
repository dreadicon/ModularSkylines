using System.Collections.Generic;
using ColossalFramework;
using System;
using UnityEngine;

namespace ModularSkylines
{
    /// <summary>
    /// A module defining building behavior in one or more ways. Make these and add them to buildings to add any kind of static/non-active-simulation behavior.
    /// </summary>
    public abstract class BuildingModule
    {
        public List<ModuleManager.DelegateData> delegates;
        public List<EditorInfoDisplay.ElementProperties> DisplayElements;
        public List<IngameInfoDisplay.ElementProperties> OptionsFields;

        //Events for building lifecycle. Optional implementation.
        public virtual void OnCreateBuilding(ushort buildingID, ref Building data, CoreAI core) { }
        public virtual void OnReleaseBuilding(ushort buildingID, ref Building data, CoreAI core) { }
        public virtual void OnBuildingLoaded(ushort buildingID, ref Building data, uint version, CoreAI core) { }
        public virtual void OnBuildingCompleted(ushort buildingID, ref Building data, CoreAI core) { }
        public virtual void OnBuildingUpgraded(ushort buildingID, ref Building data, CoreAI core) { }

        public int BuildCostModifier = 0;
    }

    /// <summary>
    /// A child of the BuildingModule class, used explicitly for active/live simulation effect. These will impact performance! So take care what you do with them.
    /// Note that this class, unlike the BuildingModule class, has a dedicated instance created per-object. It can store data unique to the building instance.
    /// </summary>
    public abstract class SimulationBuildingModule : BuildingModule
    {
        public CoreAI core; // Saves a reference to the building which calls it, in case data from it is needed.

        public void OnSimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData) { }
        public void OnActiveSimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData) { }
    }

    public class ModuleData
    {
        //flags for which resource lists to add this module to. probably should rework this to be more elegant.
        public bool HasElectric = false;
        public bool HasWater = false;
        public bool HasGarbage = false;
        public bool HasNatural = false;
        public bool HasEconomy = false;
        public bool HasImmaterial = false;
        public bool HasCustom = false;
    }

    // Module Interfaces for bahaviors
    interface INaturalResourceModule
    {
        List<NaturalResourceManager.Resource> GetNaturalResources();
        void GetNaturalResourceRate(ref int givenValue, ushort buildingID, ref Building data, NaturalResourceManager.Resource resource);
    }

    interface IEconomyResourceModule
    {
        List<EconomyManager.Resource> GetEconomyResources(); 
        void GetEconomyResourceRate(ref int givenValue, ushort buildingID, ref Building data, EconomyManager.Resource resource);
    }

    interface IImmaterialResourceModule
    {
        List<ImmaterialResourceManager.Resource> GetImmaterialResources(); 
        void GetImmaterialResourceRate(ref int givenValue, ushort buildingID, ref Building data, ImmaterialResourceManager.Resource resource);
    }

    interface IElectricModule
    {
        void GetElectricRate(ref int givenValue, ushort buildingID, ref Building data);
    }

    interface IWaterModule
    {
        void GetWaterRate(ref int givenValue, ushort buildingID, ref Building data);
    }

    interface IGarbageModule
    {
        void GetGarbageRate(ref int givenValue, ushort buildingID, ref Building data);
    }

    interface IFireParameters
    {
        void GetFireParameters(ushort buildingID, ref Building buildingData, out int fireHazard, out int fireSize, out int fireTolerance);
    }

    interface ILevelUpModule
    {
        void OnLevelUpCheck(ushort buildingID, ref Building data);
    }

    interface IColorModule
    {
        int GetPriority(); // Returns how early this should be run as a priority
        bool GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, ref Color color);
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ColossalFramework;
using UnityEngine;
using System.Xml.Serialization;
using ColossalFramework.Packaging;

namespace ModularSkylines
{

    public class ModuleManager : Singleton<ModuleManager>
    {
        public const string assetModuleFileName = "modules.xml";

        public static Dictionary<BuildingEvents, Type> EventMap = new Dictionary<BuildingEvents, Type>
        {
            {BuildingEvents.OnCreateBuilding, typeof(BuildingDelegate) },
            {BuildingEvents.OnReleaseBuilding, typeof(BuildingDelegate) },
            {BuildingEvents.OnBuildingCompleted, typeof(BuildingDelegate) },
            {BuildingEvents.OnBuildingUpgraded, typeof(BuildingDelegate) },
            {BuildingEvents.OnBuildingLoaded, typeof(BuildingVersionDelegate) },
            {BuildingEvents.OnSimulationStep, typeof(BuildingDelegate)},

        };

        public abstract class Module
        {
            public readonly string ModuleName;
            public readonly string ModuleVerboseName;
            public readonly Type defaultBehavior;
            public readonly List<Type> defaultDataModules;
        }
        public class Residential : Module { }
        public class ResidentialLow : Residential { }
        public class ResidentialHigh : Residential { }
        public class Industrial : Module { }
        public class IndustrialForestry : Industrial { }
        public class IndustrialFarming : Industrial { }
        public class IndustrialOil : Industrial { }
        public class IndustrialOre : Industrial { }
        public class IndustrialGeneric : Industrial { }
        public class Commercial : Module { }
        public class CommercialLow : Commercial { }
        public class CommercialHigh : Commercial { }
        public class FireDepartment : Module { }
        public class HealthCare : Module { }
        public class Education : Module { }
        public class Water : Module { }
        public class Electricity : Module { }
        public class Office : Module { }
        public class Garbage : Module { }
        public class PoliceDepartment : Module { }
        public class Monument : Module { }
        public class GovernmentTransport : Module { }


        // Collections for accessing and coordinating the delegates.
        private static List<Type> loadedTypes = new List<Type>();
        private List<DelegateData> delegates;
        public Dictionary<BuildingEvents, List<DelegateData>> delegateEventMapping;
        public Dictionary<string, DelegateData> delegateNameDictionary;
        public Dictionary<string, DelegateData> defaultDelegates;
        public Dictionary<string, BuildingDelegate>

        /// <summary>
        /// Method for adding a delegate to the collection. 
        /// </summary>
        /// <param name="type">Type of the class containing the delegate</param>
        /// <param name="eventInfo">MethodInfo for the method to be invoked by the delegate</param>
        public void AddDelegate(Type type, MethodInfo eventInfo)
        {
            if (delegateNameDictionary.ContainsKey(type + "_" + eventInfo.Name))
            {
                Debug.Log("Warning: tried to add event delegate " + eventInfo.Name + " for module " + type + ", but an entry already exists!");
                return;
            }
            BuildingEvents eventID = (BuildingEvents)Enum.Parse(typeof(BuildingEvents), eventInfo.Name);
            var delType = EventMap[eventID];
            Delegate newDelegate = Delegate.CreateDelegate(delType, eventInfo);
            
            DelegateData newDelegateData = new DelegateData(newDelegate, delType, type, eventID);

            delegates.Add(newDelegateData);
            delegateEventMapping[eventID].Add(newDelegateData);
            delegateNameDictionary[newDelegateData.name] = newDelegateData;
        }

        /// <summary>
        /// Using reflection, this method finds all classes flagged as modules, then passes the methods
        /// to the appropriate handler. This should only be called once, at program load.
        /// </summary>
        public void LoadBuildingModules()
        {
            var moduleTypes =
                from a in AppDomain.CurrentDomain.GetAssemblies().AsParallel()
                from t in a.GetTypes()
                let moduleAttribute = t.GetCustomAttribute(typeof(BuildingModuleAttribute))
                where moduleAttribute != null

                select new { Module = t, ModuleAttribute = (BuildingModuleEventAttribute)moduleAttribute};

            foreach (var module in moduleTypes.ToList())
            {
                var methods = 
                    from m in module.Module.GetMethods()
                    let methodAttribute = m.GetCustomAttribute(typeof (BuildingModuleEventAttribute))
                    where methodAttribute != null
                    select new {Method = m, MethodAttribute = methodAttribute};

                if(loadedTypes.Contains(module.Module)) continue;

                foreach (var method in methods.ToList())
                {
                    AddDelegate(module.Module, method.Method);
                    //TODO: handle module defaults here using attribute data.
                }
                
                loadedTypes.Add(module.Module);
            }
        } 

        void OnAwake()
        {
            if(loadedTypes.Count < 1) LoadBuildingModules();
        }

        public void LoadAssetModuleConfigs()
        {
            foreach (Package package in PackageManager.allPackages)
            {
                foreach (Package.Asset asset in package)
                {
                    if (asset.type == UserAssetType.CustomAssetMetaData)
                    {
                        string dir = Path.GetDirectoryName(asset.pathOnDisk) + assetModuleFileName;
                        if (File.Exists(dir))
                        {
                            
                        }
                    }
                }
            }

        }



        public void CreateOrOverwriteAssetConfig(Package.Asset asset)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class BuildingModuleAttribute : Attribute
    {
        
    }

    //Functions attributed with a service other than 'None' for a service type are applied when no module is assigned
    //to a given simulation group, and a 'default' is available.
    [AttributeUsage(AttributeTargets.Method)]
    public class BuildingModuleEventAttribute : Attribute
    {
        public ItemClass.Service buildingDefault = ItemClass.Service.None;

    }
}

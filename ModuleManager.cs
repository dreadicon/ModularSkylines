using System;
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
    public enum BuildingEvents
    {
        OnCreateBuilding,
        OnReleaseBuilding,
        OnBuildingLoaded,
        OnBuildingUpgraded,
        OnBuildingCompleted
    }
    public class ModuleManager : Singleton<ModuleManager>
    {
        public readonly string assetModuleFileName = "modules.xml";

        public static Dictionary<BuildingEvents, Type> EventMap = new Dictionary<BuildingEvents, Type>
        {
            {BuildingEvents.OnCreateBuilding, typeof(BuildingDelegate) },
            {BuildingEvents.OnReleaseBuilding, typeof(BuildingDelegate) },
            {BuildingEvents.OnBuildingCompleted, typeof(BuildingDelegate) },
            {BuildingEvents.OnBuildingUpgraded, typeof(BuildingDelegate) },
            {BuildingEvents.OnBuildingLoaded, typeof(OnBuildingLoaded) },
            {BuildingEvents.OnCreateBuilding, typeof(BuildingDelegate)},

        };

        public delegate void BuildingDelegate(ushort building, ref Building data, CoreAI core);
        public delegate void OnBuildingLoaded(ushort building, ref Building data, uint version, CoreAI core);
        public delegate void BuildingColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode);


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

        // Collections for accessing and coordinating the delegates.
        private List<DelegateData> delegates;
        public Dictionary<BuildingEvents, List<DelegateData>> delegateEventMapping;
        public Dictionary<string, DelegateData> delegateNameDictionary;
        public Dictionary<string, DelegateData> defaultDelegates;
        /// <summary>
        /// Method for adding a delegate to the collection. 
        /// </summary>
        /// <param name="type">Type of the class containing the delegate</param>
        /// <param name="eventInfo">MethodInfo for the method to be invoked by the delegate</param>
        public void AddDelegate(Type type, MethodInfo eventInfo)
        {
            BuildingEvents eventID = (BuildingEvents)Enum.Parse(typeof(BuildingEvents),eventInfo.Name);
            if (!eventInfo.IsStatic)
            {
                Debug.Log("Warning: tried to load a non-static method. How is this even possible!?");
                return;
            }
            foreach (var del in delegates)
            {
                if (del.eventID == eventID && type == del.moduleType)
                {
                    Debug.Log("Warning: tried to add event delegate " + eventID + " for module " + type + ", but an entry already exists!");
                    return;
                }
            }
            var delType = EventMap[eventID];
            Delegate newDelegate = Delegate.CreateDelegate(delType, eventInfo);
            
            DelegateData newDelegateData = new DelegateData(newDelegate, delType, type, eventID);

            delegates.Add(newDelegateData);
            delegateEventMapping[eventID].Add(newDelegateData);
            delegateNameDictionary[newDelegateData.name] = newDelegateData;
        }

        private List<Type> loadedTypes = new List<Type>();

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

                foreach (var method in methods.ToList())
                {
                    AddDelegate(module.Module, method.Method);
                }

                loadedTypes.Add(module.Module);
            }
        } 

        void OnAwake()
        {
            
        }

        public void LoadAllModules()
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
                            asset.name
                        }
                    }
                }
            }

        }

        public string CreateOrOverwriteAssetConfig(Package.Asset asset, )
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

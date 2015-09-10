using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using ModularSkylines.VanillaModules;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ColossalFramework.IO;
using System.Reflection;

namespace ModularSkylines
{

    public class BuildingBehaviorManager
    {
        public const string BehaviorDefinitionFile = "Behaviors.xml";
        public const string CachedDefinitionFile = "BehaviorsCache.xml";
        public const string VanillaDensity1Ending = "Low";
        public const string VanillaDensity2Ending = "High";
        public const byte VanillaDensity1 = 1;
        public const byte VanillaDensity2 = 2;

        /// <summary>
        /// Struct which defines behaviors of a given asset once loaded, and for saving purposes.
        /// If multiple subtype/densities are desired, declare multiple cases of the BuildingBehavior.
        /// The first 128 types and 64 subtype values are reserved for now, for both vanilla type/subtype use and for potentially common
        /// mod usage. See documentation for current assignment. Note that for each custom type, all 256 subtypes are available.
        /// -'type' denotes the building's functionality type, i.e. residential, commercial, education, medical, garbage, etc.
        /// -'density' denotes the density, if applicable, for the given type
        /// -'subtype' is the subtype of a given type, i.e. farming, oil, highschool, etc.
        /// </summary>
        public class BuildingConfig
        {
            public BuildingBehavior[] behaviors;
            public BuildingBehavior primaryBehavior;
            public bool playerControlled;
            public byte maxLevel => primaryBehavior.maxLevel;
            public byte density => primaryBehavior.density;
        }
        public struct BuildingBehavior
        {
            public byte type;
            public byte density;
            public byte subtype;
            public byte maxLevel;
            public bool useAutogen;
        }

        // Used to declare a given behavior type. Reference data only.
        public struct BuildingBehaviorType
        {
            public string name;
            public byte type;
            public byte defaultMaxDensity;
            public byte defaultMaxLevel;
            public Subtype[] subtypes;
            public string[] defaultDataModules;
        }

        public struct Subtype
        {
            public string name;
            public byte type;
            public byte maxDensity;
            public byte maxLevel;
            public string[] defaultDataModules;
        }

        public static Dictionary<ItemClass.Service, byte> vanillaServiceMapping = new Dictionary<ItemClass.Service, byte>();
        public static Dictionary<ItemClass.SubService, byte> vanillaSubserviceMapping = new Dictionary<ItemClass.SubService, byte>();
        public static List<BuildingBehaviorType> behaviors = new List<BuildingBehaviorType>();
        public static Dictionary<string, byte> getBehaviorIDByName = new Dictionary<string, byte> {};
        
        public static void InitializeTypeMap()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<BuildingBehaviorType>));

            byte behaviorTypeCount = 0; //Note this is 0-indexed

            //Handle vanilla types, creating appropriate behavior for each.
            Type vanillaServiceType = typeof(ItemClass.Service);
            Type vanillaSubserviceType = typeof(ItemClass.SubService);

            SortedDictionary<int, string> serviceDict = new SortedDictionary<int, string> (Tools.EnumToDictionary<ItemClass.Service>());
            var subServiceDict = Tools.EnumToDictionary<ItemClass.SubService>();
            

            foreach (var entry in serviceDict)
            {
                List<Subtype> subtypes = new List<Subtype>();
                var behavior = new BuildingBehaviorType();
                behavior.defaultMaxDensity = 0;
                behavior.defaultMaxLevel = VanillaData.GetMaxLevel((ItemClass.Service)entry.Key);
                behavior.name = entry.Value;
                behavior.type = behaviorTypeCount;
                behavior.defaultDataModules = VanillaData.VanillaServicesData[(ItemClass.Service)entry.Key];
                foreach(var pair in subServiceDict)
                {
                    if(pair.Value.StartsWith(entry.Value))
                    {
                        // Handle vanilla density levels, which are currently exclusive of subservice types, so they go under default for services
                        if (pair.Value.EndsWith(VanillaDensity1Ending)) behavior.defaultMaxDensity = VanillaDensity1;
                        else if (pair.Value.EndsWith(VanillaDensity2Ending)) behavior.defaultMaxDensity = VanillaDensity2;
                        else
                        {
                            var subtype = new Subtype();

                            if (pair.Key == (int)ItemClass.SubService.IndustrialGeneric)
                                subtype.maxLevel = VanillaData.GenericIndustryMaxLevel - 1;
                            else
                                subtype.maxLevel = behavior.defaultMaxLevel;

                            subtype.maxDensity = behavior.defaultMaxDensity;
                            subtype.name = pair.Value;
                            subtype.defaultDataModules = behavior.defaultDataModules.Concat(VanillaData.VanillaSubservicesData[(ItemClass.SubService)pair.Key]).ToArray();
                            subtype.type = (byte)subtypes.Count;
                            subtypes.Add(subtype);
                            vanillaSubserviceMapping[(ItemClass.SubService)entry.Key] = (byte)subtypes.Count;
                        }
                    }
                }
                behavior.subtypes = subtypes.ToArray();
                behaviors.Insert(behaviorTypeCount, behavior);
                getBehaviorIDByName[behavior.name] = behaviorTypeCount;
                vanillaServiceMapping[(ItemClass.Service)entry.Key] = behaviorTypeCount;
                behaviorTypeCount++;
            }

            string corePath = Path.Combine(DataLocation.modsPath, MethodBase.GetCurrentMethod().DeclaringType.Namespace);
            string cachePath = Path.Combine(corePath, CachedDefinitionFile);
            foreach(var directory in Directory.GetDirectories(DataLocation.modsPath))
            {
                string modPath = Path.Combine(DataLocation.modsPath, directory);
                modPath = Path.Combine(modPath, BehaviorDefinitionFile);
                if(File.Exists(modPath))
                {
                    List<BuildingBehaviorType> result;
                    try
                    {
                        using (StreamReader reader = new StreamReader(modPath))
                        {
                            result = (List<BuildingBehaviorType>)xmlSerializer.Deserialize(reader);
                        }
                        if(result != null)
                        {
                            // Just want to say, I absolutely hate this code. Seriously. But it should work, so I will make it pretty later.
                            foreach (var behavior in result)
                            {
                                //First check if the BehaviorType already exists, and if it does merge the SubTypes. Otherwise, add it.
                                byte behaviorID;
                                if (getBehaviorIDByName.TryGetValue(behavior.name, out behaviorID))
                                {
                                    var existingBehavior = behaviors[behaviorID];
                                    var updatedSubtypes = new List<Subtype>(existingBehavior.subtypes);
                                    foreach(var newSubtype in behavior.subtypes)
                                    {
                                        bool subtypeExists = false;
                                        foreach (var existingSubtype in updatedSubtypes)
                                        {
                                            if (newSubtype.name == existingSubtype.name)
                                            {
                                                subtypeExists = true;
                                            }
                                        }
                                        if (!subtypeExists)
                                        {
                                            var mutableCopy = newSubtype;
                                            mutableCopy.type = (byte)updatedSubtypes.Count;
                                            updatedSubtypes.Insert(mutableCopy.type, mutableCopy);
                                        }
                                    }
                                    existingBehavior.subtypes = updatedSubtypes.ToArray<Subtype>();
                                    behaviors[behaviorID] = existingBehavior;
                                }
                                else
                                {
                                    bool validName = true;
                                    foreach(var existingBehavior in behaviors)
                                    {
                                        if (existingBehavior.name == behavior.name) validName = false;
                                    }
                                    
                                    if (validName) {
                                        var mutableCopy = behavior;
                                        mutableCopy.type = (byte)behaviors.Count;
                                        behaviors.Insert(behaviors.Count, mutableCopy);
                                        getBehaviorIDByName[mutableCopy.name] = mutableCopy.type;
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        result = null;
                        
                    }
                }

            }

            
        }


    }


    public class BuildingBehaviorAttribute : Attribute
    {
        
    }


}

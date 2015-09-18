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
    public class InitializationBehaviorArchtype : InitializationBehavior
    {
        public InitializationBehavior[] subtypes;
    }
    public class InitializationBehavior
    {
        public string name;
        public byte maxDensity;
        public byte maxLevel;
    }
    //This is the serialized data set, containing all info needed to define a building
    public struct InitializeBuildingConfig
    {
        //Name of the configuration
        public string name;
        //Alternative names. each time the building's config is renamed, it's old name is added to the aliases.
        //Note sure yet how to handle duplicates...might hook into the steam ID?
        public string[] aliases;
        public BehaviorType primary;
        public BehaviorType[] types;
        public bool playerControlled;
        public DataModuleBase[] moduleStartValues;
    }

    //Serialized class mapping an asset to a building config. 
    public class InitializeBuildingAsset
    {
        public string assetName;
        public string buildingConfigName;
    }
    public class BuildingBehaviorManager
    {
        public const string BehaviorDefinitionFile = "Behaviors.xml";
        public const string CachedDefinitionFile = "BehaviorsCache.xml";

        // Used to declare a given behavior type. Reference data only.


        public static List<InitializationBehaviorType> behaviors = new List<InitializationBehaviorType>();
        public static Dictionary<string, byte> getBehaviorIDByName = new Dictionary<string, byte> {};
        
        public static void InitializeTypeMap()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<InitializationBehaviorType>));

            VanillaData.InitializeVanillaBehaviors();

            string corePath = Path.Combine(DataLocation.modsPath, MethodBase.GetCurrentMethod().DeclaringType.Namespace);
            string cachePath = Path.Combine(corePath, CachedDefinitionFile);
            foreach(var directory in Directory.GetDirectories(DataLocation.modsPath))
            {
                string modPath = Path.Combine(DataLocation.modsPath, directory);
                modPath = Path.Combine(modPath, BehaviorDefinitionFile);
                if(File.Exists(modPath))
                {
                    List<InitializationBehaviorType> result;
                    try
                    {
                        using (StreamReader reader = new StreamReader(modPath))
                        {
                            result = (List<InitializationBehaviorType>)xmlSerializer.Deserialize(reader);
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
                                    var updatedSubtypes = new List<InitializationSubtype>(existingBehavior.subtypes);
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
                                    existingBehavior.subtypes = updatedSubtypes.ToArray<InitializationSubtype>();
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

    [AttributeUsage(AttributeTargets.Class)]
    public class BuildingBehaviorAttribute : Attribute
    {
        public string verboseName = "";
    }

    public class EventDelegateAttribute : Attribute
    {
        public BuildingEvents eventID;
    }

    public class DataModuleEventAttribute : Attribute
    {
        public Type type;
        public 
    }

    


}

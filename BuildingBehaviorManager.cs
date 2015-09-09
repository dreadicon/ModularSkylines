using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        /// <summary>
        /// Struct which defines behaviors of a given asset once loaded, and for saving purposes.
        /// If multiple subtype/densities are desired, declare multiple cases of the BuildingBehavior.
        /// The first 128 types and 64 subtype values are reserved for now, for both vanilla type/subtype use and for potentially common
        /// mod usage. See documentation for current assignment. Note that for each custom type, all 256 subtypes are available.
        /// -'type' denotes the building's functionality type, i.e. residential, commercial, education, medical, garbage, etc.
        /// -'density' denotes the density, if applicable, for the given type
        /// -'subtype' is the subtype of a given type, i.e. farming, oil, highschool, etc.
        /// </summary>
        public struct BuildingBehaviors
        {
            public BuildingBehavior[] behaviors;
            public bool ploppable;
        }
        public struct BuildingBehavior
        {
            public byte type;
            public byte density;
            public byte subtype;
            public byte maxLevel;
            public bool useAutogen;
        }

        // Used to declare a given behavior type.
        public struct BuildingBehaviorType
        {
            public string name;
            public byte typeID;
            public Subtype[] subtypes;
        }

        public struct Subtype
        {
            public byte id;
            public string name;
            public Subtype(byte newID, string newName)
            {
                id = newID;
                name = newName;
            }
        }


        public static Dictionary<byte, BuildingBehaviorType> behaviors = new Dictionary<byte, BuildingBehaviorType> { };
        public static Dictionary<string, byte> typeMapping = new Dictionary<string, byte> {};
        //public static Dictionary<byte, Dictionary<string, byte>> subtypeMapping = new Dictionary<byte, Dictionary<string, byte>> { };
        
        public static void InitializeTypeMap()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<BuildingBehaviorType>));
            
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
                            // Just want to say, I absolutely hate this code. But it should work, so I will make it pretty later.
                            foreach (var behavior in result)
                            {
                                if (behaviors.ContainsKey(behavior.typeID))
                                {
                                    var behaviorHolder = behaviors[behavior.typeID];
                                    var updatedSubtypes = new List<Subtype>(behaviorHolder.subtypes);
                                    var dictionary = updatedSubtypes.ToDictionary((item) => item.id, (item) => item.name);
                                    foreach(var subtype in behavior.subtypes)
                                    {
                                        if(subtype.id != null && subtype.name != null && !dictionary.ContainsKey(subtype.id))
                                        {
                                            dictionary[subtype.id] = subtype.name;
                                        }  
                                    }
                                    updatedSubtypes = new List<Subtype> { };
                                    foreach(var kvp in dictionary)
                                    {
                                        updatedSubtypes.Add(new Subtype(kvp.Key, kvp.Value));
                                    }
                                    behaviorHolder.subtypes = updatedSubtypes.ToArray<Subtype>();
                                    behaviors[behavior.typeID] = behaviorHolder;
                                }
                                else
                                {
                                    bool validName = true;
                                    foreach(var existingBehavior in behaviors)
                                    {
                                        if (existingBehavior.Value.name == behavior.name) validName = false;
                                    }
                                    if(validName) behaviors.Add(behavior.typeID, behavior);
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
            foreach (var behavior in behaviors)
            {
                typeMapping.Add(behavior.Value.name, behavior.Key);
            }

            
        }


    }


    public class BuildingBehaviorAttribute : Attribute
    {
        
    }


}

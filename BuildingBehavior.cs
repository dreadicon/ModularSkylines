using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSkylines
{
    /// <summary>
    /// Struct which defines behaviors of a given asset once loaded, and for saving purposes.
    /// If multiple subtype/densities are desired, declare multiple cases of the BuildingBehavior.
    /// -'type' denotes the building's functionality type, i.e. residential, commercial, education, medical, garbage, etc.
    /// -'density' denotes the density, if applicable, for the given type
    /// -'level' is the current level of a building behavior.
    /// -'subtype' is the subtype of a given type, i.e. farming, oil, highschool, etc.
    /// -'useAutogen' denotes if the building behavior should run it's autogen for a given behavior.
    /// </summary>
    [Serializable]
    public class BehaviorArchtype : BehaviorDefinition
    {
        public BehaviorDefinition[] subtypes;
    }
    [Serializable]
    public class BehaviorDefinition
    {
        public readonly string name;
        [NonSerialized]
        public readonly byte type;
        public readonly byte maxDensity;
        public readonly byte maxLevel;
        public readonly string[] defaultModules;
    }

    //NOTE: this class holds only immutable data about a building's configuration.
    [Serializable]
    public class BuildingConfig
    {
        public static Dictionary<ushort, BuildingConfig> library;

        public static void New(InitializeBuildingConfig initConfig, Dictionary<Type, DataModule> dataModules, ushort ID)
        {
            if (library.ContainsKey(ID)) return;
            BuildingConfig config = new BuildingConfig();
            config.name = initConfig.name;
            config.playerControlled = initConfig.playerControlled;
            config.primaryBehavior = initConfig.primary.name;
            config.primaryMaxLevel = initConfig.primary.defaultMaxLevel;

        }
        public string name;
        [NonSerialized]
        public ushort ID;
        public Dictionary<string, object> defaultDataModules;
        public string[] behaviors;
        public BehaviorDefinition primaryBehavior;
        public bool playerControlled;
        public byte primaryMaxLevel;
        public byte density;

        public bool hasBehavior(byte id)
        {
            if (primaryBehavior.type == id)
                return true;
            for (int i = 0; i < behaviors.Count(); i++)
                if (behaviors[i].type == id)
                    return true;
            return false;
        }

        public List<byte> GetUniqueTypes()
        {
            var types = new List<byte>();
            types.Add(primaryBehavior.type);
            if (behaviors.Count() > 0)
                for (int i = 0; i < behaviors.Count(); i++)
                    if (!types.Contains(behaviors[i].type))
                        types.Add(behaviors[i].type);
            return types;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSkylines
{
    public static class Tools
    {
        public static Dictionary<int, string> EnumToDictionary<T>()
        {
            var type = typeof(T);
            return Enum.GetValues(type).Cast<int>().ToDictionary(e => e, e => Enum.GetName(type, e));
        }

        //Dictionary extension
        public static TValue GetValueOrDefault<TKey, TValue>
    (this IDictionary<TKey, TValue> dictionary, TKey key, TValue def)
        {
            TValue ret;
            // Ignore return value
            if(dictionary.TryGetValue(key, out ret))
                return ret;
            return def;
        }

        
    }

    public class TypeDictionary
    {
        private Dictionary<RuntimeTypeHandle, object> internalDict;

        public bool TryGetValue<T>(out DataModuleEvent<T> returnValue) where T : DataModule<T>
        {
            object moduleEvent;
            if(internalDict.TryGetValue(typeof(T).TypeHandle,out moduleEvent))
            {
                returnValue = (DataModuleEvent<T>)moduleEvent;
                return true;
            }
            returnValue = null;
            return false;
        }
    }

}

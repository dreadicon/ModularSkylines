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
    }
}

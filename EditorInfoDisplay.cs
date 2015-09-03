using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularSkylines
{
    public class EditorInfoDisplay
    {
        public struct ElementProperties
        {
            public string name;
            public string verboseName;
            public Type fieldType;
            public bool isCollection;
            public ushort displayOrder;

            public ElementProperties(string name, Type fieldType, bool isCollection, string verboseName = "",
                ushort displayOrder = 0)
            {
                this.name = name;
                this.fieldType = fieldType;
                this.isCollection = isCollection;
                this.verboseName = verboseName == "" ? name : verboseName;
                this.displayOrder = displayOrder;
            }
        }
    }
}

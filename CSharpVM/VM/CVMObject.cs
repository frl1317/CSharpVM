using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpVM
{
    class CVMObject
    {
        //monoDef
        private TypeDefinition typeDef;

        //base object
        private CVMObject superObject;

        //类名
        public string Name;

        public CVMObject()
        {
        }

        public CVMObject(TypeDefinition def)
        {
            typeDef = def;
            Name = def.Name;
        }

    }
}

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

        Dictionary<string, object> feileds = new Dictionary<string, object>();
        Dictionary<string, MethodDefinition> methods = new Dictionary<string, MethodDefinition>();
        public CVMObject()
        {

        }
        public CVMObject(TypeDefinition def)
        {
            typeDef = def;
            Name = def.Name;

            for (int i = 0; i < typeDef.Fields.Count; i++)
            {
                FieldDefinition fieldDef = typeDef.Fields[i];
                feileds.Add(fieldDef.Name, fieldDef);
            }
            for (int i = 0; i < typeDef.Methods.Count; i++)
            {
                MethodDefinition methodDef = typeDef.Methods[i];
                methods.Add(methodDef.FullName, methodDef);
            }
        }

        public void SetFeiled(string name, object value)
        {
            if(feileds.ContainsKey(name))
            {
                feileds[name] = value;
            }
        }
        public object GetFeiled(string name)
        {
            if (feileds.ContainsKey(name))
            {
                return feileds[name];
            }
            return null;
        }
        public MethodDefinition GetMethod(string name)
        {
            return methods[name];
        }

        public static CVMObject Find(string name)
        {
            return new CVMObject();
        }
    }
}

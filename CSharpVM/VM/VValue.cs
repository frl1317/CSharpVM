using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSharpVM
{
    public enum ValueType
    {
        TYPE_INT,
        TYPE_LONG,
        TYPE_FLOAT,
        TYPE_DOUBLE,
    }

    [StructLayout(LayoutKind.Explicit, Size = 12)]
    struct VValue
    {
        [FieldOffset(0)]
        public ValueType valueType;

        [FieldOffset(4)]
        int iValue;
        [FieldOffset(4)]
        float dValue;
    }
}

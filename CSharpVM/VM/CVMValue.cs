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
        TYPE_BYTE,
        TYPE_SHORT,
        TYPE_INT,
        TYPE_LONG,
        TYPE_FLOAT,
        TYPE_DOUBLE,
    }

    [StructLayout(LayoutKind.Explicit, Size = 12)]
    struct CVMValue
    {
        [FieldOffset(0)]
        public ValueType valueType;

        [FieldOffset(4)]
        public int IValue;
        [FieldOffset(4)]
        public double DValue;
        [FieldOffset(4)]
        public float FValue;
        [FieldOffset(4)]
        public SByte BValue;
        [FieldOffset(4)]
        public long LValue;

        public CVMValue ChangeType(ValueType type)
        {
            this.valueType = type;
            return this;
        }

        public static bool operator ==(CVMValue a, CVMValue b)
        {
            switch (a.valueType)
            {
                case ValueType.TYPE_INT:
                    return a.IValue == b.IValue;
                case ValueType.TYPE_FLOAT:
                    return a.FValue == b.FValue;
                case ValueType.TYPE_LONG:
                    return a.LValue == b.LValue;
                case ValueType.TYPE_DOUBLE:
                    return a.DValue == b.DValue;
                case ValueType.TYPE_BYTE:
                    return a.BValue == b.BValue;
                default:
                    throw new NotSupportedException("未找到类型 " + a.GetType().Name);
            }
        }
        public static bool operator !=(CVMValue a, CVMValue b)
        {
            switch (a.valueType)
            {
                case ValueType.TYPE_INT:
                    return a.IValue != b.IValue;
                case ValueType.TYPE_FLOAT:
                    return a.FValue != b.FValue;
                case ValueType.TYPE_LONG:
                    return a.LValue != b.LValue;
                case ValueType.TYPE_DOUBLE:
                    return a.DValue != b.DValue;
                case ValueType.TYPE_BYTE:
                    return a.BValue != b.BValue;
                default:
                    throw new NotSupportedException("未找到类型 " + a.GetType().Name);
            }
        }
        public static bool operator <(CVMValue a, CVMValue b)
        {
            switch (a.valueType)
            {
                case ValueType.TYPE_INT:
                    return a.IValue < b.IValue;
                case ValueType.TYPE_FLOAT:
                    return a.FValue < b.FValue;
                case ValueType.TYPE_LONG:
                    return a.LValue < b.LValue;
                case ValueType.TYPE_DOUBLE:
                    return a.DValue < b.DValue;
                case ValueType.TYPE_BYTE:
                    return a.BValue < b.BValue;
                default:
                    throw new NotSupportedException("未找到类型 " + a.GetType().Name);
            }
        }
        public static bool operator >(CVMValue a, CVMValue b)
        {
            switch (a.valueType)
            {
                case ValueType.TYPE_INT:
                    return a.IValue > b.IValue;
                case ValueType.TYPE_FLOAT:
                    return a.FValue > b.FValue;
                case ValueType.TYPE_LONG:
                    return a.LValue > b.LValue;
                case ValueType.TYPE_DOUBLE:
                    return a.DValue > b.DValue;
                case ValueType.TYPE_BYTE:
                    return a.BValue > b.BValue;
                default:
                    throw new NotSupportedException("未找到类型 " + a.GetType().Name);
            }
        }
        public static CVMValue operator +(CVMValue a, CVMValue b)
        {
            switch (a.valueType)
            {
                case ValueType.TYPE_INT:
                    a.IValue += b.IValue;
                    break;
                case ValueType.TYPE_FLOAT:
                    a.FValue += b.FValue;
                    break;
                case ValueType.TYPE_LONG:
                    a.LValue += b.LValue;
                    break;
                case ValueType.TYPE_DOUBLE:
                    a.DValue += b.DValue;
                    break;
                case ValueType.TYPE_BYTE:
                    a.BValue += b.BValue;
                    break;
                default:
                    throw new NotSupportedException("未找到类型 " + a.GetType().Name);
            }
            return a;
        }

        public static CVMValue operator -(CVMValue a, CVMValue b)
        {
            switch (a.valueType)
            {
                case ValueType.TYPE_INT:
                    a.IValue -= b.IValue;
                    break;
                case ValueType.TYPE_FLOAT:
                    a.FValue -= b.FValue;
                    break;
                case ValueType.TYPE_LONG:
                    a.LValue -= b.LValue;
                    break;
                case ValueType.TYPE_DOUBLE:
                    a.DValue -= b.DValue;
                    break;
                case ValueType.TYPE_BYTE:
                    a.BValue -= b.BValue;
                    break;
                default:
                    throw new NotSupportedException("未找到类型 " + a.GetType().Name);
            }
            return a;
        }

        public object ToObject()
        {
            switch (valueType)
            {
                case ValueType.TYPE_INT:
                    return IValue;
                case ValueType.TYPE_FLOAT:
                    return FValue;
                case ValueType.TYPE_LONG:
                    return LValue;
                case ValueType.TYPE_DOUBLE:
                    return DValue;
                case ValueType.TYPE_BYTE:
                    return BValue;
                default:
                    throw new NotSupportedException("未找到类型 " + valueType);
            }
        }
    }
}

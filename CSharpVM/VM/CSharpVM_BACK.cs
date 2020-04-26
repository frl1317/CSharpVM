using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSharpVM
{
    class CSharpVM
    {
        const int MAX_STACK = 256;
        private int stackSize = 0;
        private object[] stack = new object[MAX_STACK];

        //所有模块的集合
        List<ModuleDefinition> modules;

        public CSharpVM()
        {
            stackSize = 0;
            modules = new List<ModuleDefinition>();
        }

        public void ImportModule(ModuleDefinition moduleDef)
        {
            modules.Add(moduleDef);
        }

        Dictionary<string, MethodDefinition> methodDefDic = new Dictionary<string, MethodDefinition>();

        private void Push(object value)
        {
            System.Diagnostics.Debug.Assert(stackSize < MAX_STACK);
            stack[stackSize++] = value;
        }

        private object Pop()
        {
            System.Diagnostics.Debug.Assert(stackSize > 0);
            return stack[--stackSize];
        }

        public void Load(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] buffur = new byte[fs.Length];
                fs.Read(buffur, 0, (int)fs.Length);
                //通过Mono.Cecil读取dll的pe信息
                AssemblyDefinition assemblyDef = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { ReadSymbols = true });
                var module = assemblyDef.MainModule;
                if (module.HasTypes)
                {
                    foreach (TypeDefinition typeDef in module.GetTypes()) //获取所有此模块定义的类型
                    {
                        System.Console.WriteLine(typeDef.FullName);

                        foreach (MethodDefinition methodDef in typeDef.Methods)
                        {
                            methodDefDic.Add(methodDef.FullName, methodDef);
                        }
                    }
                }
            }
        }

        //临时变量
        object[] localVar = new object[50];

        public object RunInterpreter(MethodDefinition methodDef)
        {
            Instruction instruction = methodDef.Body.Instructions[0];
            while (instruction != null)
            {
                Code code = instruction.OpCode.Code;
                switch (code)
                {
                    case Code.Nop:
                    case Code.Volatile:
                    case Code.Castclass:
                    case Code.Readonly:
                        break;
                    case Code.Ldc_I4:
                        {
                            System.Console.WriteLine("instruction.Operand:" + instruction.Operand.GetType().Name);
                            //将提供的 int32 值作为 int32 推送到计算堆栈上（短格式）。
                            VValue vValue = new VValue();
                            vValue.valueType = ValueType.TYPE_INT;
                            vValue.IValue = (int)instruction.Operand;
                            Push(vValue);
                        }
                        break;
                    case Code.Ldc_I4_S:
                        {
                            System.Console.WriteLine("instruction.Operand:" + instruction.Operand.GetType().Name);
                            //将提供的 int8 值作为 int32 推送到计算堆栈上（短格式）。
                            VValue vValue = new VValue();
                            vValue.valueType = ValueType.TYPE_INT;
                            vValue.BValue = (SByte)instruction.Operand;
                            Push(vValue);
                        }
                        break;
                    case Code.Ldc_I4_0:
                    case Code.Ldc_I4_1:
                    case Code.Ldc_I4_2:
                    case Code.Ldc_I4_3:
                    case Code.Ldc_I4_4:
                    case Code.Ldc_I4_5:
                    case Code.Ldc_I4_6:
                    case Code.Ldc_I4_7:
                    case Code.Ldc_I4_8:
                        {
                            //将整数值 x 作为 int32 推送到计算堆栈上。
                            VValue vValue = new VValue();
                            vValue.valueType = ValueType.TYPE_INT;
                            vValue.IValue = (int)(code - Code.Ldc_I4_0);
                            Push(vValue);
                        }
                        break;
                    case Code.Ldc_R4:
                        {
                            //将整数值 x 作为 int32 推送到计算堆栈上。
                            VValue vValue = new VValue();
                            vValue.valueType = ValueType.TYPE_FLOAT;
                            vValue.FValue = (float)instruction.Operand;
                            Push(vValue);
                        }
                        break; 
                    case Code.Stloc_S:
                        {
                            //从计算堆栈的顶部弹出当前值并将其存储在局部变量列表中的 index 处
                            VariableDefinition vardef = (VariableDefinition)instruction.Operand;
                            localVar[vardef.Index] = Pop();
                        }
                        break;
                    case Code.Stloc_0:
                    case Code.Stloc_1:
                    case Code.Stloc_2:
                    case Code.Stloc_3:
                        {
                            //从计算堆栈的顶部弹出当前值并将其存储到索引 x 处的局部变量列表中。
                            Byte idx = (Byte)(code - Code.Stloc_0);
                            localVar[idx] = Pop();
                        }
                        break;
                    case Code.Add:
                        {
                            VValue v2 = (VValue)Pop();
                            VValue v1 = (VValue)Pop();
                            Push(v2 + v1);
                        }
                        break;
                    case Code.Sub:
                        {
                            VValue v2 = (VValue)Pop();
                            VValue v1 = (VValue)Pop();
                            Push(v1 - v2);
                        }
                        break;
                    case Code.Cgt:
                        {
                            //比较两个值。如果第一个值大于第二个值，则将整数值 1 (int32) 推送到计算堆栈上；反之，将 0 (int32) 推送到计算堆栈上。
                            VValue v2 = (VValue)Pop();
                            VValue v1 = (VValue)Pop();
                            bool res = v1 > v2;
                            if (res)
                                Push(1);
                            else
                                Push(0);
                        }
                        break;
                    case Code.Beq:
                    case Code.Beq_S:
                        {
                            //如果两个值相等，则将控制转移到目标指令（短格式）。
                            VValue v2 = (VValue)Pop();
                            VValue v1 = (VValue)Pop();
                            bool res = v1 == v2;
                            if (res)
                            {
                                instruction = (Instruction)instruction.Operand;
                                continue;
                            }
                        }
                        break;
                    case Code.Clt:
                        {
                            //比较两个值。如果第一个值大于第二个值，则将整数值 1 (int32) 推送到计算堆栈上；反之，将 0 (int32) 推送到计算堆栈上。
                            VValue v2 = (VValue)Pop();
                            VValue v1 = (VValue)Pop();
                            bool res = v1 < v2;
                            if (res)
                                Push(1);
                            else
                                Push(0);
                        }
                        break;
                    case Code.Ldloc_S:
                        {
                            //将指定索引处的局部变量加载到计算堆栈上。
                            VariableDefinition vardef = (VariableDefinition)instruction.Operand;
                            Push(localVar[vardef.Index]);
                        }
                        break;
                    case Code.Ldloc_0:
                    case Code.Ldloc_1:
                    case Code.Ldloc_2:
                    case Code.Ldloc_3:
                        //将索引 0 处的局部变量加载到计算堆栈上
                        Push(localVar[code - Code.Ldloc_0]);
                        break;
                    case Code.Conv_I1:
                        {
                            Push(((VValue)Pop()).ChangeType(ValueType.TYPE_BYTE));
                        }
                        break;
                    case Code.Conv_I2:
                        {
                            Push(((VValue)Pop()).ChangeType(ValueType.TYPE_SHORT));
                        }
                        break;
                    case Code.Conv_I4:
                        {
                            Push(((VValue)Pop()).ChangeType(ValueType.TYPE_INT));
                        }
                        break;
                    case Code.Conv_I8:
                        {
                            Push(((VValue)Pop()).ChangeType(ValueType.TYPE_LONG));
                        }
                        break;
                    case Code.Conv_R4:
                        {
                            Push(((VValue)Pop()).ChangeType(ValueType.TYPE_FLOAT));
                        }
                        break;
                    case Code.Conv_R8:
                        {
                            Push(((VValue)Pop()).ChangeType(ValueType.TYPE_DOUBLE));
                        }
                        break;
                    case Code.Ldstr:
                        //推送对元数据中存储的字符串的新对象引用。
                        Push(instruction.Operand);
                        break;
                    case Code.Br_S:
                        {
                            //无条件地将控制转移到目标指令（短格式）。
                            instruction = (Instruction)instruction.Operand;
                            continue;
                        }
                        break;
                    case Code.Brfalse:
                    case Code.Brfalse_S:
                        {
                            int res = (int)Pop();
                            if(res == 0)
                            {
                                instruction = (Instruction)instruction.Operand;
                                continue;
                            }
                        }
                        break;
                    case Code.Brtrue:
                    case Code.Brtrue_S:
                        {
                            int res = (int)Pop();
                            if (res == 1)
                            {
                                instruction = (Instruction)instruction.Operand;
                                continue;
                            }
                        }
                        break;
                    case Code.Ret:
                        //从当前方法返回，并将返回值（如果存在）从调用方的计算堆栈推送到被调用方的计算堆栈上。
                        TypeReference returnType = methodDef.ReturnType;
                        if (returnType.Name != Type.GetType("System.Void").Name)
                            return Pop();
                        break;
                    case Code.Box:
                        object o = Pop();
                        Push(o);
                        break;
                    case Code.Call:
                        {
                            MethodReference methodReference = (MethodReference)instruction.Operand;
                            /*
                              MethodDefinition methDef;
                             methodDefDic.TryGetValue(methodReference.DeclaringType.FullName, out methDef);
                             if(methDef != null)
                             {
                                 Execute
                             }
                              */
#if true
                           //方式一 直接通过制定参数查找
                           Type[] types = new Type[methodReference.Parameters.Count];
                            object[] values = new object[methodReference.Parameters.Count];
                            for (int i = methodReference.Parameters.Count-1; i  >= 0; i--)
                            {
                                values[i] = Pop();
                                types[i] = values[i].GetType();
                            }

                            Type t = Type.GetType(methodReference.DeclaringType.FullName);
                            var methodInfo = t.GetMethod(methodReference.Name, types);
                            object res;
                            if (methodInfo.IsStatic)
                                res = methodInfo.Invoke(null, values);
                            else
                                res = methodInfo.Invoke(t, values);
                            if (res != null)
                                Push(res);
#else

                        //方式二， 遍历函数，通过参数对比
                        List<object> param = new List<object>();
                        for(int i = 0; i < methodReference.Parameters.Count; i++)
                        {
                            param.Add(Pop());
                        }
                        
                        foreach (var methodDef in t.GetMethods())
                        {
                            if(methodDef.GetParameters().Length == methodReference.Parameters.Count
                                && methodDef.Name == methodReference.Name
                                && methodDef.GetParameters()[0].ParameterType == typeof(string))
                            {
                                methodDef.Invoke(null, param.ToArray());
                            }
                        }
#endif

                            //AppDomain.CurrentDomain.CreateInstance（a.FullName，Test2.Class1）;
                        }
                        break;
                    case Code.Newobj:
                        {
                            MethodReference methodReference = (MethodReference)instruction.Operand;
                            //方式一 直接通过制定参数查找
                            Type[] types = new Type[methodReference.Parameters.Count];
                            object[] values = new object[methodReference.Parameters.Count];
                            for (int i = methodReference.Parameters.Count - 1; i >= 0; i--)
                            {
                                values[i] = Pop();
                                types[i] = values[i].GetType();
                            }

                            Type t = Type.GetType(methodReference.DeclaringType.FullName);
                            object inst = t.Assembly.CreateInstance(t.FullName);
                            Push(inst);
                        }
                        break;
                    case Code.Stfld:
                        {
                            object newData = Pop();
                            object obj = Pop();
                            FieldDefinition fieldDefinition = (FieldDefinition)instruction.Operand;
                            FieldInfo fieldInfo =  obj.GetType().GetField(fieldDefinition.Name);
                            if(newData is VValue)
                                fieldInfo.SetValue(obj, ((VValue)newData).ToObject());
                            else
                                fieldInfo.SetValue(obj, newData);
                        }
                        break;
                    case Code.Callvirt:
                        {
                            //对对象调用后期绑定方法，并且将返回值推送到计算堆栈上。
                            object obj = Pop();

                            MethodReference methodReference = (MethodReference)instruction.Operand;
                            Type[] types = new Type[methodReference.Parameters.Count];
                            object[] values = new object[methodReference.Parameters.Count];
                            for (int i = methodReference.Parameters.Count - 1; i >= 0; i--)
                            {
                                values[i] = Pop();
                                types[i] = values[i].GetType();
                            }

                            var methodInfo = obj.GetType().GetMethod(methodReference.Name, types);
                            object res = methodInfo.Invoke(obj, values);
                            if (res != null)
                            {
                                if(res is int)
                                {
                                    VValue vValue = new VValue();
                                    vValue.IValue = (int)res;
                                    vValue.valueType = ValueType.TYPE_INT;
                                    Push(vValue);
                                }
                                else if (res is float)
                                {
                                    VValue vValue = new VValue();
                                    vValue.FValue = (float)res;
                                    vValue.valueType = ValueType.TYPE_FLOAT;
                                    Push(vValue);
                                }
                                else if (res is long)
                                {
                                    VValue vValue = new VValue();
                                    vValue.LValue = (long)res;
                                    vValue.valueType = ValueType.TYPE_LONG;
                                    Push(vValue);
                                }
                                else if (res is SByte)
                                {
                                    VValue vValue = new VValue();
                                    vValue.BValue = (SByte)res;
                                    vValue.valueType = ValueType.TYPE_BYTE;
                                    Push(vValue);
                                }
                                else if (res is double)
                                {
                                    VValue vValue = new VValue();
                                    vValue.DValue = (double)res;
                                    vValue.valueType = ValueType.TYPE_DOUBLE;
                                    Push(vValue);
                                }
                                else
                                {
                                    Push(res);
                                }
                            }
                        }
                        break;
                    default:
                        throw new NotSupportedException("Not supported opcode " + code);
                }
                instruction = instruction.Next;
            }
            return null;
        }

        public object CreateInstance(string fullName)
        {
            Type t = Type.GetType(fullName);
            return t.Assembly.CreateInstance(t.FullName);
        }
    }
}

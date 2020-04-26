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
        private CVMStackFrame[] stacks = new CVMStackFrame[MAX_STACK];
        Dictionary<string, CVMObject> templateClasses = new Dictionary<string, CVMObject>();
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

        //所有动态函数集合
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

        public void Load(string dllPath)
        {
            using (FileStream fs = new FileStream(dllPath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffur = new byte[fs.Length];
                fs.Read(buffur, 0, (int)fs.Length);
                //通过Mono.Cecil读取dll的pe信息
                AssemblyDefinition assemblyDef = AssemblyDefinition.ReadAssembly(dllPath, new ReaderParameters { ReadSymbols = true });
                var module = assemblyDef.MainModule;
                if (module.HasTypes)
                {
                    foreach (TypeDefinition typeDef in module.GetTypes()) //获取所有此模块定义的类型
                    {
                        CVMObject obj = new CVMObject(typeDef);
                        templateClasses.Add(obj.Name, obj);
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
                            Push(instruction.Operand);
                        }
                        break;
                    case Code.Ldc_I4_S:
                        //将提供的 int8 值作为 int32 推送到计算堆栈上（短格式）。
                        Push(instruction.Operand);
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
                        //将整数值 x 作为 int32 推送到计算堆栈上。
                        Push(code - Code.Ldc_I4_0);
                        break;
                    case Code.Ldc_R4:
                        {
                            Push(instruction.Operand);
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
                            object v2 = Pop();
                            object v1 = Pop();
                            if (v1 is int)
                            {
                                Push(Convert.ToInt32(v1) + Convert.ToInt32(v2));
                            }
                            else if (v1 is float)
                            {
                                Push(Convert.ToSingle(v1) + Convert.ToSingle(v2));
                            }
                            else if (v1 is long)
                            {
                                Push(Convert.ToInt64(v1) + Convert.ToInt64(v2));
                            }
                            else if (v1 is double)
                            {
                                Push(Convert.ToDouble(v1) + Convert.ToDouble(v2));
                            }
                            else if (v1 is SByte)
                            {
                                Push(Convert.ToSByte(v1) + Convert.ToSByte(v2));
                            }
                            else
                            {
                                throw new NotSupportedException("未找到类型 " + v1.GetType().Name);
                            }
                        }
                        break;
                    case Code.Sub:
                        {
                            object v2 = Pop();
                            object v1 = Pop();
                            if (v1 is int)
                            {
                                Push(Convert.ToInt32(v1) - Convert.ToInt32(v2));
                            }
                            else if (v1 is float)
                            {
                                Push(Convert.ToSingle(v1) - Convert.ToSingle(v2));
                            }
                            else if (v1 is long)
                            {
                                Push(Convert.ToInt64(v1) - Convert.ToInt64(v2));
                            }
                            else if (v1 is double)
                            {
                                Push(Convert.ToDouble(v1) - Convert.ToDouble(v2));
                            }
                            else if (v1 is SByte)
                            {
                                Push(Convert.ToSByte(v1) - Convert.ToSByte(v2));
                            }
                            else
                            {
                                throw new NotSupportedException("未找到类型 " + v1.GetType().Name);
                            }
                        }
                        break;
                    case Code.Cgt:
                        {
                            //比较两个值。如果第一个值大于第二个值，则将整数值 1 (int32) 推送到计算堆栈上；反之，将 0 (int32) 推送到计算堆栈上。
                            object v2 = Pop();
                            object v1 = Pop();
                            bool res = false;
                            if (v1 is int)
                            {
                                res = Convert.ToInt32(v1) > Convert.ToInt32(v2);
                            }
                            else if (v1 is float)
                            {
                                res = Convert.ToSingle(v1) > Convert.ToSingle(v2);
                            }
                            else if (v1 is long)
                            {
                                res = Convert.ToInt64(v1) > Convert.ToInt64(v2);
                            }
                            else if (v1 is double)
                            {
                                res = Convert.ToDouble(v1) > Convert.ToDouble(v2);
                            }
                            else if (v1 is SByte)
                            {
                                res = Convert.ToSByte(v1) > Convert.ToSByte(v2);
                            }
                            else
                            {
                                throw new NotSupportedException("未找到类型 " + v1.GetType().Name);
                            }
                            if (res)
                                Push(1);
                            else
                                Push(0);
                        }
                        break;
                    case Code.Clt:
                        {
                            //比较两个值。如果第一个值大于第二个值，则将整数值 1 (int32) 推送到计算堆栈上；反之，将 0 (int32) 推送到计算堆栈上。
                            object v2 = Pop();
                            object v1 = Pop();
                            bool res = false;
                            if (v1 is int)
                            {
                                res = Convert.ToInt32(v1) < Convert.ToInt32(v2);
                            }
                            else if (v1 is float)
                            {
                                res = Convert.ToSingle(v1) < Convert.ToSingle(v2);
                            }
                            else if (v1 is long)
                            {
                                res = Convert.ToInt64(v1) < Convert.ToInt64(v2);
                            }
                            else if (v1 is double)
                            {
                                res = Convert.ToDouble(v1) < Convert.ToDouble(v2);
                            }
                            else if (v1 is SByte)
                            {
                                res = Convert.ToSByte(v1) < Convert.ToSByte(v2);
                            }
                            else
                            {
                                throw new NotSupportedException("未找到类型 " + v1.GetType().Name);
                            }
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
                        Push(Convert.ToSByte(Pop()));
                        break;
                    case Code.Conv_I2:
                        Push(Convert.ToInt16(Pop()));
                        break;
                    case Code.Conv_I4:
                        Push(Convert.ToInt32(Pop()));
                        break;
                    case Code.Conv_I8:
                        Push(Convert.ToInt64(Pop()));
                        break;
                    case Code.Conv_R4:
                        Push(Convert.ToSingle(Pop()));
                        break;
                    case Code.Conv_R8:
                        Push(Convert.ToDouble(Pop()));
                        break;
                    case Code.Ldstr:
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
                        return Pop();
                    case Code.Beq:
                    case Code.Beq_S:
                        {
                            //如果两个值相等，则将控制转移到目标指令（短格式）。
                            object v2 = Pop();
                            object v1 = Pop();
                            bool res = false;
                            if (v1 is int)
                            {
                                res = Convert.ToInt32(v1) == Convert.ToInt32(v2);
                            }
                            else if (v1 is float)
                            {
                                res = Convert.ToSingle(v1) == Convert.ToSingle(v2);
                            }
                            else if (v1 is long)
                            {
                                res = Convert.ToInt64(v1) == Convert.ToInt64(v2);
                            }
                            else if (v1 is double)
                            {
                                res = Convert.ToDouble(v1) == Convert.ToDouble(v2);
                            }
                            else if(v1 is SByte)
                            {
                                res = Convert.ToSByte(v1) == Convert.ToSByte(v2);
                            }
                            else
                            {
                                throw new NotSupportedException("未找到类型 " + v1.GetType().Name);
                            }
                            if (res)
                            {
                                instruction = (Instruction)instruction.Operand;
                                continue;
                            }
                        }
                        break;
                    case Code.Box:
                        object o = Pop();
                        Push(o);
                        break;
                    case Code.Call:
                        {
                            MethodReference methodRef = (MethodReference)instruction.Operand;
                            /*
                              MethodDefinition methDef;
                             methodDefDic.TryGetValue(methodRef.DeclaringType.FullName, out methDef);
                             if(methDef != null)
                             {
                                 Execute
                             }
                              */
                           //方式一 直接通过制定参数查找
                           Type[] types = new Type[methodRef.Parameters.Count];
                            object[] values = new object[methodRef.Parameters.Count];
                            for (int i = methodRef.Parameters.Count-1; i  >= 0; i--)
                            {
                                values[i] = Pop();
                                types[i] = values[i].GetType();
                            }

                            Type t = Type.GetType(methodRef.DeclaringType.FullName);
                            var methodInfo = t.GetMethod(methodRef.Name, types);
                            object res;
                            if (methodInfo.IsStatic)
                                res = methodInfo.Invoke(null, values);
                            else
                                res = methodInfo.Invoke(t, values);
                            Push(res);
                        }
                        break;
                    case Code.Callvirt:
                        {
                            //对对象调用后期绑定方法，并且将返回值推送到计算堆栈上。
                            object obj = Pop();

                            MethodReference methodRef = (MethodReference)instruction.Operand;
                            Type[] types = new Type[methodRef.Parameters.Count];
                            object[] values = new object[methodRef.Parameters.Count];
                            for (int i = methodRef.Parameters.Count - 1; i >= 0; i--)
                            {
                                values[i] = Pop();
                                types[i] = values[i].GetType();
                            }

                            var methodInfo = obj.GetType().GetMethod(methodRef.Name, types);
                            object res = methodInfo.Invoke(obj, values);
                            Push(res);
                        }
                        break;
                    case Code.Newobj:
                        {
                            //构造函数
                            MethodReference methodRef = (MethodReference)instruction.Operand;
                            
                            Type[] types = new Type[methodRef.Parameters.Count];
                            object[] values = new object[methodRef.Parameters.Count];
                            for (int i = methodRef.Parameters.Count - 1; i >= 0; i--)
                            {
                                values[i] = Pop();
                                types[i] = values[i].GetType();
                            }

                            Type t = Type.GetType(methodRef.DeclaringType.FullName);
                            //object inst = t.Activator.CreateInstance(t.FullName);
                            object inst = System.Activator.CreateInstance(t, values);
                            Push(inst);
                        }
                        break;
                    case Code.Stfld:
                        {
                            object newData = Pop();
                            object obj = Pop();
                            FieldDefinition fieldDefinition = (FieldDefinition)instruction.Operand;
                            FieldInfo fieldInfo =  obj.GetType().GetField(fieldDefinition.Name);
                            fieldInfo.SetValue(obj, newData);
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

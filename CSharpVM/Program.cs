using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpVM
{
    class Program
    {
        //static List<Instruction> simulationInstruction = new List<Instruction>();
        static MethodDefinition simulationMethodDef;
        static void Main(string[] args)
        {
            CSharpVM vm = new CSharpVM();

            string path = @"D:\GitHub\CSharpVM\CSharpVM\bin\Debug\CSharpVM.exe";
            vm.Load(path);

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] buffur = new byte[fs.Length];
                fs.Read(buffur, 0, (int)fs.Length);
                AppDomain appDomain = AppDomain.CurrentDomain;
                //通过Mono.Cecil读取dll的pe信息
                AssemblyDefinition assemblyDef = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { ReadSymbols = true });
                var module = assemblyDef.MainModule;
                if (module.HasTypes)
                {
                    vm.ImportModule(module);

                    foreach (TypeDefinition typeDef in module.GetTypes()) //获取所有此模块定义的类型
                    {
                        System.Console.WriteLine(typeDef.FullName);

                        foreach (MethodDefinition methodDef in typeDef.Methods)
                        {
                            Console.WriteLine("IsGenericInstance:" + methodDef.IsGenericInstance.ToString());
                            Console.WriteLine("MetadataToken:" + methodDef.MetadataToken);
                            System.Console.WriteLine(typeDef.FullName + "----" + methodDef);
                            if(methodDef.Name == "Test")
                            {
                                simulationMethodDef = methodDef;
                                foreach (Instruction v in methodDef.Body.Instructions)//Instructions
                                {
                                    System.Console.WriteLine("Instructions:" + v.OpCode.Code + "  OpCode.Name:" + v.OpCode.Name + "  v.Operand:" + v.Operand + "  OpCode.Value:" + v.OpCode.Value);
                                    //simulationInstruction.Add(v);
                                }
                            }
                            foreach (Instruction v in methodDef.Body.Instructions)//Instructions
                            {
                                //System.Console.WriteLine("Instructions:" + v.OpCode.Code + "  OpCode.Name:" + v.OpCode.Name + "  v.Operand:" + v.Operand + "  OpCode.Value:" + v.OpCode.Value);
                            }
                            /*
                         
                            foreach (VariableDefinition v in methodDef.Body.Variables)//Instructions
                            {
                                System.Console.WriteLine("v.OpCode.Code:" + v.Operand);
                            }    
                         */
                        }
                    }
                    System.Console.WriteLine("IL代码运算----------");
                    System.Console.WriteLine(vm.RunInterpreter(simulationMethodDef));
                    System.Console.WriteLine("IL代码运算结束----------");
                    System.Console.WriteLine("");
                    System.Console.WriteLine("");
                    System.Console.WriteLine("");
                    //c = (int)stackList.Pop();

                    System.Console.WriteLine("C#代码运算----------");
                    System.Console.WriteLine(Test());
                    System.Console.WriteLine("C#代码运算结束----------");
                }
            }
            while (true) ;
        }

        static float Add(float a, float b)
        {
            return a + b;
        }
        static public float Test()
        {
            long a = 300000L;
            float b = 5.32214f;
            int c = (int)a + (int)b;
            c = c - (int)12000.6111f;
            if (c > 300)
                c = 1;
            else
                c = 0;

            switch(c)
            {
                case 100:
                    System.Console.WriteLine("测试switch1111111 走进case " + c );
                    break;
                case -100:
                    System.Console.WriteLine("测试switch2222222 走进case " + c);
                    break;
            }

            c = c + 100 - 5 + (100 - 40);

            while(c < 600)
            {
                c += 50;
            }

            System.Console.WriteLine("最终结果" + c);

            Student student = new Student("张三");
            student.Name = "李四";
            student.Age = 18;
            student.Print();

            c += student.GetAge();
            c = TestWhile(c);
            System.Console.WriteLine("最终结果" + c);
            return c;
        }

        public static int TestWhile(int v)
        {
            v += 1;
            if(v < 10000)
            {
                return TestWhile(v);
            }
            else
            {
                return v;
            }
        }
    }

    public class Student
    {
        public string Name;
        public int Age;

        public Student(string name)
        {
            Name = name;
        }

        public void Print()
        {
            System.Console.WriteLine("学生:" + Name + " 年龄:" + Age);
        }

        public int GetAge()
        {
            return Age;
        }
    }
}

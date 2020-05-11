using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpVM
{
    class CVMStackFrame
    {
        const int MAX_STACK = 64;
        const int MAX_LOCAL_VAR = 64;   //最大局部变量

        //局部变量表
        public object[] LocalVariable = new object[MAX_LOCAL_VAR];
        private int stackSize = 0;
        //操作数栈
        private object[] operandStack = new object[MAX_STACK];
        //返回地址
        Instruction returnAddress;

        public void Push(object value)
        {
            System.Diagnostics.Debug.Assert(stackSize < MAX_STACK);
            operandStack[stackSize++] = value;
        }

        public object Pop()
        {
            System.Diagnostics.Debug.Assert(stackSize > 0);
            return operandStack[--stackSize];
        }
    }
}

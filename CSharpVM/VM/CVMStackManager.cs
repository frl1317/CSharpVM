using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpVM
{
    class CVMStackManager
    {
        const int MAX_STACK = 256;
        private CVMStackFrame[] stacks = new CVMStackFrame[MAX_STACK];
        private int stackTop = 0;

        public CVMStackFrame CreatStackFrame()
        {
            return new CVMStackFrame();
        }

        public void Push(CVMStackFrame stackFrame)
        {
            stacks[stackTop++] = stackFrame;
        }
        public CVMStackFrame Pop()
        {
            return stacks[--stackTop];
        }
    }
}

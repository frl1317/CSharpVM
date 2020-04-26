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
        //局部变量表
        List<object> localVariables = new List<object>();
        //返回地址
        Instruction returnAddress;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isol8_Compiler
{
    class WindowsNativeAssembly
    {
        void placeholder()
        {
            string output =
                "push rbp\n" +      //Preserve RBP
                "mov rbp, rsp\n" +  //Preserve RSP in RBP
                "sub rsp, 28h\n" +  //Add room on stack for printf param
                "and spl, 0F0h\n" + //Align rsp to 16-bytes using AND bitwise (last 4 bytes)
                "lea rcx, [LOCATION OF PRINT MESSAGE, MEMORY / VAR]\n" +
                "call printf\n" +   //Call printf
                "mov rsp, rbp\n" +  //Return RSP to original value
                "pop rbp\n";        //Return RBP to original value
        }
    }
}

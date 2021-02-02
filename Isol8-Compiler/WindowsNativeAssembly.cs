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
            //For me, prints hello + hello
            /* 
            EXTERN printf :PROC
            .DATA
                msg db "hello"
            .CODE
            Initial PROC
                push rbp
                mov rbp, rsp
    
                sub rsp, 36h
                ;and spl, 0F0h 

                lea rcx, [msg]
                lea rax, [rcx+5]
                mov rdx, [rcx]
                mov [rax], rdx

                call printf

                mov rsp, rbp
                pop rbp    
                ret
            Initial ENDP
            END 
            */

    }
}

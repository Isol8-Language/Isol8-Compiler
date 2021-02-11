using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isol8_Compiler
{
    class WindowsNativeAssembly
    {
        public static string CreatePrintFAssembly(string variableName)
        {


            return "\txor edx, edx\n" +
                    $"\tmov eax, [{variableName}]\n" + 
                    "\tmov ecx, 0xA\n" + 
                    "\tdiv ecx\n" + 
                    "\tadd eax, 0x30\n" + 
                    "\tadd eax, edx\n" + 
                    "\tmov ecx, eax\n" + 
                    "\tcall printf\n";


            /*return
                $"\tlea rcx, [{variableName}]\n" +
                "\tcall printf\n";*/
        }
  
    }
}

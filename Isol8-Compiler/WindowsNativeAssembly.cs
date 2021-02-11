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
                    "\tmov ecx, 0Ah\n" +
                    "\tdiv ecx\n" +
                    "\tadd eax, 30h\n" +
                    "\tadd eax, edx\n";


            /*return
                $"\tlea rcx, [{variableName}]\n" +
                "\tcall printf\n";*/
        }
  
    }
}

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
            return
                $"\tlea rcx, [{variableName}]\n" +
                "\tcall printf\n";
        }
  
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Isol8_Compiler.Enumerables;

namespace Isol8_Compiler
{
    class WindowsNativeAssembly
    {
        public static string CreatePrintFAssembly(string variableName)
        {

            // gonna replace this with list.find to get rid of the loop
            foreach (Variable x in Parser.variables)
            {
                if (x.name == variableName && x.type == Types.INT)
                {
                    return "\txor edx, edx\n" +
                            $"\tmov eax, [{variableName}]\n" +
                            "\tmov ecx, 0Ah\n" +
                            "\tdiv ecx\n" +
                            "\tadd eax, 30h\n" +
                            "\tadd eax, edx\n" +
                            $"\tmov r10, [{variableName}]\n"+
                            "\tmov[rsp], r10\n" +
                            $"\tmov[{variableName}], eax\n" +
                            $"\tlea rcx, [{variableName}]\n" +
                            "\tcall printf\n" + 
                            "\tmov r10, [rsp + 8]\n" +
                            $"\tmov[{variableName}], r10\n";
                }
                else if (x.name == variableName && x.type == Types.STRING)
                {
                    return
                        $"\tlea rcx, [{variableName}]\n" +
                        "\tcall printf\n";
                }
            }

            return "";

        }
  
    }
}

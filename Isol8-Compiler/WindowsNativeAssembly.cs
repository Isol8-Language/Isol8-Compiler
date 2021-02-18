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
            int i;
            for (i = 0; i < Parser.variables.Count-1; i++)
                if (variableName == Parser.variables[i].name)
                    break;

            if (Parser.variables[i].type == Types.INT)
            {
                return "\txor edx, edx\n" +
                            $"\tmov eax, [{variableName}]\n" +
                            $"\tmov ecx, 0Ah\n" +
                            $"\tdiv ecx\n" +
                            $"\tadd eax, 30h\n" +
                            $"\tadd eax, edx\n" +
                            $"\tmov edx, [{variableName}]\n" +
                            $"\tmov[rsp], r10\n" +
                            $"\tmov[{variableName}], eax\n" +
                            $"\tlea rcx, [{variableName}]\n" +
                            $"\tcall printf\n" +
                            $"\tmov edx, [rsp + 8]\n" +
                            $"\tmov[{variableName}], edx\n";
            }
            else if (Parser.variables[i].type == Types.BOOL)
            {
                return $"\tcmp {variableName}, 1\n" +
                             $"\tje {variableName}_1\n" +
                             $"\tjne {variableName}_2\n" +
                             $"\n" +
                             $"\t{variableName}_1:\n" +
                             $"\t\tlea rcx, [ISOL8_true_msg]\n" +
                             $"\t\tcall printf\n"+
                             $"\t\tjmp {variableName}_3\n" +
                             $"\t{variableName}_2:\n" +
                             $"\t\tlea rcx, [ISOL8_false_msg]\n" +
                             $"\t\tcall printf\n" +
                             $"\t\tjmp {variableName}_3\n" +
                             $"\t{variableName}_3:\n"+
                             $"\t\tnop\n";
            }
            else
                return
                $"\tlea rcx, [{variableName}]\n" +
                "\tcall printf\n";
  



        }
  
    }
}

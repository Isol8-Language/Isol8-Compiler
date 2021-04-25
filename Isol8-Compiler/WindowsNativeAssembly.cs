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
        private static int labelIndex = 0;
        public static string CreatePrintFAssembly(string variableName)
        {
            bool newline = false;
            string outString;
            if (variableName.Contains("\\n"))
            {
                variableName = variableName.Replace("\\n", "");
                newline = true;
            }

            int i;
            for (i = 0; i < Parser.variables.Count-1; i++)
                if (variableName == Parser.variables[i].name)
                    break;

            if (Parser.variables[i].type == Types.INT)
            {
                outString = 
                    $"\tmov edx, [{variableName}]\n" +
                    $"\tlea rcx, [PRINTF_DECIMAL_FLAG]\n" +
                    $"\tcall printf\n";
                    
            }
            else if (Parser.variables[i].type == Types.SHORT)
            {
                outString =
                    $"\tmov dx, [{variableName}]\n" +
                    $"\tlea rcx, [PRINTF_SHORT_FLAG]\n" +
                    $"\tcall printf\n";
            }
            else if (Parser.variables[i].type == Types.LONG)
            {
                outString =
                    $"\tmov rdx, [{variableName}]\n" +
                    $"\tlea rcx, [PRINTF_LONG_FLAG]\n" +
                    $"\tcall printf\n";
            }
            else if (Parser.variables[i].type == Types.BOOL)
            {
                string exitLabel = variableName + "_Exit_LI" + GenerateLabelIndex().ToString();
                string trueLabel = variableName + "_True_LI" + GenerateLabelIndex().ToString();
                string falseLabel = variableName + "_False_LI" + GenerateLabelIndex().ToString();
                outString =
                    $"\tcmp {variableName}, 1\n" +
                    $"\tje {trueLabel}\n" +
                    $"\tjne {falseLabel}\n" +
                    $"\t{trueLabel}:\n" +
                    $"\t\tlea rcx, [ISOL8_true_msg]\n" +
                    $"\t\tcall printf\n" +
                    $"\t\tjmp {exitLabel}\n" +
                    $"\t{falseLabel}:\n" +
                    $"\t\tlea rcx, [ISOL8_false_msg]\n" +
                    $"\t\tcall printf\n" +
                    //$"\t\tjmp {exitLabel}\n" +
                    $"\t{exitLabel}:\n";//+
                                        //$"\t\tnop\n";
            }
            else if (Parser.variables[i].type == Types.PTR)
            {
                outString =
                        $"\tmov rax, [{variableName}]\n" +
                        $"\tmov rbx, [rax]\n" +
                        $"\tmov r10, 40\n" +
                        $"\tmov [rsp+r10], rbx\n" +
                        $"\tmov eax, [rsp+r10]\n" +
                        $"\t_ascii_conv:\n" +
                        $"\t\tmov ecx, 10\n" +
                        $"\t\txor edx, edx\n" +
                        $"\t\tdiv ecx\n" +
                        $"\t\tadd edx, 30h\n" +
                        $"\t\tmov [rsp+r10], rdx\n" +
                        $"\t\tlea rcx, [rsp+r10]\n" +
                        $"\t\tmov rbx, rax\n" +
                        $"\t\tcall printf\n" +
                        $"\t\tmov rax, rbx\n" +
                        //$"\t\tinc r10\n" + 
                        $"\t\ttest eax, eax\n" +
                        $"\t\tjnz _ascii_conv\n";

                /*
                 *  move address into rax
                    move content into rbx
                    move 28h into r10
                    move rbx into stack+r10 offset		; moves original content into stack
                    move stack+r10 into eax
                    _ascii:
	                    move 10 into ecx
	                    empty edx
	                    divide eax by ecx, store in eax and reminader in edx
	                    add 30h to edx
	                    move edx into stack+r10		    ; store freshly converted char
	                    load effective address stack+r10 into rcx
	                    increment r10
	                    move rax into new stack+r10	    ; storing remaining calc before printf overwrites
	                    call printf
	                    move new stack+r10 into rax
	                    test if eax is zero (test eax, eax)
	                    jnz _ascii
                 */

            }
            else
            {
                outString =
                $"\tlea rcx, [{variableName}]\n" +
                "\tcall printf\n";

            }
            if (newline)
            {
                outString += "\tlea rcx, [NEW_LINE]\n";
                outString += "\tcall printf\n";
            }
            return outString;


        }

        public static string CreateScanFAssembly(string variableName)
        {

            int i;
            for (i = 0; i < Parser.variables.Count - 1; i++)
                if (variableName == Parser.variables[i].name)
                    break;

            if (Parser.variables[i].type == Types.INT)
            {
                return  $"\tlea rdx, [{variableName}]\n" + 
                        $"\tlea rcx, [PRINTF_DECIMAL_FLAG]\n" + 
                        $"\tcall scanf\n";
            } else if (Parser.variables[i].type == Types.STRING)
            {
                return  $"\tlea rdx, [{variableName}]\n" + 
                        $"\tlea rcx, [PRINTF_STRING_FLAG]\n" + 
                        $"\tcall scanf_s\n";
            } else
            {
                throw new NotImplementedException("Todo");
            }

        }

        public static int GenerateLabelIndex() => labelIndex++;
    }
}

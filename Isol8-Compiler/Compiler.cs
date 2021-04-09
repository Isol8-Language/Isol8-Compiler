//Remove this define before release
#define ASMComment
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Diagnostics;
using static Isol8_Compiler.Enumerables;
using static Isol8_Compiler.Enumerables.InstructionTypes;
using static Isol8_Compiler.Enumerables.ErrorCodes;
using static Isol8_Compiler.Parser;
using System.Linq;


namespace Isol8_Compiler
{
    class Compiler
    {
        private static string lastError = "NO_ERROR";
       
        private readonly string inputFileName;
        public readonly string outputName;
        private string register, countRegister;
        public static string GetLastError() => lastError;

        //Set the last error message and return the error code associated with it.
        internal static ErrorCodes SetLastError(int lineIndex, ErrorCodes errorCode, string lineContent)
        {
            lastError = $"Compiler Error: {errorCode} at line index: {lineIndex}. ({lineContent})";
            return errorCode;
        }

        public Compiler(string file, string outputFile)
        { 
            inputFileName = file;
            outputName = outputFile;
        }

        public ErrorCodes CreateAssemblyFile()
        {
            //Parse the code and validate
            ErrorCodes error = ParseFile(inputFileName); 
            if (error != NO_ERROR)
                return error;
        

            //Create the output file
            var outputFile = File.Create($"Output\\{outputName}.asm");
            
            //Add the .DATA section -- ToDo: remove hardcoded printf
            string output = "EXTERN printf :PROC\nEXTERN scanf :PROC\n.DATA\n";
            output += $"\tEXIT_LOOP_CODE DD 0\n"; 

            //For every declaration statement found in the parse.
            for (var i = 0; i < declarationStatements.Count; i++)
            {
                //Tab in and add the variable name,
                output += "\t" + declarationStatements[i].variableName + " ";
                //The type,
                switch(declarationStatements[i].type)
                {
                    case (Types.INT):
                        output += "DD " + declarationStatements[i].value + '\n';
                        break;
                    case (Types.PTR):
                        output += "DQ " + declarationStatements[i].value + '\n'; 
                        break;
                    case (Types.STRING):
                        output += "DB " + declarationStatements[i].value + ", 0" +/*", 10, 0"+*/ '\n';
                        break;
                    case (Types.BOOL):
                        output += "DB " + declarationStatements[i].value + '\n';
                        break;
                    case (Types.BYTE):
                        output += "DB " + declarationStatements[i].value + '\n';
                        break;
                    case (Types.INTARRAY):
                        output += $"DD {Convert.ToInt32(declarationStatements[i].value)} dup(0)\n";
                        break;
                    case (Types.SHORT):                    
                        output += $"DW {declarationStatements[i].value}\n";
                        break;
                    case (Types.LONG):
                        output += $"DQ {declarationStatements[i].value}\n";
                        break;
                }
            }

            //Adding the .CONST section
            // TODO: Is there a better way to print true/false for booleans
            //       without using constants?
            output += ".CONST\n";
            output += $"\tNEW_LINE DB 10, 0\n";
            output += $"\tISOL8_true_msg DB \"true\", 0\n";
            output += $"\tISOL8_false_msg DB \"false\", 0\n";
            output += $"\tEXIT_MESSAGE DB \"Press Enter To Exit...\",10,0\n";
            output += $"\tPRINTF_DECIMAL_FLAG DD \"d%\"\n";
            output += $"\tPRINTF_STRING_FLAG DD \"s%\"\n";
            output += $"\tPRINTF_SHORT_FLAG DD \"ih%\"\n";
            output += $"\tPRINTF_LONG_FLAG DD \"ill%\"\n";

            //Add the .CODE section
            output += ".CODE\n";    

            //For every function found in the parse.
            for (var i = 0; i < functions.Count; i++)
            {
#if (ASMComment)
                output += ";START FUNCTION PROLOGUE\n";
#endif
                output += Assembly.CreateFunctionEntry(functions[i].name);

#if (ASMComment)
                output += ";END FUNCTION PROLOGUE\n\n";
#endif

                //For every instruction of the function.
                for (int x = 0; x < functions[i].body.Count; x++)
                {
                    if (functions[i].body[x].instructionType == RET)
                    {
#if (ASMComment)
                        output += ";START FUNCTION EPILOGUE\n";
#endif

                        if (functions[i].body[x].lineContent.Length >= 2)
                            output += Assembly.CreateFunctionClose(functions[i].name, functions[i].body[x].lineContent[1]);
                        else
                            output += Assembly.CreateFunctionClose(functions[i].name);

#if (ASMComment)
                        output += ";END FUNCTION EPILOGUE\n";
#endif

                    }
                    else if (functions[i].body[x].instructionType == PLUSEQUALS)
                    {
#if (ASMComment)
                        output += ";START INC/ADD ROUTINE\n";
#endif
                        //If only adding 1, add some efficiency by using INC as opposed to ADD.
                        if (functions[i].body[x].lineContent[2] == "1")
                            output += $"\tinc " +
                            $"[{functions[i].body[x].lineContent[0]/*[1..]*/}]\n";

                        //If the right hand side of the operator is a variable.
                        else if (!int.TryParse(functions[i].body[x].lineContent[2], out int _))
                            output +=
                                $"\tmov eax, {functions[i].body[x].lineContent[2]}\n" +
                                $"\tadd [{functions[i].body[x].lineContent[0]}], eax\n";

                        else
                            output += $"\tadd " +
                            $"[{functions[i].body[x].lineContent[0]/*[1..]*/}], " +
                            $"{functions[i].body[x].lineContent[2]}\n";
#if (ASMComment)
                        output += ";END INC/ADD ROUTINE\n\n";
#endif
                    }
                    else if (functions[i].body[x].instructionType == ASSIGNPTR)
                    {
#if (ASMComment)
                        output += $";START ASSIGNPTR ROUTINE\n";
#endif
                        //ToDo: effiency? 
                        output += $"\tpush rax\n";
                        output += $"\tlea " +
                            $"rax, " +
                            $"[{functions[i].body[x].lineContent[4]}]\n";
                        output += $"\tmov {functions[i].body[x].lineContent[0][1..]}, rax\n";
                        output += $"\tpop rax\n";
#if (ASMComment)
                        output += $";END ASSIGNPTR ROUTINE\n\n";
#endif                    
                    }
                    else if (functions[i].body[x].instructionType == OUT)
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
#if (ASMComment)
                            output += $";START PRINTF ROUTINE\n";
#endif
                            output += WindowsNativeAssembly.CreatePrintFAssembly(functions[i].body[x].lineContent[1]);
#if (ASMComment)
                            output += $";END PRINTF ROUTINE\n\n";
#endif
                        }
                        else
                        {
                            //ToDo: Brandon
                            throw new Exception("ToDo: Linux Implementation");
                        }
                    }
                    else if (functions[i].body[x].instructionType == IN)
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            output += $";START SCANF ROUTINE\n";

                            output += WindowsNativeAssembly.CreateScanFAssembly(functions[i].body[x].lineContent[1]);

                            output += $";END SCANF ROUTINE\n\n";
                        }
                    }
                    else if (functions[i].body[x].instructionType == DELETE)
                    {
                        int varIndex = -1;
                        for (int vari = 0; vari < variables.Count; vari++)
                        {
                            if (variables[vari].name == functions[i].body[x].lineContent[1])
                                varIndex = vari;
                        }

                        if (variables[varIndex].type == Types.INT)
                        {
#if (ASMComment)
                            output += ";START DEL ROUTINE\n";
#endif

                            output += Assembly.Delete4ByteVariable(functions[i].body[x].lineContent[1]);
#if (ASMComment)
                            output += ";END DEL ROUTINE\n\n";
#endif
                        }
                        else
                            throw new NotImplementedException("ToDo");

                    }

                    else if (functions[i].body[x].instructionType == IF)
                    {
#if (ASMComment)
                        output += ";START IF ROUTINE\n";
#endif
                        string ifnotTrueLabel = "False_LI" + WindowsNativeAssembly.GenerateLabelIndex().ToString();

                        //if (functions[i].body[x].)

                        if (functions[i].body[x].assignmentType == Types.INT)
                            output += $"\tmov eax, [{functions[i].body[x].lineContent[1]}]\n";
                        else
                            output += $"\tmovzx rax, [{functions[i].body[x].lineContent[1]}]\n";

                        //If the condition is an int
                        if (int.TryParse(functions[i].body[x].lineContent[3], out int result))
                            output += $"\tcmp eax, {result}\n";
                        
                        //If the condition is a static true or false
                        else if (functions[i].body[x].lineContent[3].ToLower() == "true")
                            output += "\tcmp rax, 1\n";

                        else if (functions[i].body[x].lineContent[3].ToLower() == "false")
                            output += "\tcmp rax, 0\n";

                        output += $"\tjne {ifnotTrueLabel}\n";

                        for (int nextIf = x; nextIf < functions[i].body.Count; nextIf++)
                            if (functions[i].body[nextIf].instructionType == ENDIF && Convert.ToInt32(functions[i].body[nextIf].lineContent[2]) == x)
                            {
                                functions[i].body[nextIf].lineContent = new string[] { ifnotTrueLabel };
                                break;
                            }
                    }
                    else if (functions[i].body[x].instructionType == ENDIF)
                    {
                        output += $"\t{functions[i].body[x].lineContent[0]}:\n";
#if (ASMComment)
                        output += ";END IF ROUTINE\n\n";
#endif
                    }

                    else if (functions[i].body[x].instructionType == FOR)
                    {
#if (ASMComment)
                        output += ";START FOR ROUTINE\n";
#endif
                        string endLoopLabel = "End_Loop_LI" + WindowsNativeAssembly.GenerateLabelIndex();
                        string continueLoopLabel = "Continue_Loop_Label_LI" + WindowsNativeAssembly.GenerateLabelIndex(); ;


                        for (int xi = x; xi < functions[i].body.Count; xi++)
                        {
                            if (functions[i].body[xi].instructionType == ENDFOR)
                            {

                                endLoopLabel = functions[i].body[xi].lineContent[0];
                                continueLoopLabel = functions[i].body[xi].lineContent[1];
                                for (int xii = x; xii < functions[i].body.Count; xii++)
                                {
                                    if (functions[i].body[xii].instructionType == BREAK)
                                        functions[i].body[xii].lineContent[0] = endLoopLabel;

                                }
                                break;
                            }
                        }

                        register = "eax";
                        countRegister = "ecx";
                        output += $"\txor {countRegister}, {countRegister}\n";

                        if (functions[i].body[x].assignmentType !=  Types.NULL)
                        {
                            output += $"\tmov {register}, [{functions[i].body[x].lineContent[2]}]\n";
                        }
                        else
                        {
                            if (Convert.ToInt32(functions[i].body[x].lineContent[2]) >= int.MaxValue)
                                throw new NotImplementedException("64-bit loops not yet implemented");
                            output += $"\tmov {register}, {functions[i].body[x].lineContent[2]}\n";
                        }
                        output += $"\t{continueLoopLabel}:\n";
                        output += $"\tcmp {register}, {countRegister}\n";
                        output += $"\tje {endLoopLabel}\n";
                        output += $"\tmov [rsp+20h], {register}\n";
                        output += $"\tmov [rsp+24h], {countRegister}\n";

                    }
                    else if (functions[i].body[x].instructionType == ENDFOR)
                    {
                        output += $"\tmov {register}, [rsp+20h]\n";
                        output += $"\tmov {countRegister}, [rsp+24h]\n";
                        output += $"\tinc {countRegister}\n";
                        output += $"\tjmp {functions[i].body[x].lineContent[1]}\n";
                        output += $"\t{functions[i].body[x].lineContent[0]}:\n";
#if (ASMComment)
                        output += ";END FOR ROUTINE\n";
#endif

                    }
                    else if (functions[i].body[x].instructionType == BREAK)
                    {
#if (ASMComment)
                        output += ";BREAK\n";
#endif
                        output += $"\tjmp {functions[i].body[x].lineContent[0]}\n";
                    }

                    else if (functions[i].body[x].instructionType == ASSIGNMENT)
                    {
#if (ASMComment)
                        output += ";START REASSIGNMENT ROUTINE\n";
#endif
                        //Mov the address of the variable to rax
                        output += $"\tlea rax, [{functions[i].body[x].lineContent[0].Replace("\t", "")}]\n";

                        if (functions[i].body[x].assignmentType == Types.INT)
                        {

                            //If the source is an int.
                            if (int.TryParse(functions[i].body[x].lineContent[2], out _))
                                output += $"\tmov rcx, {functions[i].body[x].lineContent[2]}\n";

                            //Otherwise it's a variable.
                            else
                            {
                                output += $"\tlea rcx, [{functions[i].body[x].lineContent[2]}]\n";
                                output += $"\tmov ecx, [rcx]\n";
                            }
                            output += $"\tmov [rax], ecx\n";

                        }
                        //ToDo: If it's a string.
                        else if (functions[i].body[x].assignmentType == Types.BOOL)
                        {
                            if (functions[i].body[x].lineContent[2].ToUpper() == "FALSE")
                                output += $"\tmov [{functions[i].body[x].lineContent[0].Replace("\t", "")}], 0\n";

                            else if (functions[i].body[x].lineContent[2].ToUpper() == "TRUE")
                                output += $"\tmov [{functions[i].body[x].lineContent[0].Replace("\t", "")}], 1\n";
                            else //it's a variable
                                throw new NotImplementedException();
                        }

#if (ASMComment)
                        output += ";END REASSIGNMENT ROUTINE\n\n";
#endif
                    }

                    else if (functions[i].body[x].instructionType == FUNCASSIGNMENT)
                    {
                        int me = 5;
                    }

                    else if(functions[i].body[x].instructionType == PLUS)
                    {
#if (ASMComment)
                        output += ";START ADDITION\n";
#endif
                        // array content: [0] [1] [2] [3] [4]
                        //                 x   =   y   +   z
                        //                 i   +   j
                        // this is the same for all other operators

                        switch (functions[i].body[x].lineContent.Length)
                        {
                            case 3:
                                // TODO: check which operand is "1", so that 1 + x is a valid expression?
                                //       (regex would need to be updated in that case)
                                if (functions[i].body[x].lineContent[2] == "1")
                                {
                                    output +=   $"\tinc {functions[i].body[x].lineContent[0].Replace("\n", "")}\n";
                                } else
                                {
                                    output +=   $"\tmov eax, {functions[i].body[x].lineContent[2]}\n" +
                                                $"\tadd {functions[i].body[x].lineContent[0].Replace("\t", "")}, eax\n";
                                }
                                break;

                            case 5:
                                if (functions[i].body[x].lineContent[4] == "1")
                                {
                                    output +=   $"\tmov eax,{functions[i].body[x].lineContent[2]}\n" +
                                                $"\tinc eax\n" + 
                                                $"\tmov {functions[i].body[x].lineContent[0]}, eax\n";
                                } else
                                {
                                    output +=   $"\tmov eax,{functions[i].body[x].lineContent[2]}\n" +
                                                $"\tadd eax,{functions[i].body[x].lineContent[4]}\n" +
                                                $"\tmov {functions[i].body[x].lineContent[0].Replace("\t", "")},eax\n";
                                }
                                break;
                        }

#if (ASMComment)
                        output += ";END ADDITION\n\n";
#endif
                    }
                    else if(functions[i].body[x].instructionType == MINUS)
                    {
#if (ASMComment)
                        output += ";START SUBTRACTION\n";
#endif
                        switch (functions[i].body[x].lineContent.Length)
                        {
                            case 3:
                                if (functions[i].body[x].lineContent[2] == "1")
                                {
                                    output += $"\tdec {functions[i].body[x].lineContent[0].Replace("\t", "")}\n";
                                } else
                                {
                                    output +=   $"\tmov eax, {functions[i].body[x].lineContent[2]}\n" +
                                                $"\tsub {functions[i].body[x].lineContent[0].Replace("\t", "")}, eax\n";
                                }
                                break;
                            case 5:
                                if (functions[i].body[x].lineContent[4] == "1")
                                {
                                    output +=   $"\tmov eax, {functions[i].body[x].lineContent[2]}\n" +
                                                $"\tdec eax\n" +
                                                $"\tmov {functions[i].body[x].lineContent[0]}, eax\n";
                                } else
                                {
                                    output +=   $"\tmov eax,{functions[i].body[x].lineContent[2]}\n" +
                                                $"\tsub eax,{functions[i].body[x].lineContent[4]}\n" +
                                                $"\tmov {functions[i].body[x].lineContent[0].Replace("\t", "")},eax\n";
                                }
                                break;
                        }

#if (ASMComment)
                        output += ";END SUBTRACTION\n\n";
#endif
                    }
                    else if(functions[i].body[x].instructionType == MULTIPLY)
                    {
#if (ASMComment)
                        output += ";START MULTIPLICATION\n";
#endif
                        switch (functions[i].body[x].lineContent.Length)
                        {
                            case 3:
                                output +=   $"\tmov eax, {functions[i].body[x].lineContent[0].Replace("\t", "")}\n" +
                                            $"\tmul {functions[i].body[x].lineContent[2]}\n" +
                                            $"\tmov {functions[i].body[x].lineContent[0].Replace("\t", "")}, eax\n";
                                break;
                            case 5:
                                output +=   $"\tmov eax, {functions[i].body[x].lineContent[2]}\n"+
                                            $"\tmul {functions[i].body[x].lineContent[4]}\n" +
                                            $"\tmov {functions[i].body[x].lineContent[0].Replace("\t", "")}, eax\n";
                                break;
                        }
#if (ASMComment)
                        output += ";END MULTIPLICATION\n\n";
#endif
                    }
                    else if(functions[i].body[x].instructionType == DIVIDE)
                    {
#if (ASMComment)
                        output += ";START DIVISION\n";
#endif
                        // TODO: signed division

                        switch (functions[i].body[x].lineContent.Length)
                        {
                            case 3:
                                output += $"\tmov edx, 0\t;clear high dividend\n"+
                                            $"\tmov eax, {functions[i].body[x].lineContent[0].Replace("\t", "")}\n"+
                                            $"\tdiv {functions[i].body[x].lineContent[2]}\n" +
                                            $"\tmov {functions[i].body[x].lineContent[0].Replace("\t", "")}, eax\n";
                                break;
                            case 5:
                                output += $"\tmov edx, 0\t;clear high dividend\n" +
                                            $"\tmov eax, {functions[i].body[x].lineContent[2]}\n" +
                                            $"\tdiv {functions[i].body[x].lineContent[4]}\n" +
                                            $"\tmov {functions[i].body[x].lineContent[0].Replace("\t", "")}, eax\n";
                                break;
                        }

#if (ASMComment)
                        output += ";END DIVISION\n\n";
#endif
                    }
                    else if(functions[i].body[x].instructionType == GREATEREQUAL)
                    {
                        output += ";START GREATER THAN OR EQUAL TO\n";

                        string jgeLabel = $"_jge_{functions[i].body[x].lineContent[0]}_{WindowsNativeAssembly.GenerateLabelIndex()}";
                        string jngeLabel = $"_jnge_{functions[i].body[x].lineContent[0]}_{WindowsNativeAssembly.GenerateLabelIndex()}";
                        string exitLabel = $"_exit_{functions[i].body[x].lineContent[0]}_{WindowsNativeAssembly.GenerateLabelIndex()}";

                        output +=   $"\tmov eax, {functions[i].body[x].lineContent[2]}\n" +
                                    $"\tmov ebx, {functions[i].body[x].lineContent[4]}\n" +
                                    $"\tcmp eax, ebx\n" +
                                    $"\tjge {jgeLabel}\n" +
                                    $"\tjnge {jngeLabel}\n" +
                                    $"\t{jgeLabel}:\n" +
                                    $"\t\tmov {functions[i].body[x].lineContent[0]}, 1\n" +
                                    $"\t\tjmp {exitLabel}\n" +
                                    $"\t{jngeLabel}:\n" +
                                    $"\t\tmov {functions[i].body[x].lineContent[0]}, 0\n" +
                                    $"\t{exitLabel}:\n";

                        output += ";END GREATER THAN OR EQUAL TO\n\n";
                    }
                    else if(functions[i].body[x].instructionType == LESSEQUAL)
                    {
                        output += ";START LESS THAN OR EQUAL TO\n";

                        string jleLabel = $"_jle_{functions[i].body[x].lineContent[0]}_{WindowsNativeAssembly.GenerateLabelIndex()}";
                        string jnleLabel = $"_jnle_{functions[i].body[x].lineContent[0]}_{WindowsNativeAssembly.GenerateLabelIndex()}";
                        string exitLabel = $"_exit_{functions[i].body[x].lineContent[0]}_{WindowsNativeAssembly.GenerateLabelIndex()}";

                        output += $"\tmov eax, {functions[i].body[x].lineContent[2]}\n" +
                                    $"\tmov ebx, {functions[i].body[x].lineContent[4]}\n" +
                                    $"\tcmp eax, ebx\n" +
                                    $"\tjle {jleLabel}\n" +
                                    $"\tjnle {jnleLabel}\n" +
                                    $"\t{jleLabel}:\n" +
                                    $"\t\tmov {functions[i].body[x].lineContent[0]}, 1\n" +
                                    $"\t\tjmp {exitLabel}\n" +
                                    $"\t{jnleLabel}:\n" +
                                    $"\t\tmov {functions[i].body[x].lineContent[0]}, 0\n" +
                                    $"\t{exitLabel}:\n";


                        output += ";END START LESS THAN OR EQUAL TO\n\n";
                    } 
                    else if(functions[i].body[x].instructionType == GREATERTHAN)
                    {
                        output += ";START GREATER THAN\n";

                        string jgLabel = $"_jg_{functions[i].body[x].lineContent[0]}_{WindowsNativeAssembly.GenerateLabelIndex()}";
                        string jngLabel = $"_jng_{functions[i].body[x].lineContent[0]}_{WindowsNativeAssembly.GenerateLabelIndex()}";
                        string exitLabel = $"_exit_{functions[i].body[x].lineContent[0]}_{WindowsNativeAssembly.GenerateLabelIndex()}";

                        output += $"\tmov eax, {functions[i].body[x].lineContent[2]}\n" +
                                    $"\tmov ebx, {functions[i].body[x].lineContent[4]}\n" +
                                    $"\tcmp eax, ebx\n" +
                                    $"\tjg {jgLabel}\n" +
                                    $"\tjng {jngLabel}\n" +
                                    $"\t{jgLabel}:\n" +
                                    $"\t\tmov {functions[i].body[x].lineContent[0]}, 1\n" +
                                    $"\t\tjmp {exitLabel}\n" +
                                    $"\t{jngLabel}:\n" +
                                    $"\t\tmov {functions[i].body[x].lineContent[0]}, 0\n" +
                                    $"\t{exitLabel}:\n";

                        output += ";END GREATER THAN\n\n";
                    } 
                    else if(functions[i].body[x].instructionType == LESSTHAN)
                    {
                        output += ";START LESS THAN\n";

                        string jlLabel = $"_jl_{functions[i].body[x].lineContent[0]}_{WindowsNativeAssembly.GenerateLabelIndex()}";
                        string jnlLabel = $"_jnl_{functions[i].body[x].lineContent[0]}_{WindowsNativeAssembly.GenerateLabelIndex()}";
                        string exitLabel = $"_exit_{functions[i].body[x].lineContent[0]}_{WindowsNativeAssembly.GenerateLabelIndex()}";

                        output += $"\tmov eax, {functions[i].body[x].lineContent[2]}\n" +
                                    $"\tmov ebx, {functions[i].body[x].lineContent[4]}\n" +
                                    $"\tcmp eax, ebx\n" +
                                    $"\tjl {jlLabel}\n" +
                                    $"\tjnl {jnlLabel}\n" +
                                    $"\t{jlLabel}:\n" +
                                    $"\t\tmov {functions[i].body[x].lineContent[0]}, 1\n" +
                                    $"\t\tjmp {exitLabel}\n" +
                                    $"\t{jnlLabel}:\n" +
                                    $"\t\tmov {functions[i].body[x].lineContent[0]}, 0\n" +
                                    $"\t{exitLabel}:\n";

                        output += ";END LESS THAN\n\n";
                    }
                    else if (functions[i].body[x].instructionType == ISEQUAL)
                    {
                        output += ";START IS EQUAL CHECK\n";

                        string jeLabel = $"{functions[i].body[x].lineContent[0]}_je_{WindowsNativeAssembly.GenerateLabelIndex()}";
                        string jneLabel = $"{functions[i].body[x].lineContent[0]}_jne_{WindowsNativeAssembly.GenerateLabelIndex()}";
                        string exitLabel = $"{functions[i].body[x].lineContent[0]}_exit_{WindowsNativeAssembly.GenerateLabelIndex()}";

                        output += $"\tmov eax, {functions[i].body[x].lineContent[2]}\n" +
                                    $"\tmov ebx, {functions[i].body[x].lineContent[4]}\n" +
                                    $"\tcmp eax, ebx\n" +
                                    $"\tje {jeLabel}\n" +
                                    $"\tjne {jneLabel}\n" +
                                    $"\t{jeLabel}:\n" +
                                    $"\t\tmov {functions[i].body[x].lineContent[0]}, 1\n" +
                                    $"\t\tjmp {exitLabel}\n" +
                                    $"\t{jneLabel}:\n" +
                                    $"\t\tmov {functions[i].body[x].lineContent[0]}, 0\n" +
                                    $"\t{exitLabel}:\n";

                        output += ";END IS EQUAL CHECK\n\n";
                    }
                    else if (functions[i].body[x].instructionType == ISNOTEQUAL)
                    {
                        output += ";START IS NOT EQUAL CHECK\n";

                        string jneLabel = $"{functions[i].body[x].lineContent[0]}_jne_{WindowsNativeAssembly.GenerateLabelIndex()}";
                        string jeLabel = $"{functions[i].body[x].lineContent[0]}_je_{WindowsNativeAssembly.GenerateLabelIndex()}";
                        string exitLabel = $"{functions[i].body[x].lineContent[0]}_exit_{WindowsNativeAssembly.GenerateLabelIndex()}";

                        output += $"\tmov eax, {functions[i].body[x].lineContent[2]}\n" +
                                    $"\tmov ebx, {functions[i].body[x].lineContent[4]}\n" +
                                    $"\tcmp eax, ebx\n" +
                                    $"\tjne {jneLabel}\n" +
                                    $"\tje {jeLabel}\n" +
                                    $"\t{jneLabel}:\n" +
                                    $"\t\tmov {functions[i].body[x].lineContent[0]}, 1\n" +
                                    $"\t\tjmp {exitLabel}\n" +
                                    $"\t{jeLabel}:\n" +
                                    $"\t\tmov {functions[i].body[x].lineContent[0]}, 0\n" +
                                    $"\t{exitLabel}:\n";

                        output += ";END IS NOT EQUAL CHECK\n\n";
                    }
                    else
                        throw new Exception("This should never occur");
                }

#if (ASMComment)
                output += ";START FUNCTION EPILOGUE\n";
                output += Assembly.CreateInfiniteLoopPLACEHOLDER();
#endif
                output += Assembly.CreateFunctionClose(functions[i].name);
#if (ASMComment)
                output += ";END FUNCTION EPILOGUE\n\n";
#endif

            }

            //Add an END directive
            output += "END";
            outputFile.Write(new UTF8Encoding(true).GetBytes(output));

            outputFile.Close();
            return NO_ERROR;
        }
      
        public ErrorCodes Assemble(string fileName)
        {
            
            string links = "/defaultlib:";
            for (int i = 0; i < dependancies.Count; i++)
                links += $"\"{dependancies[i]}\"" + " ";
            

            Process ml64 = new Process()
            {
                
                StartInfo =
                {
                    Arguments = $"\"{Environment.CurrentDirectory}\\Output\\{fileName}.asm\" /Zi " +
                    $"/link " +
                    $"/subsystem:console " +
                    links + 
                    $"/entry:Initial " +
                    $"/out:\"{Directory.GetCurrentDirectory()}\\Output\\{outputName}.exe\" ",

                    FileName = Path.Combine(Directory.GetCurrentDirectory(), "ML64\\ml64.exe"),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
                
                
            };
            
            try
            {
                ml64.Start();
            }
            catch(Exception ex)
            {
                //Failed on ML64.exe launching
                SetLastError(-1, ML64_ERROR, ex.Message);
                return ML64_ERROR;
            }

            string mlResult = ml64.StandardOutput.ReadToEnd();

            //ML64.exe produced an error
            if (mlResult.Contains("error"))
            {
                SetLastError(-1, ML64_ERROR, mlResult);
                return ML64_ERROR;
            }
            return NO_ERROR;            
        }
    }
}

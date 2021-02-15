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
        public static string GetLastError() => lastError;

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
            string output = "EXTERN printf :PROC\n.DATA\n";
            
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
                        output += "DB " + declarationStatements[i].value + ", 10, 0" + '\n';
                        break;
                }

            }

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
                            $"[{functions[i].body[x].lineContent[0][1..]}]\n";
                        
                        //If the right hand side of the operator is a variable.
                        else if (!int.TryParse(functions[i].body[x].lineContent[2], out int _))
                        {
                            output += 
                                $"\tmov eax, {functions[i].body[x].lineContent[2]}\n" +
                                $"\tadd [{functions[i].body[x].lineContent[0][1..]}], eax\n";
                        }
                        else
                            output += $"\tadd " +
                            $"[{functions[i].body[x].lineContent[0][1..]}], " +
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
                        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
                }



            }

            //Add an END directive
            output += "END";
            outputFile.Write(new UTF8Encoding(true).GetBytes(output));

            outputFile.Close();
            return NO_ERROR;
        }
      
        public ErrorCodes Assemble(string fileName)
        {
            Process ml64 = new Process()
            {
                StartInfo =
                {
                    Arguments = $"\"{Environment.CurrentDirectory}\\Output\\{fileName}.asm\" /Zi /link /subsystem:console /defaultlib:\"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\VC\\Tools\\MSVC\\14.28.29333\\lib\\x64\\msvcrt.lib\" \"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\VC\\Tools\\MSVC\\14.28.29333\\lib\\x64\\legacy_stdio_definitions.lib\" \"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\VC\\Tools\\MSVC\\14.28.29333\\lib\\x64\\legacy_stdio_wide_specifiers.lib\" \"C:\\Program Files (x86)\\Windows Kits\\10\\Lib\\10.0.18362.0\\ucrt\\x64\\ucrt.lib\" /entry:Initial /out:\"{Directory.GetCurrentDirectory()}\\Output\\{outputName}.exe\"",
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

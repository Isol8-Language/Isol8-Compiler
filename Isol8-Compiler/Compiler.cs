using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using static Isol8_Compiler.Enumerables;
using static Isol8_Compiler.Enumerables.ErrorCodes;
using System.Linq;

namespace Isol8_Compiler
{
    class Compiler
    {
        private static string lastError = "NO_ERROR";

        //ToDo: Update this list to use the Variable class
        private readonly List<Variable> variables = new List<Variable>();
        private readonly List<Declaration> declarationStatements = new List<Declaration>();
        
        private readonly string inputFileName;
        public readonly string outputName;
        public static string GetLastError() => lastError;
        private static void SetLastError(int lineIndex, ErrorCodes errorCode, string lineContent)
        {
            lastError = $"Errorcode {errorCode} at line index: {lineIndex}. ({lineContent})";
        }

        public Compiler(string file, string outputFile)
        { 
            inputFileName = file;
            outputName = outputFile;
        }
        private ErrorCodes ParseFile()
        {
            var fileText = File.ReadLines(inputFileName).ToList();
            int lineIndex = 0;

            
            

            for(int i = 0; i < fileText.Count; i++)
            {
                //If a declaration pattern is found
                if ((_ = Patterns.createPattern.Match(fileText[i])) != Match.Empty)
                {
                    //Generate an array of values and a new declaration.

                    var values = fileText[i].Split(" ");

                    //Keyword does not need to be checked as regex will handle this
                    Declaration declaration = new Declaration()
                    { 
                        keyword = Enum.Parse<Keywords>(values[0]) 
                    };

                    if (!Patterns.lettersOnly.IsMatch(values[1]))
                    {
                        //Failure on variable name -- ToDo: SetLastError
                        return INVALID_VAR_NAME;
                    }
                    else
                    {
                        for (int x = 0; x < variables.Count; x++)
                        {
                            if (variables[x].name == values[1])
                                return INVALID_VAR_NAME;    //ToDo: SetLastError
                        }
                    }

                    declaration.variableName = values[1];


                    if (!Enum.TryParse(values[3], out declaration.type))
                    {
                        //Failure on type -- ToDo: SetLastError
                        return INVALID_TYPE;
                    };

                    //Revisit, inefficient as string usage
                    var trueValue = values[4].Replace(";", string.Empty);

                    if (declaration.type == Types.INT)
                    {
                        if (trueValue.Contains("0x"))
                            trueValue = Convert.ToInt32(trueValue, 16).ToString();

                        if (int.TryParse(trueValue, out _))
                            declaration.value = trueValue;
                        else
                        {
                            SetLastError(lineIndex, TYPE_MISMATCH, fileText[i]);
                            return TYPE_MISMATCH;
                        }
                    }
                    else if (declaration.type == Types.STRING)
                    {
                        //ToDo:
                        return default;
                    }
                    else
                    {
                        //Failure on type match (I.E, INT = "Hello" -- ToDo: SetLastError
                        return TYPE_MISMATCH;
                    }

                    //ToDo: update variables list to variable type
                    variables.Add(new Variable()
                    {
                        name = declaration.variableName,
                        //Add scope?
                        status = VarState.ACTIVE,
                        type = declaration.type,
                        value = declaration.value,
                    });
                    declarationStatements.Add(declaration);
                }
                else if ((_ = Patterns.functionPattern.Match(fileText[i])) != Match.Empty)
                {
                    //Get the values of the function declarations.
                    var values = fileText[i].Split(new char[] {' ','(', ')' });

                    if(!Enum.TryParse(values.Last(), out Types _))
                    {
                        //Fail on return type -- ToDo: SetLastError
                        return INVALID_RETURN_TYPE;
                    }

                    Function func = new Function()
                    {
                        name = values[1],
                        returnType = values.Last(),

                    };


                    if (fileText[i+1] == "{")
                    {

                    }
                    else
                    {
                        //next line is not {, fail
                    }

                    int me = 5;
                }
                else
                {
                    //No match for line, do what?
                }
                lineIndex++;
            }
            return NO_ERROR;
        }

        public ErrorCodes CreateAssemblyFile()
        {
            //Parse the code and validate
            ErrorCodes error = ParseFile(); 
            if (error != NO_ERROR)
                return error;

            //Create the output file
            var outputFile = File.Create($"Output\\{outputName}.asm");
            
            //Add the .DATA section
            string output = ".DATA\n";
            
            //For every declaration statement found in the parse
            for (var i = 0; i < declarationStatements.Count; i++)
            {
                //Tab in and add the variable name,
                output += "\t" + declarationStatements[i].variableName + " ";
                //The type,
                switch(declarationStatements[i].type)
                {
                    case (Types.INT):
                        output += "DD ";
                        break;
                }
                //And the value.
                output += declarationStatements[i].value + '\n';
            }
 
            //Add the .CODE section with a PLACEHOLDER entry point (***change this further down the line)
            output += 
                ".CODE\n" +
                "dummyEntry PROC\n"+
                "\tret\n" +
                "dummyEntry ENDP\n";

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
                //PLACEHOLDER ENTRY POINT NAME -- ToDo: Fix this

                StartInfo =
                {
                    Arguments = $"\"{Environment.CurrentDirectory}\\Output\\{fileName}.asm\" /Zi /link /subsystem:windows /entry:dummyEntry /out:\"{Directory.GetCurrentDirectory()}\\Output\\{outputName}.exe\"",
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
            catch
            {
                //ToDo: Error Handling -- ToDo: set last error
                return ML64_ERROR;
            }

            string mlResult = ml64.StandardOutput.ReadToEnd();
            if (mlResult.Contains("error"))
            {
                SetLastError(-1, ML64_ERROR, mlResult);
                return ML64_ERROR;
            }
            return NO_ERROR;            
        }
    }
}

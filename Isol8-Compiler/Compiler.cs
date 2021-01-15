using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using static Isol8_Compiler.Enumerables;
using static Isol8_Compiler.Enumerables.ErrorCodes;
namespace Isol8_Compiler
{
    class Compiler
    {
        private static string lastError;
        private readonly List<string> variables = new List<string>();
        private readonly List<Declaration> declarationStatements = new List<Declaration>();
        private readonly string fileName;
        public static string GetLastError() => lastError;
        private static void SetLastError(int lineIndex, ErrorCodes errorCode, string lineContent)
        {
            lastError = $"Errorcode {errorCode} at line index: {lineIndex}. ({lineContent})";
        }

        public Compiler(string file)
        {
            //Do we need a constructor?
            fileName = file;
        }
        private ErrorCodes ParseFile()
        {
            Match syntaxMatch;
            var fileText = File.ReadLines(fileName);
            int lineIndex = 0;

            foreach (var line in fileText)
            {
                //If a declaration pattern is found
                if ((syntaxMatch = Patterns.createPattern.Match(line)) != Match.Empty)
                {
                    //Generate an array of values and a new declaration.
                    Declaration declaration = new Declaration();
                    var values = line.Split(" ");

                    //Keyword does not need to be checked as regex will handle this
                    declaration.keyword = Enum.Parse<Keywords>(values[0]);

                    if (!Patterns.lettersOnly.IsMatch(values[1]) || variables.Contains(values[1]))
                    {
                        //Failure on variable name
                        return INVALID_VAR_NAME;
                    }

                    declaration.variableName = values[1];

                    if (!Enum.TryParse(values[3], out declaration.type))
                    {
                        //Failure on type
                        return INVALID_TYPE;
                    };

                    //Revisit, inefficient as string usage
                    var trueValue = values[4].Replace(";", string.Empty);

                    if (declaration.type == Types.INT)
                    {
                        if (int.TryParse(trueValue, out _))
                            declaration.value = trueValue;
                        else
                        {
                            SetLastError(lineIndex, ErrorCodes.TYPE_MISMATCH, line);
                            return TYPE_MISMATCH;
                        }
                    }
                    else if (declaration.type == Types.STRING)
                    {
                        return default;
                    }
                    else
                    {
                        //Failure on type match (I.E, INT = "Hello"
                        return TYPE_MISMATCH;
                    }

                    variables.Add(declaration.variableName);
                    declarationStatements.Add(declaration);


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
            ErrorCodes error = ParseFile(); 
            if (error != NO_ERROR)
                return error;

            //Create the output file
            var outputFile = File.Create("Output.txt");
            
            //Add the .DATA section
            string output = ".DATA\n";
            
            //For every declaration statement found in the parse
            for (var i = 0; i < declarationStatements.Count; i++)
            {
                output += "    " + declarationStatements[i].variableName + " ";
                switch(declarationStatements[i].type)
                {
                    case (Types.INT):
                        output += "DD ";
                        break;
                }
                output += declarationStatements[i].value + '\n';
            }
 
            //Add the .CODE section with a PLACEHOLDER entry point (***change this further down the line)
            output += 
                ".CODE\n" +
                "dummyEntry PROC\n"+
                "\tret\n" +
                "dummyEntry ENDP\n";



            //ADd an END directive
            output += "END";
            outputFile.Write(new UTF8Encoding(true).GetBytes(output));



            outputFile.Close();
            return NO_ERROR;
        }
        
        public void Assemble()
        {
            Process ml64 = new Process()
            {
                //PLACEHOLDER ENTRY POINT NAME

                StartInfo =
                {
                    Arguments = $"\"Output.txt\" /Zi /link /subsystem:windows /entry:dummyEntry /out:\"{Directory.GetCurrentDirectory()}\\Output\\Output.exe\"",
                    FileName = Path.Combine(Directory.GetCurrentDirectory(), "ML64\\ml64.exe"),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
                
            };
            ml64.Start();
            string temp = ml64.StandardOutput.ReadToEnd();
           
        }
        private class Declaration
        {
            public Keywords keyword;
            public string variableName;
            public Types type;
            public string value;
        }
    }
}

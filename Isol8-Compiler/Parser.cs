using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static Isol8_Compiler.Enumerables;
using static Isol8_Compiler.Enumerables.ErrorCodes;
using static Isol8_Compiler.Enumerables.InstructionTypes;
using static Isol8_Compiler.Compiler;
using System.Text.RegularExpressions;
namespace Isol8_Compiler
{
    public static class Parser
    {
        internal static readonly List<Variable> variables = new List<Variable>();
        internal static readonly List<Declaration> declarationStatements = new List<Declaration>();
        internal static readonly List<Function> functions = new List<Function>();

        internal static ErrorCodes ParseFile(string inputFileName)
        {
            #region localFunctions
            static ErrorCodes ParseDeclaration(string[] values, string lineContent, int lineIndex, bool local = false)
            {
                //Keyword does not need to be checked as regex will handle this.
                Declaration declaration = new Declaration()
                {
                    keyword = Enum.Parse<Keywords>(values[0])
                };

                //Check variable name is only letters.
                if (!Patterns.lettersOnly.IsMatch(values[1]))
                    return SetLastError(lineIndex, INVALID_VAR_NAME, lineContent);


                //Check the variable name is not already in use
                for (int x = 0; x < variables.Count; x++)
                    if (variables[x].name == values[1])
                        return SetLastError(lineIndex, DUPLICATE_VAR_NAME, lineContent);


                declaration.variableName = values[1];


                if (!Enum.TryParse(values[3], out declaration.type))
                    return SetLastError(lineIndex, INVALID_TYPE, lineContent);


                //ToDo: Revisit, inefficient as string usage
                var trueValue = values[4].Replace(";", string.Empty);

                if (declaration.type == Types.INT)
                {
                    //If value declared as hex, remove 0x notation. //ToDo: Add try catch for 0xSTRING
                    if (trueValue.Contains("0x"))
                    {
                        if (Patterns.standardOrHexDigitsOnly.Match(trueValue) != Match.Empty)
                            trueValue = Convert.ToInt32(trueValue, 16).ToString();
                        else
                            return SetLastError(lineIndex, INVALID_VAR_VALUE, lineContent);

                    }

                    //Ensure the assigned value is actually an INT
                    if (int.TryParse(trueValue, out _))
                        declaration.value = trueValue;
                    else
                        return SetLastError(lineIndex, TYPE_MISMATCH, lineContent);
                }
                else if (declaration.type == Types.STRING)
                {
                    //ToDo:
                    return default;
                }
                //Create a new variables.
                variables.Add(new Variable()
                {
                    name = declaration.variableName,
                    scope = local ? Scope.LOCAL : Scope.GLOBAL,
                    status = VarState.ACTIVE,
                    type = declaration.type,
                    value = declaration.value,
                });
                declarationStatements.Add(declaration);
                return default;
            }
            #endregion

            var fileText = File.ReadLines(inputFileName).ToList();

            for (int i = 0; i < fileText.Count; i++)
            {
                #region Declarations
                //If a declaration pattern is found
                if ((Patterns.createPattern.Match(fileText[i])) != Match.Empty)
                {
                    //Generate an array of values.
                    var values = fileText[i].Split(" ");

                    ErrorCodes errorCode = ParseDeclaration(values, fileText[i], i);
                    if (errorCode != NO_ERROR)
                        return errorCode;
                }
                #endregion
                #region Functions
                else if (Patterns.functionPattern.Match(fileText[i]) != Match.Empty)
                {
                    //Get the values of the function declarations.
                    var values = fileText[i].Split(new char[] { ' ', '(', ')' });

                    if (!Enum.TryParse(values.Last(), out Types _))
                        return SetLastError(i, INVALID_RETURN_TYPE, fileText[i]);

                    Function func = new Function()
                    {
                        name = values[1],
                        returnType = values.Last(),

                    };

                    //Check the function open and closes with the correct brackers
                    if (fileText[i + 1] == "{")
                    {
                        bool closeFunction = false;

                        //For each line after the initial {
                        for (int initialIndex = i + 2; initialIndex < fileText.Count; initialIndex++)
                        {
                            //If end of function
                            if (fileText[initialIndex] == "}")
                            {
                                closeFunction = true;
                                break;
                            }
                            else
                            {
                                //Remove tabs
                                Instruction instruction = new Instruction
                                {
                                    lineContent = fileText[initialIndex].Replace(";","").Split(' '),
                                };

                                //If ret type
                                if (Patterns.retPattern.Match(fileText[initialIndex].Replace("\t", "")) != Match.Empty)
                                    instruction.instructionType = RET;

                                func.body.Add(instruction);
                            }
                                
                        }
                        if (!closeFunction)
                            return SetLastError(i, NO_CLOSING_BRACKET, fileText[i]);

                        functions.Add(func);
                    }
                    else
                        return SetLastError(i, NO_OPENING_BRACKET, fileText[i]);

                    

                }
                #endregion
                else
                {
                    //No match for line, do what?
                }
            }
            return NO_ERROR;
        }

    }
}

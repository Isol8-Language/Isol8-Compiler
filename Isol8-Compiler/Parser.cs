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
            static ErrorCodes ParseGenerics(string line, ref Instruction instruction)
            {
                if (Patterns.deletePattern.Match(line) != Match.Empty)
                    return ParseDelete(ref instruction);

                else if (Patterns.simpleSelfAdditionOperator.Match(line) != Match.Empty)
                    return ParseSelfAddition(ref instruction);

                else if (Patterns.outPattern.Match(line) != Match.Empty)
                    return ParseOut(ref instruction);

                else if (Patterns.createPattern.Match(line) != Match.Empty)
                    throw new NotImplementedException("ToDo: Local Variables");

                else if (Patterns.simpleMathsOperator.Match(line) != Match.Empty)
                    return ParseSimpleMathsOp(ref instruction);

                else if (Patterns.ptrPattern.Match(line) != Match.Empty)
                    return ParsePtr(ref instruction);

                return NO_PATTERN_MATCH; 
            }

            static ErrorCodes ParseDeclaration(string[] values, string lineContent, int lineIndex, bool local = false)
            {
                //Keyword does not need to be checked as regex will handle this. ToDo: error handling
                Declaration declaration = new Declaration()
                {
                    keyword = Enum.Parse<Keywords>(values[0], true)
                };

                //Check variable name is only letters.
                if (!Patterns.lettersOnly.IsMatch(values[1]))
                    return SetLastError(lineIndex, INVALID_VAR_NAME, lineContent);


                //Check the variable name is not already in use.
                for (int x = 0; x < variables.Count; x++)
                    if (variables[x].name == values[1])
                        return SetLastError(lineIndex, DUPLICATE_VAR_NAME, lineContent);

                declaration.variableName = values[1];

                if (!Enum.TryParse(values[3], true, out declaration.type))
                    return SetLastError(lineIndex, INVALID_TYPE, lineContent);


                //ToDo: Revisit, inefficient as string usage
                var trueValue = values[4].Replace(";", string.Empty);

                if (declaration.type == Types.INT || declaration.type == Types.PTR)
                {
                    //If value declared as hex, remove 0x notation. 
                    if (trueValue.Contains("0x"))
                    {
                        //If the function contains standard numbers or hex digits only (A-F)
                        if (Patterns.standardOrHexDigitsOnly.Match(trueValue) != Match.Empty)
                            trueValue = Convert.ToInt32(trueValue, 16).ToString();
                        else
                            return SetLastError(lineIndex, INVALID_VAR_VALUE, lineContent);

                    }

                    //Ensure the assigned value is actually an INT when the declare type is INT
                    if (trueValue.ToUpper() == "NULL")
                        declaration.value = "0";

                    else if (int.TryParse(trueValue, out _))
                        declaration.value = trueValue;
                    else
                        return SetLastError(lineIndex, TYPE_MISMATCH, lineContent);
                } else if (declaration.type == Types.BOOL)
                {

                    if (trueValue.ToUpper() == "FALSE")
                    {
                        declaration.value = "0";
                    } else if (trueValue.ToUpper() == "TRUE")
                    {
                        declaration.value = "1";
                    } else
                    {
                        return SetLastError(lineIndex, TYPE_MISMATCH, lineContent);
                    }

                }
                else if (declaration.type == Types.STRING)
                {
                    trueValue = null;
                    for (int i = 4; i < values.Length; i++)
                        trueValue += values[i] + " ";

                    //Remove the last ; and space
                    trueValue = trueValue.Remove(trueValue.Length - 2);

                    //Ensure the assigned value is actually a String
                    if (Patterns.stringPattern.Match(trueValue) != Match.Empty)
                        declaration.value = trueValue;
                    else
                        return SetLastError(lineIndex, TYPE_MISMATCH, lineContent);
                }

                //Create a new variable and add it to the existing variables list.
                variables.Add(new Variable()
                {
                    name = declaration.variableName,
                    scope = local ? Scope.LOCAL : Scope.GLOBAL,
                    status = VarState.ACTIVE,
                    type = declaration.type,
                    value = declaration.value,
                });

                //Add the declaration to the declaration list.
                declarationStatements.Add(declaration);
                return NO_ERROR;
            }
            static ErrorCodes ParseDelete(ref Instruction instruction)
            {
                if (!CheckVarState(instruction.lineContent[1].Replace("\t", ""), out int varIndex))
                    throw new NotImplementedException("ToDo"); //ToDo: fail on non-existant variable OR inactive variable.

                variables[varIndex].status = VarState.DELETED;
                instruction.instructionType = DELETE;

                return NO_ERROR;
            }
            static ErrorCodes ParseSelfAddition(ref Instruction instruction)
            {
                if (!CheckVarState(instruction.lineContent[0].Replace("\t", ""), out _))
                    throw new NotImplementedException("ToDo"); //ToDo: fail on non-existant variable OR inactive variable.

                //If the input value is NOT a number, then it's a variable
                else if (!int.TryParse(instruction.lineContent[2], out int result))
                    if (!CheckVarState(instruction.lineContent[2].Replace("\t", ""), out _))
                        throw new NotImplementedException("ToDo"); //ToDo: fail on non-existant variable OR inactive variable.

                instruction.instructionType = PLUSEQUALS;

                return NO_ERROR;
            }
            static ErrorCodes ParseOut(ref Instruction instruction)
            {
                //ToDo: parse variable, check it's active
                instruction.instructionType = OUT;
                return NO_ERROR;
            }
            static ErrorCodes ParseSimpleMathsOp(ref Instruction instruction)
            {
                if (!CheckVarState(instruction.lineContent[0].Replace("\t", ""), out _))
                    throw new NotImplementedException("ToDo"); //ToDo: fail on non-existant variable OR inactive variable.

                throw new NotImplementedException("ToDo");
            };
            static ErrorCodes ParsePtr(ref Instruction instruction)
            {
                if (!CheckVarState(instruction.lineContent[0].Replace("\t", ""), out _))
                    throw new NotImplementedException("ToDo"); //ToDo: fail on non-existant variable OR inactive variable.
                else
                    instruction.instructionType = ASSIGNPTR;

                return NO_ERROR;
            };

            static bool CheckVarState(string varName, out int varIndex)
            {
                varIndex = -1;

                //Check the variable exists
                if (!variables.Any(v => v.name == varName))
                    return false;


                //Ensure the variable we're trying to modify exists and is active
                for (int x = 0; x < variables.Count; x++)
                    if (variables[x].name == varName && variables[x].status == VarState.ACTIVE)
                    {
                        varIndex = x;
                        return true;
                    }

                return false;
            }
            #endregion

            var fileText = File.ReadLines(inputFileName).ToList();

            for (int i = 0; i < fileText.Count; i++)
            {
                /*Loop will look for global declarations or functions. 
                 * There is nothing else to check for, as other instructions must be made INSIDE of functions.*/

                //Ignore comments - ToDo: pass to assembly file, comments start with ; in MASM?
                if (fileText[i].Length >= 2 && fileText[i][0..2] == "��")
                {
                    //ToDo: remove from fileText and fix i index
                    continue;
                }

                #region Declarations
                //If a declaration pattern is found
                if (Patterns.createPattern.Match(fileText[i]) != Match.Empty)
                {
                    //Generate an array of values.
                    var values = fileText[i].Split(" ");

                    ErrorCodes errorCode = ParseDeclaration(values, fileText[i], i);
                    if (errorCode != NO_ERROR)
                        return errorCode;
                }
                #endregion

                #region Functions
                //If a funtion pattern is found
                else if (Patterns.functionPattern.Match(fileText[i]) != Match.Empty)
                {
                    //Get the values of the function declarations.
                    var values = fileText[i].Split(new char[] { ' ', '(', ')' });

                    //Initialize a new function
                    Function func = new Function()
                    {
                        name = values[0],
                    };

                    //Check the return type is a valid type
                    if (!Enum.TryParse(values.Last(), true, out func.returnType))
                        return SetLastError(i, INVALID_RETURN_TYPE, fileText[i]);

                    //Check the function open and closes with the correct brackers, and grab the body.
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

                            Instruction instruction = new Instruction
                            {
                                lineContent = fileText[initialIndex].Replace(";", "").Split(new char[] { ' ', '(', ')' }),
                            };
                            string patternText = fileText[initialIndex].Replace("\t", "");

                            //If return instruction
                            if (Patterns.retPattern.Match(patternText) != Match.Empty)
                            {
                                instruction.instructionType = RET;

                                //If function return type is an int
                                if (func.returnType == Types.INT && instruction.lineContent.Length >= 2)
                                {
                                    //ToDo: if returning variable type, perform check on variable.

                                    //If hex declaration
                                    if (instruction.lineContent[1].Contains("0x"))
                                    {
                                        //Check the conversion is valid
                                        try
                                        {
                                            Convert.ToUInt32(instruction.lineContent[1], 16);
                                        }
                                        catch
                                        {
                                            return SetLastError(i, INVALID_RETURN_TYPE, fileText[i]);
                                        }

                                        //If the first letter is a letter, then add a 0 as it's required
                                        if (Patterns.lettersOnly.Match(instruction.lineContent[1][2..][0].ToString()) != Match.Empty)
                                            instruction.lineContent[1] = '0' + instruction.lineContent[1][2..] + 'h';

                                        //Otherwise just cut the 0x off and append a h
                                        else
                                            instruction.lineContent[1] = instruction.lineContent[1][2..] + 'h';

                                    }

                                    //If the return value is a variable, the same type of return, and active.
                                    else if (variables.Any(v => v.name == instruction.lineContent[1] && v.type == Types.INT && v.status == VarState.ACTIVE))
                                    {

                                        //ToDo: if no longer is here, can just merge with above if?
                                    }
                                    //Else just check the int is valid
                                    else if (!int.TryParse(instruction.lineContent[1], out _))
                                        return SetLastError(i, INVALID_RETURN_TYPE, fileText[i]);
                                }
                                else
                                {
                                    //ToDo: string
                                }

                            }

                            //If if statement
                            else if (Patterns.ifPattern.Match(patternText) != Match.Empty)
                            {
                                if (fileText[i + 1] == "{")
                                {
                                    
                                    bool closeIf = false;
                                    for (int ifIndex = i + 2; ifIndex < fileText.Count; ifIndex++)
                                    {


                                        if (fileText[ifIndex] == "}")
                                        {
                                            closeIf = true;
                                            break;
                                        }

                                        //ToDo: Turn Previous Pattern Matches Into Function Before Completing this

                                    }



                                    if (!closeIf) //ToDo: OR RET
                                        return SetLastError(i, NO_CLOSING_BRACKET, fileText[i]);
                                }
                                else
                                    return SetLastError(i, NO_OPENING_BRACKET, fileText[i]);



                            }

                            //If generic
                            else if (ParseGenerics(patternText, ref instruction) != NO_ERROR)
                                throw new NotImplementedException("ToDo"); //ToDo - if no pattern found then what?
                            

                            func.body.Add(instruction);
                        }

                        //If no closing brack located.
                        if (!closeFunction) //ToDo: OR RET
                            return SetLastError(i, NO_CLOSING_BRACKET, fileText[i]);

                        //Check the function name is not already in use.
                        for (int x = 0; x < functions.Count; x++)
                            if (functions[x].name == func.name)
                                return SetLastError(i, DUPLICATE_FUNC_NAME, fileText[i]);

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

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
            //Local functions for use in parsing.
            #region localFunctions
            static ErrorCodes ParseGenerics(string line, ref Instruction instruction, ref Function func, List<string> fileText, ref int i)
            {
                if (Patterns.deletePattern.Match(line) != Match.Empty)
                    return ParseDelete(ref instruction, ref func);

                else if (Patterns.simpleSelfAdditionOperator.Match(line) != Match.Empty)
                    return ParseSelfAddition(ref instruction, ref func);

                else if (Patterns.outPattern.Match(line) != Match.Empty)
                    return ParseOut(ref instruction, ref func);

                else if (Patterns.createPattern.Match(line) != Match.Empty)
                    throw new NotImplementedException("ToDo: Local Variables");

                else if (Patterns.simpleMathsOperator.Match(line) != Match.Empty)
                    return ParseSimpleMathsOp(ref instruction, ref func);

                else if (Patterns.ptrPattern.Match(line) != Match.Empty)
                    return ParsePtr(ref instruction, ref func);

                else if (Patterns.assignPattern.Match(line) != Match.Empty)
                    return ParseAssignment(ref instruction, ref func);

                else if (Patterns.ifPattern.Match(line) != Match.Empty)
                    return ParseSubLoop(IF, ref func, ref instruction, fileText, ref i);

                else if (Patterns.forPattern.Match(line) != Match.Empty)
                    return ParseSubLoop(FOR, ref func, ref instruction, fileText, ref i);

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

                //Set the variable name, assuming it's not in use.
                declaration.variableName = values[1];

                if (!Enum.TryParse(values[3], true, out declaration.type))
                    return SetLastError(lineIndex, INVALID_TYPE, lineContent);


                //ToDo: Revisit, inefficient as string usage
                var trueValue = values[4].Replace(";", string.Empty);

                //If the type of declaration is an INT or a pointer
                if (declaration.type == Types.INT || declaration.type == Types.PTR)
                {
                    //If value declared as hex, remove 0x notation for assembly conversion to Xh. 
                    if (trueValue.Contains("0x"))
                    {
                        /*If the function contains standard numbers or hex digits only (A-F),
                         then convert it to a string integer in base-16 (Hex).*/
                        if (Patterns.standardOrHexDigitsOnly.Match(trueValue) != Match.Empty)
                            trueValue = Convert.ToInt32(trueValue, 16).ToString();
                        else
                            return SetLastError(lineIndex, INVALID_VAR_VALUE, lineContent);

                    }

                    //If the string is NULL then it's essentially 0 in assembly.
                    if (trueValue.ToUpper() == "NULL")
                        declaration.value = "0";

                    //Ensure the assignment is of the same type, I.E int = int, and not int = string etc.
                    else if (int.TryParse(trueValue, out _))
                        declaration.value = trueValue;
                    else
                        return SetLastError(lineIndex, TYPE_MISMATCH, lineContent);
                } 
                else if (declaration.type == Types.BOOL)
                {
                    //IF the value is FALSE or less than, or equal, to 0, then set the value to "0".
                    if (trueValue.ToUpper() == "FALSE" || (int.TryParse(trueValue, out int falseInt) && falseInt <= 0))
                        declaration.value = "0";

                    else if (trueValue.ToUpper() == "TRUE" || (int.TryParse(trueValue, out int trueInt) && trueInt >= 1))
                        declaration.value = "1";

                    else
                        return SetLastError(lineIndex, TYPE_MISMATCH, lineContent);

                }
                else if (declaration.type == Types.STRING)
                {
                    trueValue = null;
                    /*Index 4 will always be the start of the string value in the pattern. ToDo: declare as const?
                    For every value after 4, append it to the trueValue string.*/
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
                    //If the scope is local bool (true) then set scope to local, if not then global.
                    scope = local ? Scope.LOCAL : Scope.GLOBAL,
                    status = VarState.ACTIVE,
                    type = declaration.type,
                    value = declaration.value,
                });

                //Add the declaration to the declaration list.
                declarationStatements.Add(declaration);
                return NO_ERROR;
            }
            static ErrorCodes ParseDelete(ref Instruction instruction, ref Function func)
            {
                if (!CheckVarState(instruction.lineContent[1].Replace("\t", ""), out int varIndex))
                    throw new NotImplementedException("ToDo"); //ToDo: fail on non-existant variable OR inactive variable.

                variables[varIndex].status = VarState.DELETED;
                instruction.instructionType = DELETE;

                func.body.Add(instruction);
                return NO_ERROR;
            }
            static ErrorCodes ParseSubLoop(InstructionTypes type, ref Function func, ref Instruction instruction, List<string> fileText, ref int i)
            {
                instruction.instructionType = type;
                func.body.Add(instruction);

                if (fileText[i + 1].Replace("\t", "") == "{")
                {

                    bool closeLoop = false;
                    //For every line within the sub statement
                    for (i += 2; i < fileText.Count; i++)
                    {
                        //If the if statement is closing
                        if (fileText[i].Replace("\t", "") == "}")
                        {
                            closeLoop = true;
                            func.body.Add(new Instruction()
                            {
                                instructionType = type == IF ? ENDIF : ENDFOR,
                                lineContent = new string[] 
                                { 
                                    type == IF ? null : "End_Loop_LI" + WindowsNativeAssembly.GenerateLabelIndex().ToString(), 
                                    type == IF ? null : "Continue_Loop_LI" + WindowsNativeAssembly.GenerateLabelIndex().ToString()
                                },
                            });
                            break;
                        }

                        Instruction innerInstruction = new Instruction()
                        {
                            lineContent = fileText[i].Replace(";", "").Split(new char[] { ' ', '(', ')' }),
                        };

                        ErrorCodes errorCodes = ParseGenerics(fileText[i].Replace("\t", ""), ref innerInstruction, ref func, fileText, ref i);
                        if (errorCodes != NO_ERROR)
                            throw new NotImplementedException("ToDo");

                        //Add the inner instruction to the function body.
                        //func.body.Add(innerInstruction);
                    }

                    if (!closeLoop)
                        return SetLastError(i, NO_CLOSING_BRACKET, fileText[i]);
                    else
                        return NO_ERROR;
                }
                else
                    return SetLastError(i, NO_OPENING_BRACKET, fileText[i]);

            }
            static ErrorCodes ParseSelfAddition(ref Instruction instruction, ref Function func)
            {
                if (!CheckVarState(instruction.lineContent[0].Replace("\t", ""), out _))
                    throw new NotImplementedException("ToDo"); //ToDo: fail on non-existant variable OR inactive variable.

                //If the input value is NOT a number, then it's a variable
                else if (!int.TryParse(instruction.lineContent[2], out int result))
                    if (!CheckVarState(instruction.lineContent[2].Replace("\t", ""), out _))
                        throw new NotImplementedException("ToDo"); //ToDo: fail on non-existant variable OR inactive variable.

                instruction.instructionType = PLUSEQUALS;
                func.body.Add(instruction);
                return NO_ERROR;
            }
            static ErrorCodes ParseOut(ref Instruction instruction, ref Function func)
            {
                //ToDo: parse variable, check it's active
                instruction.instructionType = OUT;
                func.body.Add(instruction);
                return NO_ERROR;
            }
            static ErrorCodes ParseSimpleMathsOp(ref Instruction instruction, ref Function func)
            {
                if (!CheckVarState(instruction.lineContent[0].Replace("\t", ""), out _))
                    throw new NotImplementedException("ToDo"); //ToDo: fail on non-existant variable OR inactive variable.

                // array content: [0] [1] [2] [3] [4]
                //                 x   =   y   +   z
                //                 i   +   j

                instruction.lineContent[0].Replace("\t", "");
                int operand_index = instruction.lineContent[1] == "=" ? 3 : 1;  // only looking for +, -, *, /

                switch (instruction.lineContent[operand_index])
                {
                    case "+": instruction.instructionType = PLUS; break;
                    case "-": instruction.instructionType = MINUS; break;
                    case "*": instruction.instructionType = MULTIPLY; break;
                    case "/": instruction.instructionType = DIVIDE; break;
                }

                func.body.Add(instruction);
                return NO_ERROR;
            };
            static ErrorCodes ParsePtr(ref Instruction instruction, ref Function func)
            {
                if (!CheckVarState(instruction.lineContent[0].Replace("\t", ""), out _))
                    throw new NotImplementedException("ToDo"); //ToDo: fail on non-existant variable OR inactive variable.
                else
                    instruction.instructionType = ASSIGNPTR;

                func.body.Add(instruction);
                return NO_ERROR;
            };
            static ErrorCodes ParseAssignment(ref Instruction instruction, ref Function func)
            {
                if (!CheckVarState(instruction.lineContent[0].Replace("\t", ""), out int varIndex))
                    throw new NotImplementedException("ToDo"); //ToDo: fail on non-existant variable OR inactive variable.

                instruction.instructionType = ASSIGNMENT;

                if (variables[varIndex].type == Types.INT)
                {
                    instruction.assignmentType = Types.INT;
                    //ToDo: These if statements can be concatenated
                    //Check if the assignment is, correctly, another integer
                    if (int.TryParse(instruction.lineContent[2], out int result))
                        return NO_ERROR;

                    //Check if it's a variable instead
                    else if (!CheckVarState(instruction.lineContent[2].Replace("\t", ""), out varIndex))
                        throw new NotImplementedException("ToDo"); //ToDo: fail on non-existant variable OR inactive variable.

                }
                else if (variables[varIndex].type == Types.BOOL)
                {
                    instruction.assignmentType = Types.BOOL;
                    string assignmentValue = instruction.lineContent[2].Replace("\t", "");
                    if (assignmentValue.ToUpper() == "TRUE" || assignmentValue.ToUpper() == "FALSE")
                        return NO_ERROR;

                    //Check if it's a variable instead
                    else if (!CheckVarState(assignmentValue, out varIndex))
                        throw new NotImplementedException("ToDo"); //ToDo: fail on non-existant variable OR inactive variable.
                }
                else
                    throw new NotImplementedException();

                func.body.Add(instruction);
                return NO_ERROR;
            }
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

            //Read all the files into a list of strings.
            var fileText = File.ReadLines(inputFileName).ToList();

            //For every line in the file
            for (int i = 0; i < fileText.Count; i++)
            {
                //Ignore white space lines.
                if (fileText[i] == string.Empty)
                    continue;

                /*Loop will look for global declarations or functions. 
                 * There is nothing else to check for, as other instructions must be made INSIDE of functions.*/

                //Ignore comments - ToDo: pass to assembly file, comments start with ; in MASM?
                if (fileText[i].Length >= 2 && fileText[i][0..2] == "��")
                    continue;

                #region Declarations
                //If a declaration pattern is found
                if (Patterns.createPattern.Match(fileText[i]) != Match.Empty)
                {
                    //Generate an array of values from the line, splitting on a space.
                    var values = fileText[i].Split(" ");

                    //Parse the declaration using the ParseDeclaration function.
                    ErrorCodes errorCode = ParseDeclaration(values, fileText[i], i);

                    //If the error code is not NO_ERROR.
                    if (errorCode != NO_ERROR)
                        return errorCode;
                }
                #endregion

                #region Functions
                //If a funtion pattern is found
                else if (Patterns.functionPattern.Match(fileText[i]) != Match.Empty)
                {
                    //Get the values of the function declarations, split on spaces and paranthesis.
                    var values = fileText[i].Split(new char[] { ' ', '(', ')' });

                    //Initialize a new function
                    Function func = new Function()
                    {
                        //The name is always the first value.
                        name = values[0],
                    };

                    //Check the return type is a valid type, and not made up or incorrect spelt.
                    if (!Enum.TryParse(values.Last(), true, out func.returnType))
                        return SetLastError(i, INVALID_RETURN_TYPE, fileText[i]);

                    //Check the function open and closes with the correct brackers, and grab the function body.
                    if (fileText[i + 1] == "{")
                    {
                        bool closeFunction = false;

                        //For each line after the initial {
                        for (i += 2; i < fileText.Count; i++)
                        {
                            //If end of function
                            if (fileText[i] == "}")
                            {
                                closeFunction = true;
                                break;
                            }

                            //Create a new instruction and grab the line content, remove the ; and split on the characters.
                            Instruction instruction = new Instruction
                            {
                                lineContent = fileText[i].Replace(";", "").Split(new char[] { ' ', '(', ')' }),
                            };
                            
                            string patternText = fileText[i].Replace("\t", "");

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
                                        //Check the conversion is valid.
                                        try
                                        {
                                            Convert.ToUInt32(instruction.lineContent[1], 16);
                                        }
                                        catch
                                        {
                                            return SetLastError(i, INVALID_RETURN_TYPE, fileText[i]);
                                        }

                                        //If the first letter is a letter, then add a 0 as it's  in assembly.
                                        if (Patterns.lettersOnly.Match(instruction.lineContent[1][2..][0].ToString()) != Match.Empty)
                                            instruction.lineContent[1] = '0' + instruction.lineContent[1][2..] + 'h';

                                        //Otherwise just cut the 0x off and append a h to conform to assembly.
                                        else
                                            instruction.lineContent[1] = instruction.lineContent[1][2..] + 'h';

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


                            //else if (Patterns.forPattern.Match(patternText) != Match.Empty)
                            //{
                            //    ErrorCodes errorCode = ParseSubLoop(FOR, ref func, ref instruction, fileText, ref i);
                            //    if (errorCode != NO_ERROR)
                            //        return errorCode;
                            //}
                            //If generic
                            else
                            {
                                ErrorCodes temp = ParseGenerics(patternText, ref instruction, ref func, fileText, ref i);
                                if (temp != NO_ERROR)
                                    throw new NotImplementedException("ToDo");
                            };


                            //else
                            //    throw new NotImplementedException("ToDo"); //ToDo - if no pattern found then what?
                            
                            

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
                    //Temporary fix:
                    if (fileText[i].Contains("{") || fileText[i].Contains("}"))
                        continue;
                    //if no pattern is found then what?
                    throw new NotImplementedException();
                }
            }
            return NO_ERROR;
        }
    }
}

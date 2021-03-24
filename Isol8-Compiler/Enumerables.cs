using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isol8_Compiler
{
    internal static class Enumerables
    {
        internal enum Keywords
        {
            NULL,   //Invalid
            CREATE,
            DELETE,
        }
        internal enum Operators
        {
            [Display(Name = "+")]
            Add,
        }
        internal enum Types
        {
            NULL,
            BYTE,
            SHORT,
            LONG,
            INT,
            STRING,
            PTR,
            BOOL,
            INTARRAY,
        }
        internal enum ErrorCodes
        {
            NO_ERROR = 0x0,
            INVALID_KEYWORD = 0x20001,
            INVALID_VAR_NAME = 0x20002,
            INVALID_TYPE = 0x20003,
            TYPE_MISMATCH = 0x20004,
            ML64_ERROR = 0x20005,
            INVALID_RETURN_TYPE = 0x20006,
            DUPLICATE_VAR_NAME = 0x20007,
            NO_CLOSING_BRACKET = 0x20008,
            INVALID_VAR_VALUE = 0x20009,
            NO_OPENING_BRACKET = 0x20010,
            DUPLICATE_FUNC_NAME = 0x20011,
            NO_PATTERN_MATCH = 0x20012,
            INACTIVE_VAR = 0x20013,
        }
        internal enum Scope
        {
            GLOBAL,
            LOCAL,
        }
        internal enum VarState
        {
            ACTIVE,
            DELETED
        }
        internal enum InstructionTypes
        {
            NULL,   //Invalid
            RET,
            PLUSEQUALS,
            ASSIGNMENT,
            ASSIGNPTR,
            OUT,
            IN,
            DELETE,
            BREAK,
            IF,
            ENDIF,
            FOR,
            ENDFOR,
            PLUS,
            MINUS,
            MULTIPLY,
            DIVIDE,
            LESSTHAN,
            GREATERTHAN,
            LESSEQUAL,
            GREATEREQUAL,
            ISEQUAL,
            ISNOTEQUAL,
        }   
    }
}

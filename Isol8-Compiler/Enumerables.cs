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
            NULL,   //Invalid
            [Display(Name = "INT")]
            INT,
            STRING,
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
        
    }
}

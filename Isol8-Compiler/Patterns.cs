using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Isol8_Compiler
{
    public static class Patterns
    {
        public static readonly Regex createPattern = new Regex(@"\b(CREATE|create)\b \w+ \b(AS|as)\b \b(?:INT|STRING|PTR|int|string|ptr)\b (.*);");
        public static readonly Regex lettersOnly = new Regex(@"^[a-zA-Z]+$");
        public static readonly Regex standardOrHexDigitsOnly = new Regex(@"^[0-9a-zA-F]*$");
        public static readonly Regex functionPattern = new Regex(@"[A-Za-z]\w*\((?:(?:[A-Za-z]+) +(?:[A-Za-z]\w*))?(?: *, *(?:[A-Za-z]+) (?:[A-Za-z]\w*))*\) \b(RET|ret)\b (?:[A-Za-z]+)");
        public static readonly Regex stringPattern = new Regex("\".*\"");

        #region Function Patterns
        public static readonly Regex retPattern = new Regex(@"\b(RET|ret)\b \w+");
        #endregion
        #region Maths Patterns
        public static readonly Regex simpleAdditionOperator = new Regex("^[A-Za-z]+ \\+= [A-Za-z0-9];$");
        #endregion
    }
}

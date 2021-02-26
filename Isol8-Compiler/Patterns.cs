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
        public static readonly Regex assignPattern = new Regex("^[a-zA-Z]+ = \"?[a-zA-Z0 -9]+\"?;$");
        public static readonly Regex createPattern = new Regex(@"\b(CREATE|create)\b \w+ \b(AS|as)\b \b(?:INT|STRING|PTR|BOOL|int|string|ptr|bool)\b (.*);");
        public static readonly Regex lettersOnly = new Regex(@"^[a-zA-Z]+$");
        public static readonly Regex standardOrHexDigitsOnly = new Regex(@"^[0-9a-zA-F]*$");
        public static readonly Regex functionPattern = new Regex(@"[A-Za-z]\w*\((?:(?:[A-Za-z]+) +(?:[A-Za-z]\w*))?(?: *, *(?:[A-Za-z]+) (?:[A-Za-z]\w*))*\) \b(RET|ret)\b (?:[A-Za-z]+)");
        public static readonly Regex stringPattern = new Regex("\".*\"");
        public static readonly Regex ptrPattern = new Regex("^[a-zA-Z]+ = \\([a-zA-Z]+\\)[a-zA-Z]+;$");

        #region Function Patterns
        // Use RegexOptions.IgnoreCase
        public static readonly Regex forPattern = new Regex(@"^for [(][0-9]+[)]$");
        public static readonly Regex retPattern = new Regex(@"^ret\s?[a-zA-Z0-9]*?;$", RegexOptions.IgnoreCase);
        public static readonly Regex outPattern = new Regex(@"^(out|OUT)\s?\([a-zA-Z0-9]*\);$");
        public static readonly Regex ifPattern = new Regex(@"^if ?[[a-zA-Z]+]? == ?[[a-zA-Z0-9]+]?$");

        #endregion
        #region Maths Patterns
        public static readonly Regex simpleSelfAdditionOperator = new Regex("^[A-Za-z]+ \\+= [A-Za-z0-9]+;$");
        //public static readonly Regex simpleMathsOperator = new Regex("^[A-Za-z]+ (\\+|-|/|\\*) [A-Za-z0-9];$");
        public static readonly Regex simpleMathsOperator = new Regex(@"(?:([A-Za-z]+ \= ))?[A-Za-z]+ (\+|\-|\/|\*) [A-Za-z0-9]+;$");
        #endregion
        #region Memory Patterns
        public static readonly Regex deletePattern = new Regex("^del\\s[A-Za-z]+;", RegexOptions.IgnoreCase);
        #endregion
    }
}

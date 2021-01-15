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
        public static readonly Regex createPattern = new Regex(@"\b(CREATE)\b \w+ \b(AS)\b \b(INT|STRING)\b \w+;");
        public static readonly Regex lettersOnly = new Regex(@"^[a-zA-Z]+$");
        public static readonly Regex functionPattern = new Regex(@"\b(FUNCTION)\b \w+ \([\s\S].*[\s\S]\)"); // ToDo: Finish
        public static readonly Regex paramPattern = new Regex(@"\w+ \w+,?");
    }   
}

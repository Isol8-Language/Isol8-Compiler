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
        
        public static readonly Regex delcarePattern = new Regex(@"\b(CREATE|DELETE)\b \w+ \b(AS)\b \b(INT|STRING)\b \w+");
    }
}

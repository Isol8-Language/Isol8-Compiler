using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

namespace Isol8_Compiler
{
    class Compiler
    {
        private List<Variable> variables;
        FileStream inputSyntaxFile;



        public Compiler(string file)
        {

            Match syntaxMatch;
            var fileText = File.ReadLines("File.txt");
            foreach (var line in fileText)
            {
                if ((syntaxMatch = Patterns.delcarePattern.Match(line)) != default)
                {
                    int me = 0;
                };

            
            }
        }
        private enum Keywords
        {
            CREATE,
            DELETE,
        }
        private enum Operators
        {
            [Display(Name = "+")]
            Add,


        }
        private class Variable
        {
            string name;
        }
    }
}

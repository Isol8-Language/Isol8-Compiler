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
        private List<string> variables = new List<string>();
        private List<Declaration> declarationStatements = new List<Declaration>();


        public Compiler(string file)
        {
            Parse(file);
            CreateAssemblyFile();
        }

        private void CreateAssemblyFile()
        {
            var outputFile = File.Create("Output.txt");
            string output = ".DATA\n";
            for (var i = 0; i < declarationStatements.Count; i++)
            {
                output += "    " + declarationStatements[i].variableName + " ";
                switch(declarationStatements[i].type)
                {
                    case (Type.INT):
                        output += "DD ";
                        break;
                }
                output += declarationStatements[i].value + '\n';
            }
            byte[] outBytes = new UTF8Encoding(true).GetBytes(output);
            outputFile.Write(outBytes);
            outputFile.Close();
        }
        private void Parse(string file)
        {
            Match syntaxMatch;
            var fileText = File.ReadLines("File.txt"); //PLACEHOLDER
            
            foreach (var line in fileText)
            {
                //If a declaration pattern is found
                if ((syntaxMatch = Patterns.createPattern.Match(line)) != Match.Empty)
                {
                    //Generate an array of values and a new declaration.
                    Declaration declaration = new Declaration();
                    var values = line.Split(" ");

                    //Ensure the keyword is Create.
                    if (!Enum.TryParse<Keywords>(values[0], out declaration.keyword) && declaration.keyword == Keywords.CREATE)
                    {
                        //Failure on keyword
                        return;
                    };


                    if (!Patterns.lettersOnly.IsMatch(values[1]) || variables.Contains(values[1]))
                    {
                        //Failure on variable name
                        return; 
                    }

                    declaration.variableName = values[1];

                    if (!Enum.TryParse<Type>(values[3], out declaration.type))
                    {
                        //Failure on type
                        return;
                    };

                    //Revisit, inefficient as string usage
                    var trueValue = values[4].Replace(";", string.Empty);

                    if (declaration.type == Type.INT && int.TryParse(trueValue, out _))
                    {
                        declaration.value = trueValue;
                    }
                    else if (declaration.type == Type.STRING)
                    {

                    }
                    else
                    {
                        //Failure on type match (I.E, INT = "Hello"
                        return;
                    }
                    
                    variables.Add(declaration.variableName);
                    declarationStatements.Add(declaration);
                };
            }
        }
        private enum Keywords
        {
            NULL,   //Invalid
            CREATE,
            DELETE,
        }
        private enum Operators
        {
            [Display(Name = "+")]
            Add,
        }
        public enum Type
        {
            NULL,   //Invalid
            [Display(Name = "INT")]
            INT,
            STRING,
        }
        private class Declaration
        {
            public Keywords keyword;
            public string variableName;
            public Type type;
            public string value;
        }
        //private class Variable
        //{
        //    public string name;
        //}
    }
}

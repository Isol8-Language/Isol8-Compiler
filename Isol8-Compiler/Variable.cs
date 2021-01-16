using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Isol8_Compiler.Enumerables;

namespace Isol8_Compiler
{
    class Variable
    {
        public string name;
        public Scope scope;
        public VarState status;
        public Types type;     
        public string value;
    }   
}

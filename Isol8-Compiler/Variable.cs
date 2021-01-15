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
        public Scope scope;   //ToDo -- add enum for scope
        public VarState status;  //ToDo -- as above
        public Types type;     
        public string value;
    }   
}

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
        string name;
        string scope;   //ToDo -- add enum for scope
        string status;  //ToDo -- as above
        Types type;     
        string value;
    }   
}

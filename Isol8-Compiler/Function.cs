using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Isol8_Compiler.Enumerables;
namespace Isol8_Compiler
{
    internal class Function
    {
        internal string name;
        internal int parameterCount;
        internal string returnType;
        internal List<Parameters> parameters = new List<Parameters>();


    }
    internal class Parameters
    {
        public string name;
        public Types type;
        
    };
}

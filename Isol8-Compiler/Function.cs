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
        internal Types returnType;
        internal List<Parameters> parameters = new List<Parameters>();
        internal List<Instruction> body = new List<Instruction>();
    }
    internal class Parameters
    {
        public string name;
        public Types type;

    };
    internal class Instruction
    {
        public InstructionTypes instructionType;
        public string[] lineContent;
    };
}

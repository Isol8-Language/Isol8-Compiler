using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Isol8_Compiler
{
    class Assembly
    {
        //ToDo: pass out in CFE func?
        static IntPtr stackSpace;
        public static string CreateFunctionEntry(string functionName, int additionalStackSpace=0)
        {
            /*IMPORTANT ToDo: Calculate amount of local variables and their sizes, for an example.
            At runtime, RSP starts misaligned by 8 bytes (not divisible by 16). RSP must be divisible by 16 for printf.
            RSP must have 32 bytes of shadow space minimum, 4 bytes for each register (8 registerx4).
            If we have two local variables (two ints) thats 8 bytes, so:
            sub rsp, 32+8 (shadow space + two local variables) gives up an RSP of 48, which is divisible by 16. 
            For the two local variables we would need to add 8 more bytes, then another 8 so it aligns at 16
            Windows functions assume RSP is aligned when CALLED, the call will add +8 but the windows func will deal with it
            If designing our own functions, this is important*/

            string returnVal = $"{functionName} PROC\n";
            stackSpace = new IntPtr(0x28 + additionalStackSpace);

            if (((float)(stackSpace + 8) % 16) != 0)
                throw new NotImplementedException("ToDo: pad stackalign to be divisible by 16");
            returnVal += $"\tsub rsp, {stackSpace.ToString("X")}h\n";
            return returnVal;
        }
        public static string CreateFunctionClose(string functionName, string retVal=null)
        {
            //ToDo: add RSP, stackspace
            string ret = default;


            Variable var = default;
            if (Parser.variables.Any(v => v.name == retVal))
                for (int i = 0; i < Parser.variables.Count; i++)
                    if (Parser.variables[i].name == retVal)
                        var = Parser.variables[i];
            
            //If it's an int then return in a 4 byte register (eax)
            if (var != null && var.type == Enumerables.Types.INT)
                ret += $"\tmov eax, {retVal}\n";

            else if (retVal != null)
                ret += $"\tmov rax, {retVal}\n";
                
            ret += $"\tadd rsp, {stackSpace.ToString("X")}h\n";
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)){
                ret += $"\tmov eax, 60\n";
                ret += $"\tsyscall\n";
            }
            ret += $"\tret\n" + $"{functionName} ENDP\n";
            return ret;
        }
        public static string Delete4ByteVariable(string varName) => $"\tlea rax, {varName}\n" + "\tmov dword ptr [rax], 0\n";

    }
}

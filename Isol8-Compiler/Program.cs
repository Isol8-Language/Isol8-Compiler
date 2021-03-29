using System;
using static Isol8_Compiler.Compiler;
using static Isol8_Compiler.Enumerables;
using static Isol8_Compiler.Enumerables.ErrorCodes;
namespace Isol8_Compiler
{
    static class Resources
    {
        public static string title = (@"
██╗███████╗ ██████╗ ██╗      █████╗ 
██║██╔════╝██╔═══██╗██║     ██╔══██╗
██║███████╗██║   ██║██║     ╚█████╔╝
██║╚════██║██║   ██║██║     ██╔══██╗
██║███████║╚██████╔╝███████╗╚█████╔╝
╚═╝╚══════╝ ╚═════╝ ╚══════╝ ╚════╝ ");
    }
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = args[0];
            string outputName = args[1];
            Console.WriteLine($"{Resources.title}");

            Compiler isol8Compiler = new Compiler(fileName, outputName);
         
            Console.WriteLine($"Compiling {isol8Compiler.outputName} to Assembly...");
            ErrorCodes eStatus = isol8Compiler.CreateAssemblyFile();
            if (eStatus != NO_ERROR)
            {
                Console.WriteLine(GetLastError());
                return;
            }

            Console.WriteLine($"{isol8Compiler.outputName}.asm created successfully.");
            Console.WriteLine("Assembling...");
            
            eStatus = isol8Compiler.Assemble(isol8Compiler.outputName);
            if (eStatus != NO_ERROR)
            {
                Console.WriteLine(GetLastError());
                return;
            }

            Console.WriteLine($"{isol8Compiler.outputName}.exe created");
        }
    }
}

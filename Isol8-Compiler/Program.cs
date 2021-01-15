using System;
using static Isol8_Compiler.Compiler;
using static Isol8_Compiler.Enumerables;
using static Isol8_Compiler.Enumerables.ErrorCodes;
namespace Isol8_Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine
                (@"
██╗███████╗ ██████╗ ██╗      █████╗ 
██║██╔════╝██╔═══██╗██║     ██╔══██╗
██║███████╗██║   ██║██║     ╚█████╔╝
██║╚════██║██║   ██║██║     ██╔══██╗
██║███████║╚██████╔╝███████╗╚█████╔╝
╚═╝╚══════╝ ╚═════╝ ╚══════╝ ╚════╝ 
                ");

            Compiler isol8Compiler = new Compiler("File.txt", "OutputName");
            Console.WriteLine($"Compiling {isol8Compiler.outputName} to Assembly...");
            
            ErrorCodes eStatus = isol8Compiler.CreateAssemblyFile();
            if (eStatus != NO_ERROR)
            {
                Console.WriteLine(GetLastError());
                return;
            }
            Console.WriteLine($"{isol8Compiler.outputName} created successfully.");
            Console.WriteLine("Assembling...");
            eStatus = isol8Compiler.Assemble();
            if (eStatus != NO_ERROR)
            {
                Console.WriteLine(GetLastError());
                return;
            }
            Console.WriteLine($"{isol8Compiler.outputName}.exe created");
        }
    }
}

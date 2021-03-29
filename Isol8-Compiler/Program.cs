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
            
           Console.WriteLine($"{Resources.title}");

            if (args.Length > 2)
            {
                string parameters = string.Empty;
                for (int i = 0; i < args.Length; i++)
                    parameters += $"{i + 1}. " + args[i] + " ";
                
                SetLastError(-1, TOO_MANY_CL_ARGUMENTS, parameters.Trim(' '));
                Console.WriteLine(GetLastError());

                Console.WriteLine("Use syntax: Isol8-Compiler.exe <inputFileName> <outputFileName>.");
                Environment.Exit(0);
            }

            if (args.Length < 2)
            {
                string parameters = string.Empty;
                for (int i = 0; i < args.Length; i++)
                    parameters += $"{i + 1}. " + args[i] + " ";

                SetLastError(-1, TOO_FEW_CLI_ARGUMENTS, parameters.Trim(' '));
                Console.WriteLine(GetLastError());

                Console.WriteLine("Use syntax: Isol8-Compiler.exe <inputFileName> <outputFileName>.");
                Environment.Exit(0);
            }

            string fileName = args[0];
            string outputName = args[1];

            if (fileName == string.Empty)
            {
                SetLastError(-1, INVALID_FILE_NAME, "Argument 1 is empty!");
                Console.WriteLine("Use syntax: Isol8-Compiler.exe <inputFileName> <outputFileName>.");

                Environment.Exit(0);
            }
            if (outputName == string.Empty)
            {
                Console.WriteLine("Use syntax: Isol8-Compiler.exe <inputFileName> <outputFileName>.");
                SetLastError(-1, INVALID_FILE_NAME, "Argument 2 is empty!");

                Environment.Exit(0);
            }


            
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

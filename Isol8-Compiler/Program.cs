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
            Compiler isol8Compiler = new Compiler("File.txt");  //Placeholder, constructor does nothing.
            ErrorCodes eStatus = isol8Compiler.CreateAssemblyFile();
            if (eStatus != NO_ERROR)
            {
                Console.WriteLine(GetLastError());
            }
            isol8Compiler.Assemble();

        }
    }
}

using System;
using System.IO;

namespace _05_SuperCalc
{
    class Program
    {
        private static void ParseFromConsole()
        {
            while (true)
            {
                Console.WriteLine("Введите арифметическое выражение, допустимы: +-*/() (enter для выхода):");
                var str = Console.ReadLine();

                if (string.IsNullOrEmpty(str)) return;

                var clp = new CalcLineProcessor(str);
                Console.WriteLine(clp.GetAnswer());
            }
        }

        static void Main(string[] args) => ParseFromConsole();
    }
}

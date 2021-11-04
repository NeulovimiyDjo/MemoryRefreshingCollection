using CalculatorLibrary;
using System;

namespace CalculatorConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Type in the expression to calculate e.g. 2+4*(5-2)/4 or type exit to exit");
            string inputLine = Console.ReadLine();
            while (inputLine != "exit")
            {
                try
                {
                    double result = Calculator.CalculateExpression(inputLine);
                    Console.WriteLine($"= {result}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }           

                inputLine = Console.ReadLine();
            }

        }
    }
}

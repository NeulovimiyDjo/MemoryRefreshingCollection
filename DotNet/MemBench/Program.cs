using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConsoleApp1
{
    class Program
    {
        static Random r = new Random();

        struct s
        {
            public double v1;
            public double v2;
            public double v3;
            public double v4;
            public double v5;
        }

        static void Main(string[] args)
        {
            var list = new List<s>(10 * 25 * 1000 * 1000);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < list.Capacity; i++)
            {
                var el = new s
                {
                    v1 = r.Next(1, 999) * 0.77d,
                    v2 = r.Next(1, 999) * 0.77d,
                    v3 = r.Next(1, 999) * 0.77d,
                    v4 = r.Next(1, 999) * 0.77d,
                    v5 = r.Next(1, 999) * 0.77d,
                };
                list.Add(el);
            }
            sw.Stop();

            Console.WriteLine($"for1 finished in {sw.ElapsedMilliseconds}");

            sw = Stopwatch.StartNew();
            double total = 0.0d;
            for (int i = 0; i < list.Capacity; i++)
            {
                total += list[i].v1 + list[i].v2 + list[i].v3 + list[i].v4 + list[i].v5;
            }
            sw.Stop();

            Console.WriteLine($"for2 finished in {sw.ElapsedMilliseconds}");

            Console.WriteLine(total);
            Console.ReadLine();
        }
    }
}

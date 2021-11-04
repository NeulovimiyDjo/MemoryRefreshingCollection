using System;
using System.Runtime.InteropServices;

namespace SharpConsole
{
    public class A
    {
        public int m;
    }

    public class Test
    {
        public int n;
        public string name;
        public int[] arr = { 1, 2 };
        public A a = new A();
    }

    class Program
    {
        [DllImport(@"..\..\..\Debug\CppDll.dll", EntryPoint = "worksfinally", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Itworks(int a1, int a2);

        [DllImport(@"..\..\..\Debug\CppDll.dll", EntryPoint = "multi3wrapper", CallingConvention = CallingConvention.Cdecl)]
        public static extern double Multi3(IntPtr pCls);

        [DllImport(@"..\..\..\Debug\CppDll.dll", EntryPoint = "createclass", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateC(int c1, int c2, double c3);

        [DllImport(@"..\..\..\Debug\CppDll.dll", EntryPoint = "deleteclass", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteC(IntPtr pCls);

        [DllImport(@"..\..\..\Debug\CppDll.dll", EntryPoint = "printclass", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PrintC(IntPtr pCls);

        static void Main(string[] args)
        {
            Test t;
            t = new Test();
            Console.WriteLine($"{t.n} {t.name} {t.arr.ToString()} {t.a.ToString()}");



            //------------------------
            int res = Itworks(2, 5);
            Console.WriteLine(res);

            IntPtr pClass = CreateC(25, 35, 33);

            Console.WriteLine(Multi3(pClass));
            PrintC(pClass);

            DeleteC(pClass);
            //------------------------
            Console.ReadLine();
        }
    }
}

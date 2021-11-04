using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;
using System.Diagnostics;

namespace CudafyTest
{
    class Program
    {
        static void Info()
        {
            int i = 0;
            foreach (GPGPUProperties prop in CudafyHost.GetDeviceProperties(CudafyModes.Target, false))
            {
                Console.WriteLine("   --- General Information for device {0} ---", i);
                Console.WriteLine("Name:  {0}", prop.Name);
                Console.WriteLine("Platform Name:  {0}", prop.PlatformName);
                Console.WriteLine("Device Id:  {0}", prop.DeviceId);
                Console.WriteLine("Compute capability:  {0}.{1}", prop.Capability.Major, prop.Capability.Minor);
                Console.WriteLine("Clock rate: {0}", prop.ClockRate);
                Console.WriteLine("Simulated: {0}", prop.IsSimulated);
                Console.WriteLine();

                Console.WriteLine("   --- Memory Information for device {0} ---", i);
                Console.WriteLine("Total global mem:  {0}", prop.TotalMemory);
                Console.WriteLine("Total constant Mem:  {0}", prop.TotalConstantMemory);
                Console.WriteLine("Max mem pitch:  {0}", prop.MemoryPitch);
                Console.WriteLine("Texture Alignment:  {0}", prop.TextureAlignment);
                Console.WriteLine();

                Console.WriteLine("   --- MP Information for device {0} ---", i);
                Console.WriteLine("Shared mem per mp: {0}", prop.SharedMemoryPerBlock);
                Console.WriteLine("Registers per mp:  {0}", prop.RegistersPerBlock);
                Console.WriteLine("Threads in warp:  {0}", prop.WarpSize);
                Console.WriteLine("Max threads per block:  {0}", prop.MaxThreadsPerBlock);
                Console.WriteLine("Max thread dimensions:  ({0}, {1}, {2})", prop.MaxThreadsSize.x, prop.MaxThreadsSize.y, prop.MaxThreadsSize.z);
                Console.WriteLine("Max grid dimensions:  ({0}, {1}, {2})", prop.MaxGridSize.x, prop.MaxGridSize.y, prop.MaxGridSize.z);

                Console.WriteLine();

                i++;
            }
        }

        static void SimpleAdd(GPGPU gpu)
        {
            var sw = new Stopwatch();            
            sw.Restart();
            double c;
            double[] dev_c = gpu.Allocate<double>(); // cudaMalloc one Int32
            sw.Stop();
            Console.WriteLine("Allocating: " + sw.ElapsedMilliseconds);

            sw.Restart();
            gpu.Launch().add(2.3, 7.5, dev_c); // or gpu.Launch(1, 1, "add", 2, 7, dev_c);
            sw.Stop();
            Console.WriteLine("Adding: " + sw.ElapsedMilliseconds);

            sw.Restart();
            gpu.CopyFromDevice(dev_c, out c);
            sw.Stop();
            Console.WriteLine("Copying Back: " + sw.ElapsedMilliseconds);

            Console.WriteLine("2.3 + 7.5 = {0}", c);


            gpu.Launch().sub(2.0, 7.5, dev_c);
            gpu.CopyFromDevice(dev_c, out c);

            Console.WriteLine("2 - 7.5 = {0}", c);

            gpu.Free(dev_c);

        }

        [Cudafy]
        public static void add(double a, double b, double[] c)
        {
            c[0] = a + b;
        }

        [Cudafy]
        public static void sub(double a, double b, double[] c)
        {
            c[0] = a - b;
        }

        public const int N = 1024*128;

        public static void LoopTest(GPGPU gpu)
        {
            var r = new Random();

            var sw = new Stopwatch();

            double[] a = new double[N];
            double[] b = new double[N];
            double[] c = new double[N];

            // allocate the memory on the GPU
            double[] dev_a = gpu.Allocate<double>(a);
            double[] dev_b = gpu.Allocate<double>(b);
            double[] dev_c = gpu.Allocate<double>(c);

            // fill the arrays 'a' and 'b' on the CPU
            for (int i = 0; i < N; i++)
            {
                a[i] = r.Next(10) / 9.37 ;
                b[i] = r.Next(10) / 10.37;
            }
            var sw2 = new Stopwatch();
            sw.Restart();
            // copy the arrays 'a' and 'b' to the GPU
            gpu.CopyToDevice(a, dev_a);
            gpu.CopyToDevice(b, dev_b);
            for (int k = 0; k < 100000; k++)
            {
                // launch add on N threads
                sw2.Start();
                gpu.Launch(1024, 128).adder(dev_a, dev_b, dev_c);
                sw2.Stop();
                // copy the array 'c' back from the GPU to the CPU
            }
            gpu.CopyFromDevice(dev_c, c);

            Console.WriteLine("GPU func only Took: " + sw2.ElapsedMilliseconds);
            sw.Stop();
            Console.WriteLine("GPU Took: " + sw.ElapsedMilliseconds);

            // display the results
            for (int i = N-1; i < N; i++)
            {
                Console.WriteLine("{0} * {1} = {2}", a[i], b[i], c[i]);
            }

            // free the memory allocated on the GPU
            gpu.Free(dev_a);
            gpu.Free(dev_b);
            gpu.Free(dev_c);

            sw.Restart();
            Parallel.For(0, 100000, k =>
            {
                for (int i = 0; i < N; i++)
                {
                    c[i] = a[i] * b[i];
                }
            });
            sw.Stop();
            Console.WriteLine("CPU Took: " + sw.ElapsedMilliseconds);

            for (int i = N - 1; i < N; i++)
            {
                Console.WriteLine("{0} * {1} = {2}", a[i], b[i], c[i]);
            }
        }

        [Cudafy]
        public static void adder(GThread thread, double[] a, double[] b, double[] c)
        {
            int tid = thread.blockDim.x * thread.blockIdx.x + thread.threadIdx.x;
            if (tid < N)
                c[tid] = a[tid] * b[tid];
        }



        static void Main(string[] args)
        {
            //Info();

            var sw = new Stopwatch();
            sw.Restart();          
            CudafyModule km = CudafyTranslator.Cudafy();
            GPGPU gpu = CudafyHost.GetDevice(CudafyModes.Target, CudafyModes.DeviceId);
            gpu.LoadModule(km);
            sw.Stop();
            Console.WriteLine("Loading Took: " + sw.ElapsedMilliseconds);

            //SimpleAdd(gpu);

            sw.Restart();
            LoopTest(gpu);
            sw.Stop();
            Console.WriteLine("Looping Took: " + sw.ElapsedMilliseconds);


            Console.ReadKey();
        }
    }
}
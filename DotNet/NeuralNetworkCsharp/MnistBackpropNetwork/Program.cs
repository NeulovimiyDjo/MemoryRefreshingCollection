using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuralNetworkLibrary;
using System.Diagnostics;

namespace MnistBackpropNetwork
{
    class Program
    {
        static void Main(string[] args)
        {
            MnistDataReader mdr = new MnistDataReader();

            var trainingImages = mdr.ReadImages(@"..\..\..\Mnist\train-images.idx3-ubyte", 50000);
            var trainingLabels = mdr.ReadLabels(@"..\..\..\Mnist\train-labels.idx1-ubyte", 50000);

            var testLabels = mdr.ReadLabels(@"..\..\..\Mnist\t10k-labels.idx1-ubyte", 10000);
            var testImages = mdr.ReadImages(@"..\..\..\Mnist\t10k-images.idx3-ubyte", 10000);

            var sw = new Stopwatch();

            //trainingImages = Network.Shrink(trainingImages, 4);
            //testImages = Network.Shrink(testImages, 4);

            var forest = new NetForest();
            for (int i = 0; i < 1; i++)
            {
                forest.Networks.Add(new Network(784 / 1 / 1, new int[] { 150, 500, 100, 10 }, 1.0f, 0.5f, 10));
            }


            for (int i = 0; i < 60; i++)
            {
                sw.Restart();
                forest.LearnOneEpoch(trainingImages, trainingLabels);
                sw.Stop();

                Console.WriteLine("Epoch: " + i + "  Time: " + sw.ElapsedMilliseconds);

                if (i % 6 == 5 && i < 30)
                {
                    foreach (var net in forest.Networks)
                    {
                        net.DescendSpeed *= 0.7f;
                    }
                }

                if (i == 30)
                {
                    foreach (var net in forest.Networks)
                    {
                        net.BatchSize = 32;
                    }
                }

                sw.Restart();
                forest.Evaluate(testImages, testLabels, LogConsole);
                sw.Stop();
                Console.WriteLine("EvalTime: " + sw.ElapsedMilliseconds + "\n");
            }

            Console.ReadKey();
        }

        static void LogConsole(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}

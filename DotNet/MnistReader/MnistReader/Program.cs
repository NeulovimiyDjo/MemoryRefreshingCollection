using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace MnistReader
{
    class Program
    {
        static void Main(string[] args)
        {
            string pathImages = @"..\..\..\Mnist\t10k-images.idx3-ubyte";
            string pathLabels = @"..\..\..\Mnist\t10k-labels.idx1-ubyte";

            var sw = new Stopwatch();
            sw.Start();

            byte[][][] resultsPixels = new byte[10000][][];
            byte[] resultsLabels = new byte[10000];


            try
            {
                using (BinaryReader imagesReader = new BinaryReader(File.OpenRead(pathImages)), labelsReader = new BinaryReader(File.OpenRead(pathLabels)))
                {
                    imagesReader.ReadInt32();
                    imagesReader.ReadInt32();
                    imagesReader.ReadInt32();
                    imagesReader.ReadInt32();

                    labelsReader.ReadInt32();
                    labelsReader.ReadInt32();

                    byte[][] pixels = new byte[28][];
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        pixels[i] = new byte[28];
                    }


                    for (int n = 0; n < 10000; n++)
                    {
                        for (int i = 0; i < 28; i++)
                        {
                            for (int j = 0; j < 28; j++)
                            {
                                pixels[i][j] = imagesReader.ReadByte();
                            }
                        }

                        resultsPixels[n] = pixels;
                        //DrawDigit(pixels);

                        byte lbl = labelsReader.ReadByte();
                        resultsLabels[n] = lbl;
                        //Console.WriteLine("--Labels is--  "+lbl + "\n\n");
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);



            byte[][][] resultsPixels2 = new byte[10000][][];
            byte[] resultsLabels2 = new byte[10000];

            sw.Reset();
            sw.Start();
            for (int n = 0; n < 100; n++)
            {
                for (int i = 0; i < 10000; i++)
                {
                    resultsPixels2[i] = resultsPixels[i];
                    resultsLabels2[i] = resultsLabels[i];
                }
            }
            
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);



            Console.Read();
        }

        static void DrawDigit(byte [][] pixels)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < 28; i++)
            {
                for (int j = 0; j < 28; j++)
                {
                    double d = Math.Round((pixels[i][j] / 28.5));

                    sb.Append(d.ToString());
                    sb.Append(" ");
                }
                sb.Append("\n");
            }

            Console.WriteLine(sb.ToString());
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Diagnostics;

namespace NeuralNetworkLibrary
{
    public class Network
    {
        public int Depth { get; set; }

        float _descendSpeed = 1;
        int _batchSize = 1;

        public float DescendSpeed { get { return _descendSpeed; } set { _descendSpeed = value; RealSpeed = _descendSpeed / _batchSize; } }

        public int BatchSize { get { return _batchSize; } set { _batchSize = value; RealSpeed = _descendSpeed / _batchSize; } }

        public float RealSpeed { get; private set; }

        public float DecaySpeed { get; set; }


        Layer[] Layers { get; set; }

        int InputsSize { get; set; }

        public Network(int inputsSize, int[] LayersSizes, float descendSpeed, float decaySpeed, int batchSize)
        {
            InputsSize = inputsSize;

            Layers = new Layer[LayersSizes.Length];
            for (int i = 0; i < Layers.Length; i++)
            {
                if (i == 0)
                {
                    Layers[i] = new Layer(LayersSizes[i], inputsSize);
                }
                else
                {
                    Layers[i] = new Layer(LayersSizes[i], LayersSizes[i - 1]);
                }
            }

            DescendSpeed = descendSpeed;
            BatchSize = batchSize;

            RealSpeed = _descendSpeed / _batchSize;

            DecaySpeed = decaySpeed;

            Depth = LayersSizes.Length;
        }

        public void LearnOneEpoch(byte[][] images, byte[] labels, bool doParallel = true)
        {
            var sw1 = new Stopwatch();
            var sw2 = new Stopwatch();
            var sw3 = new Stopwatch();
            var sw4 = new Stopwatch();

            for (int t = 0; t < labels.Length / BatchSize; t++)
            {
                sw4.Start();
                var BiasesGrads = new Vector<float>[Depth];
                var WeightsGrads = new Matrix<float>[Depth];
                for (int i = 0; i < Depth; i++)
                {
                    BiasesGrads[i] = Vector<float>.Build.Dense(Layers[i].Size);
                    WeightsGrads[i] = Matrix<float>.Build.Dense(Layers[i].Size, Layers[i].Weights.ColumnCount);
                }
                sw4.Stop();

                if (doParallel)
                {
                    Parallel.For(t * BatchSize, t * BatchSize + BatchSize, m =>
                    {
                        var expectedResults = Vector<float>.Build.Dense(Layers[Depth - 1].Size, i =>
                        {
                            if (i == labels[m]) return 1; else return 0;
                        });

                        sw1.Start();
                        FeedForward(images[m], labels[m], out TempLayerValues[] tlvs, out Vector<float> inputs);
                        sw1.Stop();

                        sw2.Start();
                        BackPropDeltas(expectedResults, tlvs, out Vector<float>[] Deltas);
                        sw2.Stop();

                        sw3.Start();
                        for (int i = Depth - 1; i >= 0; i--)
                        {
                            Interlocked.Exchange(ref BiasesGrads[i], BiasesGrads[i] + Deltas[i]);
                            if (i == 0)
                            {
                                Interlocked.Exchange(ref WeightsGrads[i], WeightsGrads[i] + Deltas[i].OuterProduct(inputs));
                            }
                            else
                            {
                                Interlocked.Exchange(ref WeightsGrads[i], WeightsGrads[i] + Deltas[i].OuterProduct(tlvs[i - 1].Values));
                            }
                            sw3.Stop();
                        }
                    });
                }
                else
                {
                    for (int m = t * BatchSize; m < t * BatchSize + BatchSize; m++)
                    {
                        var expectedResults = Vector<float>.Build.Dense(Layers[Depth - 1].Size, i =>
                        {
                            if (i == labels[m]) return 1; else return 0;
                        });

                        sw1.Start();
                        FeedForward(images[m], labels[m], out TempLayerValues[] tlvs, out Vector<float> inputs);
                        sw1.Stop();

                        sw2.Start();
                        BackPropDeltas(expectedResults, tlvs, out Vector<float>[] Deltas);
                        sw2.Stop();

                        sw3.Start();
                        for (int i = Depth - 1; i >= 0; i--)
                        {
                            Interlocked.Exchange(ref BiasesGrads[i], BiasesGrads[i] + Deltas[i]);
                            if (i == 0)
                            {
                                Interlocked.Exchange(ref WeightsGrads[i], WeightsGrads[i] + Deltas[i].OuterProduct(inputs));
                            }
                            else
                            {
                                Interlocked.Exchange(ref WeightsGrads[i], WeightsGrads[i] + Deltas[i].OuterProduct(tlvs[i - 1].Values));
                            }
                            sw3.Stop();
                        }
                    }
                }
                
               

                sw4.Start();
                for (int i = 0; i < Depth; i++)
                {
                    Layers[i].Biases -= BiasesGrads[i] * RealSpeed;

                    //Layers[i].Weights -= WeightsGrads[i] * RealSpeed;
                    Layers[i].Weights = Layers[i].Weights * (1-DecaySpeed*DescendSpeed/labels.Length) - WeightsGrads[i] * RealSpeed;
                }
                sw4.Stop();
            }

            //Console.WriteLine("sw1: " + sw1.ElapsedMilliseconds);
            //Console.WriteLine("sw2: " + sw2.ElapsedMilliseconds);
            //Console.WriteLine("sw3: " + sw3.ElapsedMilliseconds);
            //Console.WriteLine("sw4: " + sw4.ElapsedMilliseconds);

        }

        public byte FeedForward(byte[] image, byte label, out TempLayerValues[] tlvs, out Vector<float> inputs)
        {
            inputs = Vector<float>.Build.Dense(InputsSize);
            for (int i = 0; i < image.Length; i++)
            {
                inputs[i] = image[i] * 1.0f / 255;
            }

            tlvs = new TempLayerValues[Layers.Length];

            for (int i = 0; i < Layers.Length; i++)
            {
                tlvs[i] = new TempLayerValues(Layers[i].Size);

                if (i == 0)
                {
                    tlvs[i].ZSum = Layers[i].Weights * inputs + Layers[i].Biases;
                    tlvs[i].Values = (tlvs[i].ZSum).Map(Sigmoid);
                }
                else
                {
                    tlvs[i].ZSum = Layers[i].Weights * tlvs[i - 1].Values + Layers[i].Biases;
                    tlvs[i].Values = (tlvs[i].ZSum).Map(Sigmoid);
                }
            }


            byte max = (byte)tlvs[Layers.Length - 1].Values.AbsoluteMaximumIndex();

            byte res = 0;

            if (max == label)
            {
                res = 1;
            }

            return max;
        }

        void BackPropDeltas(Vector<float> expectedResults, TempLayerValues[] tlvs, out Vector<float>[] Deltas)
        {
            Deltas = new Vector<float>[Depth];
            for (int i = Depth - 1; i >= 0; i--)
            {
                Deltas[i] = Vector<float>.Build.Dense(Layers[i].Size);

                if (i == Depth - 1)
                {
                    Deltas[i] = (tlvs[i].Values - expectedResults);
                }
                else
                {
                    Deltas[i] = Layers[i + 1].Weights.TransposeThisAndMultiply(Deltas[i + 1]);
                    Deltas[i] = Deltas[i].PointwiseMultiply(tlvs[i].ZSum.Map(SigmoidPrime));
                }
            }
        }        

        public void Evaluate(byte[][] images, byte[] labels, Action<string> logger)
        {
            int count = 0;

            Parallel.For(0, labels.Length, i =>
            {
                byte r = FeedForward(images[i], labels[i], out TempLayerValues[] tlvs, out Vector<float> inputs);
                if (r == labels[i])
                {
                    Interlocked.Increment(ref count);
                }
            });

            logger.Invoke("Success rate: " + (1.0 * count / labels.Length).ToString());
        }

        static float Sigmoid(float el)
        {
            return (float)(1 / (1.0 + Math.Exp(-el)));
        }

        static float SigmoidPrime(float el)
        {
            return Sigmoid(el) * (1.0f - Sigmoid(el));
        }

        public static void Shuffle(byte[][] images, byte[] labels)
        {
            var r = new Random();
 
            for (int i = 0; i < labels.Length; i++)
            {
                int rnd = r.Next(labels.Length - 1 - i);

                byte[] itmp = images[labels.Length - 1 - i];
                images[labels.Length - 1 - i] = images[rnd];
                images[rnd] = itmp;

                byte ltmp = labels[labels.Length - 1 - i];
                labels[labels.Length - 1 - i] = labels[rnd];
                labels[rnd] = ltmp;
            }
        }

        public static byte[][] Shrink(byte[][] images, int scale)
        {
            var res = new byte[images.Length][];
            var tmp = new int[images.Length][];
            for (int k = 0; k < images.Length; k++)
            {
                res[k] = new byte[784 / scale / scale];
                tmp[k] = new int[784 / scale / scale];
                for (int i = 0; i < 784 / scale / scale; i++)
                {
                    tmp[k][i] = 0;
                }

                for (int i = 0; i < 28; i++)
                {
                    for (int j = 0; j < 28; j++)
                    {
                        tmp[k][i / scale * 28 / scale + j / scale] += images[k][i * 28 + j];
                    }
                }

                for (int i = 0; i < 784 / scale / scale; i++)
                {
                    res[k][i] = (byte)Math.Round(tmp[k][i]*1.0 / scale / scale);
                }

            }

            return res;

        }
    }
}

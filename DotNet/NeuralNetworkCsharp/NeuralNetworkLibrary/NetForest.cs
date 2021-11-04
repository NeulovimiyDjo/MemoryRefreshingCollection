using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Diagnostics;
using MathNet.Numerics;

namespace NeuralNetworkLibrary
{
    public class NetForest
    {
        public List<Network> Networks { get; set; }

        public NetForest()
        {
            Networks = new List<Network>();
        }

        public void LearnOneEpoch(byte[][] images, byte[] labels)
        {
            /*foreach (var net in Networks)
            {
                Network.Shuffle(images, labels);
                net.LearnOneEpoch(images, labels);
            }*/
            Control.UseNativeOpenBLAS();

            Network.Shuffle(images, labels);
            Parallel.ForEach(Networks, (net) =>
            {
                net.LearnOneEpoch(images, labels, true);
            });
        }

        public void Evaluate(byte[][] images, byte[] labels, Action<string> logger)
        {
            int count = 0;
            int[] counts = new int[Networks.Count];
            for (int i = 0; i < counts.Length; i++)
            {
                counts[i] = 0;
            }

            Parallel.For(0, labels.Length, i =>
            {
                byte res = 0;
                res = ClassifyImage(images[i], labels[i], counts);

                if (res == labels[i])
                {
                    Interlocked.Increment(ref count);
                }
            });

            logger.Invoke("Success rate: " + (1.0 * count / labels.Length).ToString());

            for (int i = 0; i < Networks.Count; i++)
            {
                logger.Invoke("Success rate of net" + i + " : " + (1.0 * counts[i] / labels.Length).ToString());
            }
        }

        byte ClassifyImage(byte[] image, byte label, int[] counts)
        {
            byte[] reses = new byte[Networks.Count];
            int i = 0;
            foreach (var net in Networks)
            {
                reses[i] += net.FeedForward(image, label, out TempLayerValues[] tlvs, out Vector<float> inputs);
                if (reses[i] == label)
                {
                    Interlocked.Increment(ref counts[i]);
                }
                i++;
            }

            byte res = 0;
            res = reses.GroupBy(v => v).OrderByDescending(g => g.Count()).First().Key;

            return res;
        }
    }
}

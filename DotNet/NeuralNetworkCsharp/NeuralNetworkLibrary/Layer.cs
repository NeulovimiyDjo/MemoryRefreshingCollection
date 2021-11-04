using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace NeuralNetworkLibrary
{
    class Layer
    {
        public int Size { get; set; }

        public Matrix<float> Weights { get; set; }
        public Vector<float> Biases { get; set; }


        public Layer(int size, int prevLayerSize)
        {
            Size = size;

            Weights = Matrix<float>.Build.Random(size, prevLayerSize, new MathNet.Numerics.Distributions.Normal(0, 1.0f / Math.Sqrt(size)));
            Biases = Vector<float>.Build.Dense(size, 0);
        }
    }
}

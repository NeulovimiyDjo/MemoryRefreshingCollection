using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace NeuralNetworkLibrary
{
    public class TempLayerValues
    {
        public int Size { get; set; }

        public Vector<float> Values { get; set; }
        public Vector<float> ZSum { get; set; }

        public TempLayerValues(int size)
        {
            Values = Vector<float>.Build.Dense(size);
            ZSum = Vector<float>.Build.Dense(size);
        }
    }
}

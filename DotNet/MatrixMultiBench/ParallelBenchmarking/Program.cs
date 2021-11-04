using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Runtime.InteropServices;

class MultiplyMatrices
{
    #region 2D Multiplications
    static void MultiplyMatricesSequential(float[,] matA, float[,] matB, float[,] result)
    {
        int matACols = matA.GetLength(1);
        int matBCols = matB.GetLength(1);
        int matARows = matA.GetLength(0);

        for (int i = 0; i < matARows; i++)
        {
            for (int j = 0; j < matBCols; j++)
            {
                float temp = 0;
                for (int k = 0; k < matACols; k++)
                {
                    temp += matA[i, k] * matB[k, j];
                }
                result[i, j] = temp;
            }
        }
    }

    static void MultiplyMatricesParallel(float[,] matA, float[,] matB, float[,] result)
    {
        int matACols = matA.GetLength(1);
        int matBCols = matB.GetLength(1);
        int matARows = matA.GetLength(0);

        Parallel.For(0, matARows, i =>
        {
            for (int j = 0; j < matBCols; j++)
            {
                float temp = 0;
                for (int k = 0; k < matACols; k++)
                {
                    temp += matA[i, k] * matB[k, j];
                }
                result[i, j] = temp;
            }
        });
    }
    #endregion

    #region Jagged Multiplications
    static void MultiplyMatricesSequentialJ(float[][] matA, float[][] matB, float[][] result)
    {
        int matACols = matA[0].Length;
        int matBCols = matB[0].Length;
        int matARows = matA.Length;

        for (int i = 0; i < matARows; i++)
        {
            for (int j = 0; j < matBCols; j++)
            {
                float temp = 0;
                for (int k = 0; k < matACols; k++)
                {
                    temp += matA[i][k] * matB[k][j];
                }
                result[i][j] = temp;
            }
        }
    }

    static void MultiplyMatricesParallelJ(float[][] matA, float[][] matB, float[][] result)
    {
        int matACols = matA[0].Length;
        int matBCols = matB[0].Length;
        int matARows = matA.Length;

        Parallel.For(0, matARows, i =>
        {
            for (int j = 0; j < matBCols; j++)
            {
                float temp = 0;
                for (int k = 0; k < matACols; k++)
                {
                    temp += matA[i][k] * matB[k][j];
                }
                result[i][j] = temp;
            }
        });
    }
    #endregion

    #region One-Dim Multiplications
    static void MultiplyMatricesSequentialO(float[] matA, int colsA, float[] matB, int colsB, float[] result)
    {
        int matACols = colsA;
        int matBCols = colsB;
        int matARows = matA.Length / colsA;

        for (int i = 0; i < matARows; i++)
        {
            for (int j = 0; j < matBCols; j++)
            {
                float temp = 0;
                for (int k = 0; k < matACols; k++)
                {
                    temp += matA[i * matACols + k] * matB[k * matBCols + j];
                }
                result[i * matBCols + j] = temp;
            }
        }
    }

    static void MultiplyMatricesParallelO(float[] matA, int colsA, float[] matB, int colsB, float[] result)
    {
        int matACols = colsA;
        int matBCols = colsB;
        int matARows = matA.Length / colsA;

        Parallel.For(0, matARows, i =>
        {
            for (int j = 0; j < matBCols; j++)
            {
                float temp = 0;
                for (int k = 0; k < matACols; k++)
                {
                    temp += matA[i * matACols + k] * matB[k * matBCols + j];
                }
                result[i * matBCols + j] = temp;
            }
        });
    }
    #endregion

    #region Pointers-One-Dim Multiplications
    static void MultiplyMatricesSequentialP(float[] matA, int colsA, float[] matB, int colsB, float[] result)
    {
        int matACols = colsA;
        int matBCols = colsB;
        int matARows = matA.Length / colsA;

        unsafe
        {
            fixed (float* pA = matA, pB = matB)
            {
                for (int i = 0; i < matARows; i++)
                {
                    for (int j = 0; j < matBCols; j++)
                    {
                        float temp = 0;
                        for (int k = 0; k < matACols; k++)
                        {
                            temp += *(pA + i * matACols + k) * *(pB + k * matBCols + j);
                        }
                        result[i * matBCols + j] = temp;
                    }
                }
            }
        }
    }

    static void MultiplyMatricesParallelP(float[] matA, int colsA, float[] matB, int colsB, float[] result)
    {
        int matACols = colsA;
        int matBCols = colsB;
        int matARows = matA.Length / colsA;

        Parallel.For(0, matARows, i =>
        {
            for (int j = 0; j < matBCols; j++)
            {
                float temp = 0;
                unsafe
                {
                    fixed (float* pA = matA, pB = matB)
                    {
                        for (int k = 0; k < matACols; k++)
                        {
                            temp += pA[i * matACols + k] * pB[k * matBCols + j];
                        }
                    }
                }
                result[i * matBCols + j] = temp;
            }
        });
    }
    #endregion

    #region Transpose Multiplications
    static void MultiplyMatricesSequentialT(float[] matA, int colsA, float[] matB, int colsB, float[] result)
    {
        int matACols = colsA;
        int matBCols = colsB;
        int matARows = matA.Length / colsA;

        float[] transpB = new float[matACols * matBCols];
        for (int i = 0; i < matACols; i++)
        {
            for (int j = 0; j < matBCols; j++)
            {
                transpB[i * matBCols + j] = matB[j * matACols + i];
            }
        }

        for (int i = 0; i < matARows; i++)
        {
            for (int j = 0; j < matBCols; j++)
            {
                float temp = 0;
                for (int k = 0; k < matACols; k++)
                {
                    temp += matA[i * matACols + k] * transpB[j * matACols + k];
                }
                result[i * matBCols + j] = temp;
            }
        }
    }

    static void MultiplyMatricesParallelT(float[] matA, int colsA, float[] matB, int colsB, float[] result)
    {
        int matACols = colsA;
        int matBCols = colsB;
        int matARows = matA.Length / colsA;

        float[] transpB = new float[matACols * matBCols];
        for (int i = 0; i < matACols; i++)
        {
            for (int j = 0; j < matBCols; j++)
            {
                transpB[i * matBCols + j] = matB[j * matACols + i];
            }
        }

        Parallel.For(0, matARows, i =>
        {
            for (int j = 0; j < matBCols; j++)
            {
                float temp = 0;
                for (int k = 0; k < matACols; k++)
                {
                    temp += matA[i * matACols + k] * transpB[j * matACols + k];
                }
                result[i * matBCols + j] = temp;
            }
        });

    }
    #endregion

    #region Pointers-Transpose Multiplications
    static void MultiplyMatricesSequentialPT(float[] matA, int colsA, float[] matB, int colsB, float[] result)
    {
        int matACols = colsA;
        int matBCols = colsB;
        int matARows = matA.Length / colsA;

        float[] transpB = new float[matACols * matBCols];
        for (int i = 0; i < matACols; i++)
        {
            for (int j = 0; j < matBCols; j++)
            {
                transpB[i * matBCols + j] = matB[j * matACols + i];
            }
        }

        unsafe
        {
            fixed (float* pA = matA, pB = transpB)
            {
                for (int i = 0; i < matARows; i++)
                {
                    for (int j = 0; j < matBCols; j++)
                    {
                        float temp = 0;
                        for (int k = 0; k < matACols; k++)
                        {
                            temp += matA[i * matACols + k] * transpB[j * matACols + k];
                        }
                        result[i * matBCols + j] = temp;
                    }
                }
            }
        }
    }

    static void MultiplyMatricesParallelPT(float[] matA, int colsA, float[] matB, int colsB, float[] result)
    {
        int matACols = colsA;
        int matBCols = colsB;
        int matARows = matA.Length / colsA;

        float[] transpB = new float[matACols * matBCols];
        for (int i = 0; i < matACols; i++)
        {
            for (int j = 0; j < matBCols; j++)
            {
                transpB[i * matBCols + j] = matB[j * matACols + i];
            }
        }

        Parallel.For(0, matARows, i =>
        {
            for (int j = 0; j < matBCols; j++)
            {
                float temp = 0;
                unsafe
                {
                    fixed (float* pA = matA, pB = transpB)
                    {
                        for (int k = 0; k < matACols; k++)
                        {
                            temp += matA[i * matACols + k] * transpB[j * matACols + k];
                        }
                    }
                }
                result[i * matBCols + j] = temp;
            }
        });
    }
    #endregion

    [DllImport(@"C:\Users\Andrey\Desktop\Rep\MatrixMultiBenchmarking\MatrixMultiMingw\mydll.dll", CallingConvention = CallingConvention.StdCall)]
    extern static void MulMa(float[] matA, int rowsA, int colsA, float[] matB, int colsB, float[] result);

    [DllImport(@"C:\Users\Andrey\Desktop\Rep\MatrixMultiBenchmarking\MatrixMultiMingw\mydll.dll", CallingConvention = CallingConvention.StdCall)]
    extern static void MulMaP(float[] matA, int rowsA, int colsA, float[] matB, int colsB, float[] result);

    [DllImport(@"C:\Users\Andrey\Desktop\Rep\MatrixMultiBenchmarking\MatrixMultiMingw\mydll.dll", CallingConvention = CallingConvention.StdCall)]
    extern static void MulMaCachePtrAsm(float[] matA, int rowsA, int colsA, float[] matB, int colsB, float[] result);

    [DllImport(@"C:\Users\Andrey\Desktop\Rep\MatrixMultiBenchmarking\MatrixMultiMingw\mydll.dll", CallingConvention = CallingConvention.StdCall)]
    extern static void MulMaCachePtrAsmPar(float[] matA, int rowsA, int colsA, float[] matB, int colsB, float[] result);

    #region Main
    static void Main(string[] args)
    {
        const int ITER = 10;
        int rowCount = 223;
        int colCount = 931;
        int colCount2 = 413;

        Random r = new Random();

        float[,] m1 = InitializeMatrix(rowCount, colCount, r);
        float[,] m2 = InitializeMatrix(colCount, colCount2, r);
        float[,] result = new float[rowCount, colCount2];

        float[][] m1j = new float[rowCount][];
        float[][] m2j = new float[colCount][];
        float[][] resultj = new float[rowCount][];

        float[] m1o = new float[rowCount * colCount];
        float[] m2o = new float[colCount * colCount2];
        float[] resulto = new float[rowCount * colCount2];

        for (int i = 0; i < rowCount; i++)
        {
            m1j[i] = new float[colCount];
            for (int j = 0; j < colCount; j++)
            {
                m1j[i][j] = m1[i, j];
                m1o[i * colCount + j] = m1[i, j];
            }
        }

        for (int i = 0; i < colCount; i++)
        {
            m2j[i] = new float[colCount2];
            for (int j = 0; j < colCount2; j++)
            {
                m2j[i][j] = m2[i, j];
                m2o[i * colCount2 + j] = m2[i, j];
            }
        }

        for (int i = 0; i < rowCount; i++)
        {
            resultj[i] = new float[colCount2];
        }


        //Console.WriteLine("Number of logical processors on the system: " + Environment.ProcessorCount + "\n");

        Stopwatch stopwatch = new Stopwatch();


        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            MultiplyMatricesSequential(m1, m2, result);
        }

        stopwatch.Stop();
        Console.Error.WriteLine("Sequential loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds * 1);


        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            MultiplyMatricesParallel(m1, m2, result);
        }
        stopwatch.Stop();
        Console.Error.WriteLine("Parallel loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds * 1 + "\n");


        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            MulMa(m1o, rowCount, colCount, m2o, colCount2, resulto);
        }

        stopwatch.Stop();
        Console.Error.WriteLine("DllSequential loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds * 1);


        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            MulMaP(m1o, rowCount, colCount, m2o, colCount2, resulto);
        }
        stopwatch.Stop();
        Console.Error.WriteLine("DllParallel loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds * 1 + "\n");

        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            MulMaCachePtrAsm(m1o, rowCount, colCount, m2o, colCount2, resulto);
        }

        stopwatch.Stop();
        Console.Error.WriteLine("DllSequentialPtrAsm loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds * 1);


        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            MulMaCachePtrAsmPar(m1o, rowCount, colCount, m2o, colCount2, resulto);
        }
        stopwatch.Stop();
        Console.Error.WriteLine("DllParallelPtrAsm loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds * 1 + "\n");

        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            MultiplyMatricesSequentialT(m1o, colCount, m2o, colCount2, resulto);
        }

        stopwatch.Stop();
        Console.Error.WriteLine("SequentialT loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds * 1);


        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            MultiplyMatricesParallelT(m1o, colCount, m2o, colCount2, resulto);
        }
        stopwatch.Stop();
        Console.Error.WriteLine("ParallelT loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds * 1 + "\n");



        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            MultiplyMatricesSequentialJ(m1j, m2j, resultj);
        }

        stopwatch.Stop();
        Console.Error.WriteLine("SequentialJ loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds * 1);

        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            MultiplyMatricesParallelJ(m1j, m2j, resultj);
        }

        stopwatch.Stop();
        Console.Error.WriteLine("ParallelJ loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds * 1 + "\n");

        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            MultiplyMatricesSequentialO(m1o, colCount, m2o, colCount2, resulto);
        }

        stopwatch.Stop();
        Console.Error.WriteLine("SequentialO loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds * 1);

        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            MultiplyMatricesParallelO(m1o, colCount, m2o, colCount2, resulto);
        }

        stopwatch.Stop();
        Console.Error.WriteLine("ParallelO loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds * 1 + "\n");


        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            MultiplyMatricesSequentialP(m1o, colCount, m2o, colCount2, resulto);
        }

        stopwatch.Stop();
        Console.Error.WriteLine("SequentialP loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds * 1);

        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            MultiplyMatricesParallelP(m1o, colCount, m2o, colCount2, resulto);
        }

        stopwatch.Stop();
        Console.Error.WriteLine("ParallelP loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds * 1 + "\n");

        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            MultiplyMatricesSequentialPT(m1o, colCount, m2o, colCount2, resulto);
        }

        stopwatch.Stop();
        Console.Error.WriteLine("SequentialPT loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds * 1);

        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            MultiplyMatricesParallelPT(m1o, colCount, m2o, colCount2, resulto);
        }

        stopwatch.Stop();
        Console.Error.WriteLine("ParallelPT loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds * 1 + "\n");


        var n1 = Matrix<float>.Build.Dense(rowCount, colCount);
        var n2 = Matrix<float>.Build.Dense(colCount, colCount2);
        var nr = Matrix<float>.Build.Dense(rowCount, colCount2);

        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < colCount; j++)
            {
                n1[i, j] = m1[i, j];
            }
        }

        for (int i = 0; i < colCount; i++)
        {
            for (int j = 0; j < colCount2; j++)
            {
                n2[i, j] = m2[i, j];
            }
        }


        Control.UseManaged();

        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            nr = n1 * n2;
        }
        stopwatch.Stop();
        Console.Error.WriteLine("NumericsSeq loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds * 1);

        stopwatch.Restart();
        Parallel.For(0, ITER, i =>
        {
            nr = n1 * n2;
        });
        stopwatch.Stop();
        Console.Error.WriteLine("NumericsPar loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds * 1 + "\n");

        Control.UseBestProviders();

        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            nr = n1 * n2;
        }
        stopwatch.Stop();
        Console.Error.WriteLine("NumericsBestSeq loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds);

        Parallel.For(0, ITER, i =>
        {
            nr = n1 * n2;
        });
        stopwatch.Stop();
        Console.Error.WriteLine("NumericsBestPar time in milliseconds: {0}", stopwatch.ElapsedMilliseconds + "\n");

        Control.UseNativeMKL();

        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            nr = n1 * n2;
        }
        stopwatch.Stop();
        Console.Error.WriteLine("NumericsMKL loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds);

        Control.UseNativeOpenBLAS();

        stopwatch.Restart();
        for (int i = 0; i < ITER; i++)
        {
            nr = n1 * n2;
        }
        stopwatch.Stop();
        Console.Error.WriteLine("NumericsOpenBlas loop time in milliseconds: {0}", stopwatch.ElapsedMilliseconds);

        Control.UseManaged();


        // Keep the console window open in debug mode.
        Console.ReadKey();
    }
    #endregion

    #region Helper_Methods
    static float[,] InitializeMatrix(int rows, int cols, Random r)
    {
        float[,] matrix = new float[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                matrix[i, j] = r.Next(17) / 10.7f;
            }
        }
        return matrix;
    }
    #endregion

    #region Offer to Print
    private static void OfferToPrint(int rowCount, int colCount, float[,] matrix)
    {
        Console.Error.Write("Computation complete. Print results (y/n)? ");
        char c = Console.ReadKey(true).KeyChar;
        Console.Error.WriteLine(c);
        if (Char.ToUpperInvariant(c) == 'Y')
        {
            if (!Console.IsOutputRedirected) Console.WindowWidth = 180;
            Console.WriteLine();
            for (int x = 0; x < rowCount; x++)
            {
                Console.WriteLine("ROW {0}: ", x);
                for (int y = 0; y < colCount; y++)
                {
                    Console.Write("{0:#.##} ", matrix[x, y]);
                }
                Console.WriteLine();
            }
        }
    }
    #endregion
}

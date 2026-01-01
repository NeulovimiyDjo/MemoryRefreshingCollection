#include <iostream>
#include <ctime>	//time() for srand() and clock()
#include <cstdlib>	//rand()
#include <vector>
#include <thread>
#include <algorithm>
#include <windows.h>
#include <math.h>
#include <string>
#include <chrono>

#define EIGEN_USE_BLAS
#include <Eigen/Dense>

using Eigen::MatrixXf;
using namespace std;

float *InitializeMatrix(int rows, int cols)
{
  float *matrix = new float[rows * cols];

  for (int i = 0; i < rows; i++)
  {
    for (int j = 0; j < cols; j++)
    {
      matrix[i * cols + j] = rand() % 17 / 10.7f;
    }
  }
  return matrix;
}

float *InitializeMatrix(int rows, int cols, float value)
{
  float *matrix = new float[rows * cols];

  for (int i = 0; i < rows; i++)
  {
    for (int j = 0; j < cols; j++)
    {
      matrix[i * cols + j] = value;
    }
  }
  return matrix;
}

typedef int(__stdcall *MulFunc)(float *matA, int rowsA, int colsA, float *matB, int colsB, float *result);


#define ITER 10
#define r1 223
#define c1 931
#define c2 413
#define precision 0.1f
string CheckCorrectness(MulFunc f)
{
  bool correct = true;

  float v1 = (rand() % 16 + 1) / 10.7f;
  float v2 = (rand() % 16 + 1) / 10.7f;

  float *a = InitializeMatrix(r1, c1, v1);
  float *b = InitializeMatrix(c1, c2, v2);
  float *c = new float[r1 * c2];

  f(a, r1, c1, b, c2, c);

  for (int i = 0; i < r1; i++)
  {
    for (int j = 0; j < c2; j++)
    {
      if (fabs(c[i * c2 + j] - v1 * v2 * c1) >= precision)
      {
        correct = false;
      }
    }
  }

  delete[] a;
  delete[] b;
  delete[] c;

  if (correct)
  {
    return "Correct";
  }
  else
  {
    return "Incorrect";
  }
}

int runBenchmarks()
{
  int rowCount = r1;
  int colCount = c1;
  int colCount2 = c2;

  // initialize shit
  unsigned int start = clock();
  cout << "Program started" << endl;
  srand(static_cast<unsigned int>(time(0)));
  float *m1 = InitializeMatrix(rowCount, colCount);
  float *m2 = InitializeMatrix(colCount, colCount2);
  float *m3 = new float[rowCount * colCount2];
  cout << "Initialization took: " << clock() - start << endl;

  //load dll
  start = clock();

  HINSTANCE hGetProcIDDLL = LoadLibrary("mydll.dll");
  if (!hGetProcIDDLL)
  {
    cout << "Failed to load mydll.dll" << endl;
    return -1;
  }

  MulFunc MultiplyMatrices = (MulFunc)GetProcAddress(hGetProcIDDLL, "MulMa");
  MulFunc MultiplyMatricesValArr = (MulFunc)GetProcAddress(hGetProcIDDLL, "MulMaValArr");
  MulFunc MultiplyMatricesValArrP = (MulFunc)GetProcAddress(hGetProcIDDLL, "MulMaValArrP");
  MulFunc MultiplyMatricesSse = (MulFunc)GetProcAddress(hGetProcIDDLL, "MulMaSse");
  MulFunc MultiplyMatricesPar = (MulFunc)GetProcAddress(hGetProcIDDLL, "MulMaP");
  MulFunc MultiplyMatricesCache = (MulFunc)GetProcAddress(hGetProcIDDLL, "MulMaCache");
  MulFunc MultiplyMatricesCacheAsm = (MulFunc)GetProcAddress(hGetProcIDDLL, "MulMaCacheAsm");
  MulFunc MultiplyMatricesCachePtr = (MulFunc)GetProcAddress(hGetProcIDDLL, "MulMaCachePtr");
  MulFunc MultiplyMatricesCachePtrAsm = (MulFunc)GetProcAddress(hGetProcIDDLL, "MulMaCachePtrAsm");
  MulFunc MultiplyMatricesCachePtrAsmPar = (MulFunc)GetProcAddress(hGetProcIDDLL, "MulMaCachePtrAsmPar");

  if (!MultiplyMatrices) {
    cout << "Failed to load MultiplyMatrices" << endl;
    return -1;
  }

  if (!MultiplyMatricesValArr)
  {
    cout << "Failed to load MultiplyMatricesValArr" << endl;
    return -1;
  }

  if (!MultiplyMatricesValArrP)
  {
    cout << "Failed to load MultiplyMatricesValArrP" << endl;
    return -1;
  }

  if (!MultiplyMatricesSse)
  {
    cout << "Failed to load MultiplyMatricesSse" << endl;
    return -1;
  }

  if (!MultiplyMatricesPar)
  {
    cout << "Failed to load MultiplyMatricesPar" << endl;
    return -1;
  }

  if (!MultiplyMatricesCache)
  {
    cout << "Failed to load MultiplyMatricesCache" << endl;
    return -1;
  }

  if (!MultiplyMatricesCacheAsm)
  {
    cout << "Failed to load MultiplyMatricesCacheAsm" << endl;
    return -1;
  }

  if (!MultiplyMatricesCachePtr)
  {
    cout << "Failed to load MultiplyMatricesCachePtr" << endl;
    return -1;
  }

  if (!MultiplyMatricesCachePtrAsm)
  {
    cout << "Failed to load MultiplyMatricesCachePtrAsm" << endl;
    return -1;
  }

  if (!MultiplyMatricesCachePtrAsmPar)
  {
    cout << "Failed to load MultiplyMatricesCachePtrAsmPar" << endl;
    return -1;
  }

  cout << "Loading dll took: " << (clock() - start) << endl;

  // check correctness of functions
  start = clock();
  cout << "Correctness check started" << endl << endl;

  cout << "MultiplyMatrices: " << CheckCorrectness(MultiplyMatrices) << endl;
  cout << "MultiplyMatricesValArr: " << CheckCorrectness(MultiplyMatricesValArr) << endl;
  cout << "MultiplyMatricesValArrP: " << CheckCorrectness(MultiplyMatricesValArrP) << endl;
  //cout << "MultiplyMatricesSse: " << CheckCorrectness(MultiplyMatricesSse) << endl;
  cout << "MultiplyMatricesPar: " << CheckCorrectness(MultiplyMatricesPar) << endl;
  cout << "MultiplyMatricesCache: " << CheckCorrectness(MultiplyMatricesCache) << endl;
  cout << "MultiplyMatricesCacheAsm: " << CheckCorrectness(MultiplyMatricesCacheAsm) << endl;
  cout << "MultiplyMatricesCachePtr: " << CheckCorrectness(MultiplyMatricesCachePtr) << endl;
  cout << "MultiplyMatricesCachePtrAsm: " << CheckCorrectness(MultiplyMatricesCachePtrAsm) << endl;
  cout << "MultiplyMatricesCachePtrAsmPar: " << CheckCorrectness(MultiplyMatricesCachePtrAsmPar) << endl;

  cout << endl << "Checking correctness took: " << (clock() - start) << endl << endl;

  // do calculations transponse
  start = clock();
  for (int i = 0; i < ITER; i++)
  {
    MultiplyMatrices(m1, rowCount, colCount, m2, colCount2, m3);
  }
  cout << "Calculations transponse took: " << (clock() - start) * 1 << endl;

  // do calculations ValArr
  start = clock();
  for (int i = 0; i < ITER; i++)
  {
    MultiplyMatricesValArr(m1, rowCount, colCount, m2, colCount2, m3);
  }
  cout << "Calculations ValArr took: " << (clock() - start) * 1 << endl;

  // do calculations ValArrP
  start = clock();
  for (int i = 0; i < ITER; i++)
  {
    MultiplyMatricesValArrP(m1, rowCount, colCount, m2, colCount2, m3);
  }
  cout << "Calculations ValArrP took: " << (clock() - start) * 1 << endl;

  // do calculations Sse
  start = clock();
  for (int i = 0; i < ITER; i++)
  {
    //MultiplyMatricesSse(m1, rowCount, colCount, m2, colCount2, m3);
  }
  cout << "Calculations Sse took: " << (clock() - start) * 1 << endl;

  // do calculations parallel transponse
  start = clock();
  for (int i = 0; i < ITER; i++)
  {
    MultiplyMatricesPar(m1, rowCount, colCount, m2, colCount2, m3);
  }
  cout << "Calculations Parallel transponse took: " << (clock() - start) * 1 << endl;

  // do calculations cache
  start = clock();
  for (int i = 0; i < ITER; i++)
  {
    MultiplyMatricesCache(m1, rowCount, colCount, m2, colCount2, m3);
  }
  cout << "Calculations Cache took: " << (clock() - start) * 1 << endl;

  // do calculations cacheAsm
  start = clock();
  for (int i = 0; i < ITER; i++)
  {
    MultiplyMatricesCacheAsm(m1, rowCount, colCount, m2, colCount2, m3);
  }
  cout << "Calculations CacheAsm took: " << (clock() - start) * 1 << endl;

  // do calculations cachePtr
  start = clock();
  for (int i = 0; i < ITER; i++)
  {
    MultiplyMatricesCachePtr(m1, rowCount, colCount, m2, colCount2, m3);
  }
  cout << "Calculations CachePtr took: " << (clock() - start) * 1 << endl;

  // do calculations cachePtrAsm
  start = clock();
  for (int i = 0; i < ITER; i++)
  {
    MultiplyMatricesCachePtrAsm(m1, rowCount, colCount, m2, colCount2, m3);
  }
  cout << "Calculations CachePtrAsm took: " << (clock() - start) * 1 << endl;

  // do calculations cachePtrAsmPar
  start = clock();
  for (int i = 0; i < ITER; i++)
  {
    MultiplyMatricesCachePtrAsmPar(m1, rowCount, colCount, m2, colCount2, m3);
  }
  cout << "Calculations CachePtrAsmPar took: " << (clock() - start) * 1 << endl;

  FreeLibrary(hGetProcIDDLL);

  //------------------Eigen shit start
  //Declaration and initialization
  MatrixXf a(rowCount, colCount);
  MatrixXf b(colCount, colCount2);

  for (int i = 0; i < rowCount; i++)
  {
    for (int j = 0; j < colCount; j++)
    {
      a(i, j) = m1[i*colCount + j];
    }
  }

  for (int i = 0; i < rowCount; i++)
  {
    for (int j = 0; j < colCount2; j++)
    {
      a(i, j) = m2[i*colCount2 + j];
    }
  }

  MatrixXf r(rowCount, colCount2);
  //Calculations....

  start = clock();
  auto st = chrono::high_resolution_clock::now();
  for (int i = 0; i < ITER; i++)
  {
    r.noalias() = a * b;
  }
  cout << "Calculations Eigen took(high_resolution_clock): " << (chrono::high_resolution_clock::now() - st).count() << endl;
  cout << "Calculations Eigen took: " << (clock() - start) * 1 << endl;

  //------------------Eigen shit end

  //free memory
  start = clock();
  delete[] m1;
  delete[] m2;
  delete[] m3;
  cout << endl << "Deleting took: " << clock() - start << endl;

  return 0;
}

#ifdef _MSC_VER
#include "../QtGui/runner.h"
#endif

int main(int argc, char* argv[])
{
#ifdef _MSC_VER
  setEvent(runBenchmarks);
  
  int res = runGui(argc, argv);

  cin.get();
  return res;
#else
  runBenchmarks();

  cin.get();
  return 0;
#endif
}
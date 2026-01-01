#include <iostream>
#include <ctime>	//time() for srand() and clock()
#include <cstdlib>	//rand()
#include <vector>
#include <thread>
#include <algorithm>
#include <windows.h>
#include <math.h>
#include <string>
 
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

typedef int (__stdcall *MulFunc)(float *matA, int rowsA, int colsA, float *matB, int colsB, float *result);


#define r1 400
#define c1 2000
#define c2 500
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

int main()
{
	int rowCount = r1;
	int colCount = c1;
	int colCount2 = c2;

	// initialize shit
    unsigned int start = clock();
    cout << "Program started"<<endl;
	srand(time(0));
	float *m1 = InitializeMatrix(rowCount, colCount);
	float *m2 = InitializeMatrix(colCount, colCount2);
	float *m3 = new float[rowCount * colCount2];
	cout << "Initialization took: " << clock() - start << endl;
	
	//load dll
	start = clock();
	
	HINSTANCE hGetProcIDDLL = LoadLibrary("mydll.dll");
	if (!hGetProcIDDLL)
	{
		cout<<"Failed to load dll"<<endl;
		return -1;
	}
	
	MulFunc MultiplyMatrices = (MulFunc)GetProcAddress(hGetProcIDDLL,"_MulMa@24");
	MulFunc MultiplyMatricesSse = (MulFunc)GetProcAddress(hGetProcIDDLL,"_MulMaSse@24");
	MulFunc MultiplyMatricesPar = (MulFunc)GetProcAddress(hGetProcIDDLL,"_MulMaP@24");
	MulFunc MultiplyMatricesCache = (MulFunc)GetProcAddress(hGetProcIDDLL,"_MulMaCache@24");
	MulFunc MultiplyMatricesCacheAsm = (MulFunc)GetProcAddress(hGetProcIDDLL,"_MulMaCacheAsm@24");
	MulFunc MultiplyMatricesCachePtr = (MulFunc)GetProcAddress(hGetProcIDDLL,"_MulMaCachePtr@24");
	MulFunc MultiplyMatricesCachePtrAsm = (MulFunc)GetProcAddress(hGetProcIDDLL,"_MulMaCachePtrAsm@24");
	MulFunc MultiplyMatricesCachePtrAsmPar = (MulFunc)GetProcAddress(hGetProcIDDLL,"_MulMaCachePtrAsmPar@24");
	
	if (!MultiplyMatrices)
	{		
		cout<<"Failed to load MultiplyMatrices"<<endl;
		return -1;
	}
	
	if (!MultiplyMatricesSse)
	{		
		cout<<"Failed to load MultiplyMatricesSse"<<endl;
		return -1;
	}
	
	if (!MultiplyMatricesPar)
	{		
		cout<<"Failed to load MultiplyMatricesPar"<<endl;
		return -1;
	}
	
	if (!MultiplyMatricesCache)
	{		
		cout<<"Failed to load MultiplyMatricesCache"<<endl;
		return -1;
	}
	
	if (!MultiplyMatricesCacheAsm)
	{		
		cout<<"Failed to load MultiplyMatricesCacheAsm"<<endl;
		return -1;
	}
	
	if (!MultiplyMatricesCachePtr)
	{		
		cout<<"Failed to load MultiplyMatricesCachePtr"<<endl;
		return -1;
	}
	
	if (!MultiplyMatricesCachePtrAsm)
	{		
		cout<<"Failed to load MultiplyMatricesCachePtrAsm"<<endl;
		return -1;
	}
	
	if (!MultiplyMatricesCachePtrAsmPar)
	{		
		cout<<"Failed to load MultiplyMatricesCachePtrAsmPar"<<endl;
		return -1;
	}
	
	cout << "Loading dll took: " << (clock() - start) << endl;	
	
	// check correctness of functions
	start = clock();
	cout << "Correctness check started" << endl << endl;
	
	cout << "MultiplyMatrices: " << CheckCorrectness(MultiplyMatrices) << endl;
	cout << "MultiplyMatricesSse: " << CheckCorrectness(MultiplyMatricesSse) << endl;
	cout << "MultiplyMatricesPar: " << CheckCorrectness(MultiplyMatricesPar) << endl;
	cout << "MultiplyMatricesCache: " << CheckCorrectness(MultiplyMatricesCache) << endl;
	cout << "MultiplyMatricesCacheAsm: " << CheckCorrectness(MultiplyMatricesCacheAsm) << endl;
	cout << "MultiplyMatricesCachePtr: " << CheckCorrectness(MultiplyMatricesCachePtr) << endl;
	cout << "MultiplyMatricesCachePtrAsm: " << CheckCorrectness(MultiplyMatricesCachePtrAsm) << endl;
	cout << "MultiplyMatricesCachePtrAsmPar: " << CheckCorrectness(MultiplyMatricesCachePtrAsmPar) << endl;
	
	cout << endl << "Checking correctness took: " << (clock() - start) << endl << endl;
	
	// do calculations transponse
	start = clock();
	for (int i = 0; i < 1; i++)
	{
		MultiplyMatrices(m1, rowCount, colCount, m2, colCount2, m3);
	}
	cout << "Calculations transponse took: " << (clock() - start) * 100 << endl;
	
	// do calculations Sse
	start = clock();
	for (int i = 0; i < 1; i++)
	{
		MultiplyMatricesSse(m1, rowCount, colCount, m2, colCount2, m3);
	}
	cout << "Calculations Sse took: " << (clock() - start) * 100 << endl;
	
	// do calculations parallel transponse
	start = clock();
	for (int i = 0; i < 10; i++)
	{
		MultiplyMatricesPar(m1, rowCount, colCount, m2, colCount2, m3);
	}
	cout << "Calculations Parallel transponse took: " << (clock() - start) * 10 << endl;
	
	// do calculations cache
	start = clock();
	for (int i = 0; i < 1; i++)
	{
		MultiplyMatricesCache(m1, rowCount, colCount, m2, colCount2, m3);
	}
	cout << "Calculations Cache took: " << (clock() - start) * 100 << endl;
	
	// do calculations cacheAsm
	start = clock();
	for (int i = 0; i < 10; i++)
	{
		MultiplyMatricesCacheAsm(m1, rowCount, colCount, m2, colCount2, m3);
	}
	cout << "Calculations CacheAsm took: " << (clock() - start) * 10 << endl;
	
	// do calculations cachePtr
	start = clock();
	for (int i = 0; i < 1; i++)
	{
		MultiplyMatricesCachePtr(m1, rowCount, colCount, m2, colCount2, m3);
	}
	cout << "Calculations CachePtr took: " << (clock() - start) * 100 << endl;
	
	// do calculations cachePtrAsm
	start = clock();
	for (int i = 0; i < 10; i++)
	{
		MultiplyMatricesCachePtrAsm(m1, rowCount, colCount, m2, colCount2, m3);
	}
	cout << "Calculations CachePtrAsm took: " << (clock() - start) * 10 << endl;
	
	// do calculations cachePtrAsmPar
	start = clock();
	for (int i = 0; i < 100; i++)
	{
		MultiplyMatricesCachePtrAsmPar(m1, rowCount, colCount, m2, colCount2, m3);
	}
	cout << "Calculations CachePtrAsmPar took: " << (clock() - start) * 1 << endl;
	
	//free memory
	start = clock();
	delete[] m1;
	delete[] m2;
	delete[] m3;
	cout << endl << "Deleting took: " << clock() - start << endl;
	
    return 0;
}

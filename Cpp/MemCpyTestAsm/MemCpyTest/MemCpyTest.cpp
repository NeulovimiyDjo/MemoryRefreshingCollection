// MemCpyTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <iostream>
#include <vector>
#include <chrono>
#include <memory>

#include <math.h>
#include <Eigen/Dense>

using Eigen::VectorXf;

#define FUNC(f) void (*f)(To& to, From& from)
#define N 400*600
#define ITER 1500*5


using namespace std;


template<class To, class From>
void do_elementwise_copy(To& vec, From ptr)
{
	for (int i = 0; i < N; i++)
	{
		vec[i] = ptr[i];
	}
}

template<class To, class From>
void do_memcpy(To& vec, From ptr)
{
	memcpy(&vec[0], ptr, N*sizeof(float));
}


template<class To, class From>
void do_eigencopy(To& eigenvec, From ptr)
{
	new (&eigenvec) Eigen::Map<VectorXf>(ptr, N);
}

template<class To, class From>
void do_bench(FUNC(f), To& to, From from)
{
	auto start = chrono::high_resolution_clock::now();
	for (int k = 0; k < ITER; k++)
	{
		f(to, from);
	}
	auto end = chrono::high_resolution_clock::now();
	cout << chrono::duration_cast<chrono::milliseconds>(end - start).count() << '\n';
}

void initialize(float* arr)
{
	for (int i = 0; i < N; i++)
	{
		arr[i] = 1.0f;
	}
}


void aliasing_test(vector<float>& vec, VectorXf& eigenvec)
{
	auto start = chrono::high_resolution_clock::now();
	for (int k = 0; k < ITER; k++)
	{
		//float t = 3 * eigenvec[k / N];
		//float t = eigenvec[0];
		float* p = &eigenvec[0];
		//float* v = &vec[0];
		for (int i = 0; i < N; i++)
		{
			//vec[i] = 3 * eigenvec[k / N];

			vec[i] = p[i];
			/*
			vec[i+1] = p[i + 1];
			vec[i + 2] = p[i + 2];
			vec[i + 3] = p[i + 3];
			*/
			//vec[i] = t;
		}
	}
	auto end = chrono::high_resolution_clock::now();
	cout << chrono::duration_cast<chrono::milliseconds>(end - start).count() << '\n';
}

vector<float> aliasing_test_movable(vector<float> vec, VectorXf& eigenvec)
{
	auto start = chrono::high_resolution_clock::now();
	for (int k = 0; k < ITER; k++)
	{
		float* p = &eigenvec[0];
		for (int i = 0; i < N; i++)
		{
			vec[i] = eigenvec[i];
		}
	}
	auto end = chrono::high_resolution_clock::now();
	cout << chrono::duration_cast<chrono::milliseconds>(end - start).count() << '\n';
	return vec;
}

void funclol(VectorXf& eigenvec, unique_ptr<float> ptr)
{
	do_bench(do_eigencopy, eigenvec, ptr.release());
}

void test_memcpy()
{
	unique_ptr<float> ptr(new float[N]);
	float* p = new float[N];
	initialize(ptr.get());
	vector<float> vec(N,2);
	VectorXf eigenvec;

	do_bench(do_elementwise_copy, vec, ptr.get());
	do_bench(do_memcpy, vec, ptr.get());
	funclol(eigenvec, move(ptr));

	aliasing_test(vec, eigenvec);

	auto vec2 = aliasing_test_movable(move(vec), eigenvec);

	delete p;
}

struct TestDestuctors
{
	int size;
	//TestDestuctors() { cout << "Default Constructor\n"; }
	TestDestuctors(int sz) { cout << "Int Constructor\n"; size = sz; }
	~TestDestuctors() { cout << "Destructor\n"; }
};

void test_new()
{
	// built in types dont call default constructor unless you explicitly order it
	auto start = chrono::high_resolution_clock::now();
	double* p = new double[1'000'000'000]();
	auto end = chrono::high_resolution_clock::now();
	cout << chrono::duration_cast<chrono::milliseconds>(end - start).count() << '\n';
	cout << p[3] << '\n';
	delete[] p;

	// cant directly create array of objects without default constructor e.g. TestDestuctors* arr = new TestDestuctors[3];
	char* place = new char[sizeof(TestDestuctors) * 3];
	TestDestuctors* arr = reinterpret_cast<TestDestuctors*>(place);

	for (int i = 0; i < 3; i++)
	{
		new (arr+i) TestDestuctors(i);
	}

	for (int i = 0; i < 3; i++)
	{
		arr[i].~TestDestuctors();
	}

	delete[] place;	//or delete[] reinterpret_cast<char*>(arr)

	// on stack
	TestDestuctors arr2[3] { *(new TestDestuctors(1)), *(new TestDestuctors(1)), *(new TestDestuctors(1)) };

}

int main()
{
	test_memcpy();
	
	//test_new();

	

	cin.get();
    return 0;
}


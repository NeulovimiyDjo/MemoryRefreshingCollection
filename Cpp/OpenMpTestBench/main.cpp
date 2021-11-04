#include <iostream>
#include <chrono>
#include <cmath>
#include <vector>

#include <thread>
#include <algorithm>
#include <future>

int main(int argc, char const *argv[])
{
	auto start = std::chrono::high_resolution_clock::now();
	#pragma omp parallel
	std::cout<<"Hello world\n";
	auto end = std::chrono::high_resolution_clock::now();
	std::cout<<"Time1: "<<std::chrono::duration_cast<std::chrono::milliseconds>(end-start).count()<<'\n';

	// stupid ad hoc handmade parallel aggregation
	constexpr int N {20'000'000};
	
	std::vector<double> partial_sums(8);
	double sum{0};

	constexpr int Nlocal = N/8;

	start = std::chrono::high_resolution_clock::now();

	#pragma omp parallel for
	for (int i = 0; i < N; ++i) {
		partial_sums[i/Nlocal] += pow(i,0.37);
	}
	for (int i = 0; i < 8; ++i) {
		sum += partial_sums[i];
	}

	end = std::chrono::high_resolution_clock::now();
	std::cout<<"Time2: "<<std::chrono::duration_cast<std::chrono::milliseconds>(end-start).count()<<'\n';
	std::cout<<"Result: "<<sum<<'\n';

	//normal parallel aggregation
	sum = 0;

	start = std::chrono::high_resolution_clock::now();

	#pragma omp parallel for reduction(+:sum)
	for (int i = 0; i < N; ++i) {
		sum += pow(i,0.37);
	}

	end = std::chrono::high_resolution_clock::now();
	std::cout<<"Time3: "<<std::chrono::duration_cast<std::chrono::milliseconds>(end-start).count()<<'\n';
	std::cout<<"Result: "<<sum<<'\n';


	// handmade threads with vector
	std::fill(partial_sums.begin(),partial_sums.end(),0);
	sum = 0;

	start = std::chrono::high_resolution_clock::now();

	const size_t nthreads = std::thread::hardware_concurrency();
	std::vector<std::thread> threads(nthreads);
	for (int t = 0; t < nthreads; ++t) {
		threads[t] = std::thread(
			[t,nthreads,N,&partial_sums]()
			{
				for (int i = t * N / nthreads; i < (t + 1) * N / nthreads; ++i) {
					partial_sums[t] += pow(i,0.37);			
				}
			});
	}

	for_each(threads.begin(), threads.end(), [](std::thread& x) {x.join(); });

	for (int i = 0; i < 8; ++i) {
		sum += partial_sums[i];
	}

	end = std::chrono::high_resolution_clock::now();
	std::cout<<"Time4: "<<std::chrono::duration_cast<std::chrono::milliseconds>(end-start).count()<<'\n';
	std::cout<<"Result: "<<sum<<'\n';


	// handmade threads with array

	double arr[8];
	std::fill(&arr[0],&arr[8],0);
	sum = 0;
	
	start = std::chrono::high_resolution_clock::now();

	for (int t = 0; t < nthreads; ++t) {
		threads[t] = std::thread(
			[t,nthreads,N,&arr]()
			{
				for (int i = t * N / nthreads; i < (t + 1) * N / nthreads; ++i) {
					arr[t] += pow(i,0.37);			
				}
			});
	}

	for_each(threads.begin(), threads.end(), [](std::thread& x) {x.join(); });

	for (int i = 0; i < 8; ++i) {
		sum += arr[i];
	}

	end = std::chrono::high_resolution_clock::now();
	std::cout<<"Time5: "<<std::chrono::duration_cast<std::chrono::milliseconds>(end-start).count()<<'\n';
	std::cout<<"Result: "<<sum<<'\n';


	// async

	std::fill(&arr[0],&arr[8],0);
	sum = 0;
	
	start = std::chrono::high_resolution_clock::now();
	std::future<double> res[8];
	for (int t = 0; t < nthreads; ++t) {
		res[t] = std::async(
			[t,nthreads,N,&arr]()
			{
				for (int i = t * N / nthreads; i < (t + 1) * N / nthreads; ++i) {
					arr[t] += pow(i,0.37);			
				}
				return arr[t];
			});
	}

	for (int i = 0; i < 8; ++i) {
		sum += res[i].get();
	}

	end = std::chrono::high_resolution_clock::now();
	std::cout<<"Time6: "<<std::chrono::duration_cast<std::chrono::milliseconds>(end-start).count()<<'\n';
	std::cout<<"Result: "<<sum<<'\n';


	std::cin.get();
	return 0;
}
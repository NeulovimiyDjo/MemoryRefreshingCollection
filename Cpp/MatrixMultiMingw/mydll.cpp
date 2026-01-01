
#include <vector>
#include <thread>
#include <algorithm>
#include <emmintrin.h>
#include <valarray>
#include <numeric>

#define SM (64 * sizeof (char) / sizeof (float))

using namespace std;

extern "C"
{

	void __declspec(dllexport) __stdcall MulMa(float* matA, int rowsA, int colsA, float *matB, int colsB, float *result)
	{
		float *transpB = new float[colsA * colsB];
		for (int i = 0; i < colsA; i++)
		{
			for (int j = 0; j < colsB; j++)
			{
				transpB[i * colsB + j] = matB[j * colsA + i];
			}
		}

		for (int i = 0; i < rowsA; i++)
		{
			for (int j = 0; j < colsB; j++)
			{
				float temp = 0;
				for (int k = 0; k < colsA; k++)
				{
					temp += matA[i * colsA + k] * transpB[j * colsA + k];
				}
				result[i * colsB + j] = temp;
			}
		}

		delete[] transpB;
	}

	void __declspec(dllexport) __stdcall MulMaSse(float* matA, int rowsA, int colsA, float *matB, int colsB, float *result)
	{
		int colsR = colsB;
		int colsABext = colsA + (4 - colsA % 4) % 4;
		int colsRext = colsB + (4 - colsB % 4) % 4;
		//float *tmpr = new float[rowsA * colsRext];
		float *tmpr = (float*)_mm_malloc(rowsA * colsRext * sizeof(float), 64);
		//float *tmpa = new float[rowsA * colsABext];
		float *tmpa = (float*)_mm_malloc(rowsA * colsABext * sizeof(float), 64);
		//float *transpB = new float[colsB * colsABext];
		float *transpB = (float*)_mm_malloc(colsB * colsABext * sizeof(float), 64);

		for (int i = 0; i < rowsA; i++)
		{
			for (int j = 0; j < colsRext; j++)
			{
				if (j < colsR)
				{
					tmpr[i * colsRext + j] = 0.0f;
				}
			}
		}

		for (int i = 0; i < rowsA; i++)
		{
			for (int j = 0; j < colsABext; j++)
			{
				if (j < colsA)
				{
					tmpa[i * colsABext + j] = matA[i * colsA + j];
				}
				else
				{
					tmpa[i * colsABext + j] = 0.0f;
				}
			}
		}

		for (int i = 0; i < colsB; i++)
		{
			for (int j = 0; j < colsABext; j++)
			{
				if (j < colsA)
				{
					transpB[i * colsABext + j] = matB[j * colsB + i];
				}
				else
				{
					transpB[i * colsABext + j] = 0.0f;
				}
			}
		}

		float temp[4];
		for (int i = 0; i < rowsA; i++)
		{
			for (int j = 0; j < colsR; j++)
			{
				for (int p = 0; p < 4; p++)
				{
					temp[p] = 0.0f;
				}

				for (int k = 0; k < colsA; k += 4)
				{
					__m128 ma = _mm_load_ps(&tmpa[i * colsA + k]);
					__m128 mb = _mm_load_ps(&transpB[j * colsABext + k]);
					__m128 tmp = _mm_load_ps(&temp[0]);
					_mm_store_ps(&temp[0], _mm_add_ps(_mm_mul_ps(ma, mb), tmp));
				}

				for (int p = 0; p < 4; p++)
				{
					tmpr[i * colsRext + j] += temp[p];
				}
			}
		}

		for (int i = 0; i < rowsA; i++)
		{
			for (int j = 0; j < colsRext; j++)
			{
				if (j < colsR)
				{
					result[i * colsR + j] = tmpr[i * colsRext + j];
				}
			}
		}

		_mm_free(tmpr);
		_mm_free(tmpa);
		_mm_free(transpB);

		//delete[] tmpr;
		//delete[] tmpa;
		//delete[] transpB;	
	}

	void __declspec(dllexport) __stdcall MulMaP(float *matA, int rowsA, int colsA, float *matB, int colsB, float *result)
	{
		float *transpB = new float[colsA * colsB];
		for (int i = 0; i < colsA; i++)
		{
			for (int j = 0; j < colsB; j++)
			{
				transpB[i * colsB + j] = matB[j * colsA + i];
			}
		}

		const size_t nthreads = thread::hardware_concurrency();
		vector<thread> threads(nthreads);
		for (int t = 0; t < nthreads; t++)
		{
			threads[t] = thread(
				[](float *matA, float *transpB, float *result, int rowsA, int colsA, int colsB, int t, int nthreads)
			{
				for (int i = t * rowsA / nthreads; i < (t + 1) * rowsA / nthreads; i++)
				{
					for (int j = 0; j < colsB; j++)
					{
						float temp = 0;
						for (int k = 0; k < colsA; k++)
						{
							temp += matA[i * colsA + k] * transpB[j * colsA + k];
						}
						result[i * colsB + j] = temp;
					}
				}
			}, matA, transpB, result, rowsA, colsA, colsB, t, nthreads);
		}

		for_each(threads.begin(), threads.end(), [](thread& x) {x.join(); });

		delete[] transpB;
	}

	void __declspec(dllexport) __stdcall MulMaCache(float *matA, int rowsA, int colsA, float *matB, int colsB, float *result)
	{
		for (int i = 0; i < rowsA; i++)
		{
			for (int j = 0; j < colsB; j++)
			{
				result[i * colsB + j] = 0.0f;
			}
		}

		for (int i = 0; i < rowsA; i += SM)
		{
			for (int j = 0; j < colsB; j += SM)
			{
				for (int k = 0; k < colsA; k += SM)
				{
					for (int i2 = i; i2 < i + SM && i2 < rowsA; i2++)
					{
						for (int k2 = k; k2 < k + SM && k2 < colsA; k2++)
						{
							for (int j2 = j; j2 < j + SM && j2 < colsB; j2++)
							{
								result[i2 * colsB + j2] += matA[i2 * colsA + k2] * matB[k2 * colsB + j2];
							}
						}
					}
				}
			}
		}

	}

	void __declspec(dllexport) __stdcall MulMaCacheAsm(float *matA, int rowsA, int colsA, float *matB, int colsB, float *result)
	{
		int colsBext = colsB + (4 - colsB % 4) % 4;
		float *tmpr = new float[rowsA * colsBext];
		float *tmpa = matA;
		float *tmpb = new float[colsA * colsBext];


		for (int i = 0; i < rowsA; i++)
		{
			for (int j = 0; j < colsBext; j++)
			{
				if (j < colsB)
				{
					tmpr[i * colsBext + j] = 0.0f;
				}
			}
		}

		for (int i = 0; i < colsA; i++)
		{
			for (int j = 0; j < colsBext; j++)
			{
				if (j < colsB)
				{
					tmpb[i * colsBext + j] = matB[i * colsB + j];
				}
			}
		}

		for (int i = 0; i < rowsA; i += SM)
		{
			for (int j = 0; j < colsB; j += SM)
			{
				for (int k = 0; k < colsA; k += SM)
				{
					for (int i2 = i; i2 < i + SM && i2 < rowsA; i2++)
					{
						//_mm_prefetch(&tmpa[i2 * colsA + 16], _MM_HINT_NTA);
						for (int k2 = k; k2 < k + SM && k2 < colsA; k2++)
						{
							__m128 m1s = _mm_load_ss(&tmpa[i2 * colsA + k2]);
							m1s = _mm_unpacklo_ps(m1s, m1s);
							m1s = _mm_unpacklo_ps(m1s, m1s);
							for (int j2 = j; j2 < j + SM && j2 < colsB; j2 += 4)
							{
								__m128 m2 = _mm_load_ps(&tmpb[k2 * colsBext + j2]);
								__m128 r2 = _mm_load_ps(&tmpr[i2 * colsBext + j2]);
								_mm_store_ps(&tmpr[i2 * colsBext + j2], _mm_add_ps(_mm_mul_ps(m2, m1s), r2));
							}
						}
					}
				}
			}
		}

		for (int i = 0; i < rowsA; i++)
		{
			for (int j = 0; j < colsBext; j++)
			{
				if (j < colsB)
				{
					result[i * colsB + j] = tmpr[i * colsBext + j];
				}
			}
		}

		delete[] tmpr;
		delete[] tmpb;
	}

	void __declspec(dllexport) __stdcall MulMaCachePtr(float *matA, int rowsA, int colsA, float *matB, int colsB, float *result)
	{
		for (int i = 0; i < rowsA; i++)
		{
			for (int j = 0; j < colsB; j++)
			{
				result[i * colsB + j] = 0.0f;
			}
		}

		int i2, j2, k2;
		float *r, *a, *b;
		for (int i = 0; i < rowsA; i += SM)
		{
			for (int j = 0; j < colsB; j += SM)
			{
				for (int k = 0; k < colsA; k += SM)
				{
					for (i2 = 0, r = &result[i * colsB + j], a = &matA[i * colsA + k];
						i2 < SM && i + i2 < rowsA; i2++, r += colsB, a += colsA)
					{
						for (k2 = 0, b = &matB[k * colsB + j];
							k2 < SM && k + k2 < colsA; k2++, b += colsB)
						{
							for (j2 = 0; j2 < SM && j + j2 < colsB; j2++)
							{
								r[j2] += a[k2] * b[j2];
							}
						}
					}
				}
			}
		}

	}

	void __declspec(dllexport) __stdcall MulMaCachePtrAsm(float *matA, int rowsA, int colsA, float *matB, int colsB, float *result)
	{
		int colsBext = colsB + (4 - colsB % 4) % 4;
		float *tmpr = new float[rowsA * colsBext];
		float *tmpa = matA;
		float *tmpb = new float[colsA * colsBext];

		for (int i = 0; i < rowsA; i++)
		{
			for (int j = 0; j < colsBext; j++)
			{
				if (j < colsB)
				{
					tmpr[i * colsBext + j] = 0.0f;
				}
			}
		}

		for (int i = 0; i < colsA; i++)
		{
			for (int j = 0; j < colsBext; j++)
			{
				if (j < colsB)
				{
					tmpb[i * colsBext + j] = matB[i * colsB + j];
				}
			}
		}


		int i2, j2, k2;
		float *r, *a, *b;
		for (int i = 0; i < rowsA; i += SM)
		{
			for (int j = 0; j < colsB; j += SM)
			{
				for (int k = 0; k < colsA; k += SM)
				{
					for (i2 = 0, r = &tmpr[i * colsBext + j], a = &tmpa[i * colsA + k];
						i2 < SM && i + i2 < rowsA; i2++, r += colsBext, a += colsA)
					{
						//_mm_prefetch(&a[16], _MM_HINT_NTA);
						for (k2 = 0, b = &tmpb[k * colsBext + j];
							k2 < SM && k + k2 < colsA; k2++, b += colsBext)
						{
							__m128 m1s = _mm_load_ss(&a[k2]);
							m1s = _mm_unpacklo_ps(m1s, m1s);
							m1s = _mm_unpacklo_ps(m1s, m1s);
							for (j2 = 0; j2 < SM && j + j2 < colsB; j2 += 4)
							{
								__m128 m2 = _mm_load_ps(&b[j2]);
								__m128 r2 = _mm_load_ps(&r[j2]);
								_mm_store_ps(&r[j2], _mm_add_ps(_mm_mul_ps(m2, m1s), r2));
							}
						}
					}
				}
			}
		}

		for (int i = 0; i < rowsA; i++)
		{
			for (int j = 0; j < colsBext; j++)
			{
				if (j < colsB)
				{
					result[i * colsB + j] = tmpr[i * colsBext + j];
				}
			}
		}

		delete[] tmpr;
		delete[] tmpb;

	}

	void __declspec(dllexport) __stdcall MulMaCachePtrAsmPar(float *matA, int rowsA, int colsA, float *matB, int colsB, float *result)
	{
		int colsBext = colsB + (4 - colsB % 4) % 4;
		float *tmpr = new float[rowsA * colsBext];
		float *tmpa = matA;
		float *tmpb = new float[colsA * colsBext];

		for (int i = 0; i < rowsA; i++)
		{
			for (int j = 0; j < colsBext; j++)
			{
				if (j < colsB)
				{
					tmpr[i * colsBext + j] = 0.0f;
				}
			}
		}

		for (int i = 0; i < colsA; i++)
		{
			for (int j = 0; j < colsBext; j++)
			{
				if (j < colsB)
				{
					tmpb[i * colsBext + j] = matB[i * colsB + j];
				}
			}
		}


		const size_t nthreads = thread::hardware_concurrency();
		vector<thread> threads(nthreads);
		for (int t = 0; t < nthreads; t++)
		{
			threads[t] = thread(
				[](float *tmpa, float *tmpb, float *tmpr, int rowsA, int colsA, int colsB, int colsBext, int t, int nthreads)
			{
				int i2, j2, k2;
				float *r, *a, *b;
				int x = 0;
				if (rowsA % SM > 0)
				{
					x = 1;
				}
				for (int i = t * (rowsA / SM + x) / nthreads * SM;
					i < (t + 1) * (rowsA / SM + x) / nthreads * SM; i += SM)
				{
					for (int j = 0; j < colsB; j += SM)
					{
						for (int k = 0; k < colsA; k += SM)
						{
							for (i2 = 0, r = &tmpr[i * colsBext + j], a = &tmpa[i * colsA + k];
								i2 < SM && i + i2 < rowsA; i2++, r += colsBext, a += colsA)
							{
								_mm_prefetch((const char*)&a[16], _MM_HINT_NTA);
								for (k2 = 0, b = &tmpb[k * colsBext + j];
									k2 < SM && k + k2 < colsA; k2++, b += colsBext)
								{
									__m128 m1s = _mm_load_ss(&a[k2]);
									m1s = _mm_unpacklo_ps(m1s, m1s);
									m1s = _mm_unpacklo_ps(m1s, m1s);
									for (j2 = 0; j2 < SM && j + j2 < colsB; j2 += 4)
									{
										__m128 m2 = _mm_load_ps(&b[j2]);
										__m128 r2 = _mm_load_ps(&r[j2]);
										_mm_store_ps(&r[j2], _mm_add_ps(_mm_mul_ps(m2, m1s), r2));
									}
								}
							}
						}
					}
				}
			}, tmpa, tmpb, tmpr, rowsA, colsA, colsB, colsBext, t, nthreads);
		}

		for_each(threads.begin(), threads.end(), [](thread& x) {x.join(); });


		for (int i = 0; i < rowsA; i++)
		{
			for (int j = 0; j < colsBext; j++)
			{
				if (j < colsB)
				{
					result[i * colsB + j] = tmpr[i * colsBext + j];
				}
			}
		}

		delete[] tmpr;
		delete[] tmpb;

	}

	void __declspec(dllexport) __stdcall MulMaValArr(float* matA, int rowsA, int colsA, float *matB, int colsB, float *result)
	{
		float *transpB = new float[colsA * colsB];
		for (int i = 0; i < colsA; i++)
		{
			for (int j = 0; j < colsB; j++)
			{
				transpB[i * colsB + j] = matB[j * colsA + i];
			}
		}

		valarray<float> va{ matA, static_cast<size_t>(rowsA*colsA) };
		valarray<float> vb{ transpB, static_cast<size_t>(colsA*colsB) };
		valarray<float> vr(0.0f, rowsA*colsB);

		for (int i = 0; i < rowsA; i++)
		{
			for (int j = 0; j < colsB; j++)
			{
				//valarray<float> vtmp(rowsA*colsA);
				//vtmp[slice(i*colsA, colsA, 1)] += va[slice(i*colsA, colsA, 1)];
				//vtmp[slice(i*colsA, colsA, 1)] *= vb[slice(j*colsA, colsA, 1)];
				//vr[i*colsB + j] = vtmp.sum();
				vr[i*colsB + j] = inner_product(&va[i*colsA], &va[(i+1)*colsA], &vb[j*colsA], 0.0f);
			}
		}

		for (int i = 0; i < rowsA*colsB; i++)
		{
			result[i] = vr[i];
		}

		delete(transpB);
	}

	void __declspec(dllexport) __stdcall MulMaValArrP(float* matA, int rowsA, int colsA, float *matB, int colsB, float *result)
	{
		float *transpB = new float[colsA * colsB];
		for (int i = 0; i < colsA; i++)
		{
			for (int j = 0; j < colsB; j++)
			{
				transpB[i * colsB + j] = matB[j * colsA + i];
			}
		}

		valarray<float> va{ matA, static_cast<size_t>(rowsA*colsA) };
		valarray<float> vb{ transpB, static_cast<size_t>(colsA*colsB) };
		valarray<float> vr(0.0f, rowsA*colsB);

		const size_t nthreads = thread::hardware_concurrency();
		vector<thread> threads(nthreads);
		for (int t = 0; t < nthreads; t++)
		{
			threads[t] = thread(
				[&vr,va,vb,rowsA,colsA,colsB,t,nthreads]()
			{
				for (int i = t * rowsA / nthreads; i < (t + 1) * rowsA / nthreads; i++)
				{
					for (int j = 0; j < colsB; j++)
					{
						//valarray<float> vtmp(rowsA*colsA);
						//vtmp[slice(i*colsA, colsA, 1)] += va[slice(i*colsA, colsA, 1)];
						//vtmp[slice(i*colsA, colsA, 1)] *= vb[slice(j*colsA, colsA, 1)];
						//vr[i*colsB + j] = vtmp.sum();
						vr[i*colsB + j] = inner_product(&va[i*colsA], &va[(i + 1)*colsA], &vb[j*colsA], 0.0f);
					}
				}
			});
		}

		for_each(threads.begin(), threads.end(), [](thread& x) {x.join(); });

		for (int i = 0; i < rowsA*colsB; i++)
		{
			result[i] = vr[i];
		}

		delete(transpB);
	}
}


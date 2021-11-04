#ifdef __cplusplus
extern "C" {
#endif
	extern __declspec(dllexport) int worksfinally(int x1, int x2);

	extern __declspec(dllexport) double multi3wrapper(CppClass* cls);

	extern __declspec(dllexport) CppClass* createclass(int c1, int c2, double x3);

	extern __declspec(dllexport) void deleteclass(CppClass* cls);

	extern __declspec(dllexport) void printclass(CppClass* cls);
#ifdef __cplusplus
}
#endif
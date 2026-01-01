#include "Exports.h"

#include "../InjectorLib/GetProcessId.h"
#include "../InjectorLib/LoadLibraryInject.h"
#include "../InjectorLib/ManualMapInject.h"


DWORD __stdcall GetProcessIdWrapper(const char* szExeName) {
	return GetProcessId(szExeName);
}

bool __stdcall LoadLibraryInjectWrapper(DWORD ID, const char* dll) {
	return LoadLibraryInject(ID, dll);
}

bool __stdcall ManualMapInjectWrapper(DWORD ID, const char* dll) {
	return ManualMapInject(ID, dll);
}

#pragma once

#include <Windows.h>

#define EXPORT(returntype) __declspec(dllexport) returntype __stdcall

extern "C" {
	EXPORT(DWORD) GetProcessIdWrapper(const char* targetProcessName);

	EXPORT(bool) LoadLibraryInjectWrapper(DWORD targetProcessID, const char* dllFullName);

	EXPORT(bool) ManualMapInjectWrapper(DWORD targetProcessID, const char* dllFullName);
}
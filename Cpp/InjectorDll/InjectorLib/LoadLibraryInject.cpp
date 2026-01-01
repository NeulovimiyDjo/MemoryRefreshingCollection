#include <windows.h>
#include <fstream>

#include "LoadLibraryInject.h"
#include "SetDebugPrivilege.h"
#include "ShowError.h"


bool LoadLibraryInject(DWORD ID, const char* dll)
{
    std::ifstream test(dll);
    if (!test)
    {
        out::ShowError("DLL not found");
        return false;
    }

    if (!ID)
        return false;

    HANDLE hProc;
    LPVOID pParamMemory;
    LPVOID loadLibraryFunc;

    if (!SetDebugPrivilege()) {
        out::ShowError("Failed to elevate to debug privilege");
        return false;
    }

    hProc = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION, FALSE, ID);
    if (!hProc)
    {
        out::ShowError("Failed to open process");
        return false;
    }

    loadLibraryFunc = (LPVOID)GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
    if (!loadLibraryFunc)
    {
        out::ShowError("Failed to get LoadLibraryA function address");
        CloseHandle(hProc);
        return false;
    }

    pParamMemory = (LPVOID)VirtualAllocEx(hProc, NULL, strlen(dll) + 1, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
    if (!pParamMemory)
    {
        out::ShowError("Failed to allocate memory for LoadLibraryA parameter");
        CloseHandle(hProc);
        return false;
    }

    if (!WriteProcessMemory(hProc, (LPVOID)pParamMemory, dll, strlen(dll) + 1, NULL))
    {
        out::ShowError("Failed to write for LoadLibraryA parameter");
        VirtualFreeEx(hProc, (LPVOID)pParamMemory, 0, MEM_RELEASE);
        CloseHandle(hProc);
        return false;
    }

    auto h = CreateRemoteThread(hProc, NULL, NULL, (LPTHREAD_START_ROUTINE)loadLibraryFunc, (LPVOID)pParamMemory, NULL, NULL);
    if (!h)
    {
        out::ShowError("Failed to create remote thread");
        VirtualFreeEx(hProc, (LPVOID)pParamMemory, 0, MEM_RELEASE);
        CloseHandle(hProc);
        return false;
    }

    VirtualFreeEx(hProc, (LPVOID)pParamMemory, 0, MEM_RELEASE);
    CloseHandle(hProc);

    return true;
}
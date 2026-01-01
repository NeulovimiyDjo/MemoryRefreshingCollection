#include <windows.h>
#include <iostream>
#include <fstream>
#include <TlHelp32.h>

#include "ManualMapInject.h"
#include "SetDebugPrivilege.h"
#include "ShowError.h"


using f_LoadLibraryA = HINSTANCE(WINAPI*)(const char* lpLibFilename);
using f_GetProcAddress = UINT_PTR(WINAPI*)(HINSTANCE hModule, const char* lpProcName);
using f_DLL_ENTRY_POINT = BOOL(WINAPI*)(void* hDll, DWORD dwReason, void* pReserved);

struct MANUAL_MAPPING_DATA
{
	f_LoadLibraryA ploadLibraryA;
	f_GetProcAddress pGetProcAddress;
	HINSTANCE hMod;
};


void __stdcall Shellcode(MANUAL_MAPPING_DATA* pData);

bool ManualMapInject(DWORD ID, const char* szDllFile) {
	if (!ID)
		return false;

	BYTE* pSrcData = nullptr;
	IMAGE_NT_HEADERS* pOldNtHeader = nullptr;
	IMAGE_OPTIONAL_HEADER* pOldOptHeader = nullptr;
	IMAGE_FILE_HEADER* pOldFileHeader = nullptr;
	BYTE* pTargetBase = nullptr;

	if (!SetDebugPrivilege()) {
		out::ShowError("Failed to elevate to debug privilege");
		return false;
	}

	HANDLE hProc = OpenProcess(PROCESS_ALL_ACCESS, FALSE, ID);
	if (!hProc) {
		out::ShowError("Failed to open process");
		return false;
	}


	if (GetFileAttributesA(szDllFile) == INVALID_FILE_ATTRIBUTES) {
		out::ShowError("DLL not found");
		CloseHandle(hProc);
		return false;
	}

	std::ifstream File(szDllFile, std::ios::binary | std::ios::ate);
	if (File.fail()) {
		out::ShowError("Failed to open dll");
		File.close();
		CloseHandle(hProc);
		return false;
	}

	auto fileSize = File.tellg();

	if (fileSize < 0x1000) {
		out::ShowError("DLL too small");
		File.close();
		CloseHandle(hProc);
		return false;
	}

	pSrcData = new BYTE[static_cast<UINT_PTR>(fileSize)];
	if (!pSrcData) {
		File.close();
		out::ShowError("Memory allocation for dll failed");
		CloseHandle(hProc);
		return false;
	}

	File.seekg(0, std::ios::beg);
	File.read(reinterpret_cast<char*>(pSrcData), fileSize);
	File.close();

	if (reinterpret_cast<IMAGE_DOS_HEADER*>(pSrcData)->e_magic != 0x5A4D) {
		delete[] pSrcData;
		out::ShowError("Invalid File");
		CloseHandle(hProc);
		return false;
	}

	pOldNtHeader = reinterpret_cast<IMAGE_NT_HEADERS*>(pSrcData + reinterpret_cast<IMAGE_DOS_HEADER*>(pSrcData)->e_lfanew);
	pOldOptHeader = &pOldNtHeader->OptionalHeader;
	pOldFileHeader = &pOldNtHeader->FileHeader;

#ifdef _WIN64
	if (pOldFileHeader->Machine != IMAGE_FILE_MACHINE_AMD64) {
		out::ShowError("Invalid DLL format");
		delete[] pSrcData;
		CloseHandle(hProc);
		return false;
	}
#else
	if (pOldFileHeader->Machine != IMAGE_FILE_MACHINE_I386) {
		out::ShowError("Invalid DLL format");
		delete[] pSrcData;
		CloseHandle(hProc);
		return false;
	}
#endif // _WIN64

	pTargetBase = reinterpret_cast<BYTE*>(VirtualAllocEx(hProc, reinterpret_cast<void*>(pOldOptHeader->ImageBase), pOldOptHeader->SizeOfImage, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE));

	if (!pTargetBase) {
		pTargetBase = reinterpret_cast<BYTE*>(VirtualAllocEx(hProc, nullptr, pOldOptHeader->SizeOfImage, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE));
	}

	if (!pTargetBase) {
		out::ShowError("Memory allocation failed");
		delete[] pSrcData;
		CloseHandle(hProc);
		return false;
	}

	MANUAL_MAPPING_DATA data{ 0 };
	data.ploadLibraryA = LoadLibraryA;
	data.pGetProcAddress = reinterpret_cast<f_GetProcAddress>(GetProcAddress);

	auto* pSectionHeader = IMAGE_FIRST_SECTION(pOldNtHeader);


	for (UINT i = 0; i != pOldFileHeader->NumberOfSections; i++, pSectionHeader++) {
		if (pSectionHeader->SizeOfRawData) {
			if (!WriteProcessMemory(hProc, pTargetBase + pSectionHeader->VirtualAddress, pSrcData + pSectionHeader->PointerToRawData, pSectionHeader->SizeOfRawData, nullptr)) {
				out::ShowError("Failed to write dll sections to target process memory");
				delete[] pSrcData;
				VirtualFreeEx(hProc, pTargetBase, 0, MEM_RELEASE);
				CloseHandle(hProc);
				return false;
			}
		}
	}

	memcpy(pSrcData, &data, sizeof(data));
	if (!WriteProcessMemory(hProc, pTargetBase, pSrcData, 0x1000, nullptr)) {
		out::ShowError("Failed to write shellcode helper data target process memory");
		delete[] pSrcData;
		VirtualFreeEx(hProc, pTargetBase, 0, MEM_RELEASE);
		CloseHandle(hProc);
		return false;
	}
	delete[] pSrcData;

	void* pShellcode = VirtualAllocEx(hProc, nullptr, 0x1000, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
	if (!pShellcode)
	{
		out::ShowError("Failed to allocate memory for shellcode");
		VirtualFreeEx(hProc, pTargetBase, 0, MEM_RELEASE);
		CloseHandle(hProc);
		return false;
	}

	if (!WriteProcessMemory(hProc, pShellcode, Shellcode, 0x1000, nullptr)) {
		out::ShowError("Failed to write memory for shellcode");
		VirtualFreeEx(hProc, pShellcode, 0, MEM_RELEASE);
		VirtualFreeEx(hProc, pTargetBase, 0, MEM_RELEASE);
		CloseHandle(hProc);
		return false;
	}


	HANDLE hThread = CreateRemoteThread(hProc, nullptr, 0, reinterpret_cast<LPTHREAD_START_ROUTINE>(pShellcode), pTargetBase, 0, nullptr);
	if (!hThread)
	{
		out::ShowError("Failed to create remote thread");
		VirtualFreeEx(hProc, pShellcode, 0, MEM_RELEASE);
		VirtualFreeEx(hProc, pTargetBase, 0, MEM_RELEASE);
		CloseHandle(hProc);
		return false;
	}
	CloseHandle(hThread);

	HINSTANCE hCheck = NULL;
	while (!hCheck)
	{
		MANUAL_MAPPING_DATA data_checked{ 0 };
		ReadProcessMemory(hProc, pTargetBase, &data_checked, sizeof(data_checked), nullptr);
		hCheck = data_checked.hMod;
		Sleep(10);
	}
	VirtualFreeEx(hProc, pShellcode, 0, MEM_RELEASE);


	CloseHandle(hProc);
	return true;
}

#define RELOC_FLAG32(RelInfo) ((RelInfo >> 0x0C) == IMAGE_REL_BASED_HIGHLOW)
#define RELOC_FLAG64(RelInfo) ((RelInfo >> 0x0C) == IMAGE_REL_BASED_DIR64)

#ifdef _WIN64
#define RELOC_FLAG RELOC_FLAG64
#else
#define RELOC_FLAG RELOC_FLAG32
#endif

void __stdcall Shellcode(MANUAL_MAPPING_DATA* pData)
{
	if (!pData)
		return;

	BYTE* pBase = reinterpret_cast<BYTE*>(pData);
	auto* pOpt = &reinterpret_cast<IMAGE_NT_HEADERS*>(pBase + reinterpret_cast<IMAGE_DOS_HEADER*>(pData)->e_lfanew)->OptionalHeader;

	auto _LoadLibraryA = pData->ploadLibraryA;
	auto _GetProcAddress = pData->pGetProcAddress;
	auto _DllMain = reinterpret_cast<f_DLL_ENTRY_POINT>(pBase + pOpt->AddressOfEntryPoint);

	if (!_LoadLibraryA)
		return;

	BYTE* LocationDelta = pBase - pOpt->ImageBase;
	if (LocationDelta)
	{
		if (!pOpt->DataDirectory[IMAGE_DIRECTORY_ENTRY_BASERELOC].Size)
			return;

		auto* pRelocData = reinterpret_cast<IMAGE_BASE_RELOCATION*>(pBase + pOpt->DataDirectory[IMAGE_DIRECTORY_ENTRY_BASERELOC].VirtualAddress);
		while (pRelocData->VirtualAddress)
		{
			UINT AmountOfEntries = (pRelocData->SizeOfBlock - sizeof(IMAGE_BASE_RELOCATION)) / sizeof(WORD);
			WORD* pRelativeInfo = reinterpret_cast<WORD*>(pRelocData + 1);

			for (UINT i = 0; i != AmountOfEntries; ++i, ++pRelativeInfo)
			{
				if (RELOC_FLAG(*pRelativeInfo))
				{
					UINT_PTR* pPatch = reinterpret_cast<UINT_PTR*>(pBase + pRelocData->VirtualAddress + ((*pRelativeInfo) & 0xFFF));
					*pPatch += reinterpret_cast<UINT_PTR>(LocationDelta);
				}
			}
			pRelocData = reinterpret_cast<IMAGE_BASE_RELOCATION*>(reinterpret_cast<BYTE*>(pRelocData) + pRelocData->SizeOfBlock);
		}
	}

	if (pOpt->DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].Size)
	{
		auto* pImportDescr = reinterpret_cast<IMAGE_IMPORT_DESCRIPTOR*>(pBase + pOpt->DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress);
		while (pImportDescr->Name)
		{
			char* szMod = reinterpret_cast<char*>(pBase + pImportDescr->Name);

			HINSTANCE hDll = _LoadLibraryA(szMod);

			ULONG_PTR* pThunkRef = reinterpret_cast<ULONG_PTR*>(pBase + pImportDescr->OriginalFirstThunk);
			ULONG_PTR* pFuncRef = reinterpret_cast<ULONG_PTR*>(pBase + pImportDescr->FirstThunk);

			if (!pThunkRef)
				pThunkRef = pFuncRef;

			for (; *pThunkRef; ++pThunkRef, ++pFuncRef)
			{
				if (IMAGE_SNAP_BY_ORDINAL(*pThunkRef))
				{
					*pFuncRef = _GetProcAddress(hDll, reinterpret_cast<char*>(*pThunkRef & 0xFFFF));
				}
				else
				{
					auto* pImport = reinterpret_cast<IMAGE_IMPORT_BY_NAME*>(pBase + (*pThunkRef));
					*pFuncRef = _GetProcAddress(hDll, pImport->Name);
				}
			}
			++pImportDescr;
		}
	}

	if (pOpt->DataDirectory[IMAGE_DIRECTORY_ENTRY_TLS].Size)
	{
		auto* pTLS = reinterpret_cast<IMAGE_TLS_DIRECTORY*>(pBase + pOpt->DataDirectory[IMAGE_DIRECTORY_ENTRY_TLS].VirtualAddress);
		auto* pCallback = reinterpret_cast<PIMAGE_TLS_CALLBACK*>(pTLS->AddressOfCallBacks);
		for (; pCallback && *pCallback; ++pCallback) {
			(*pCallback)(pBase, DLL_PROCESS_ATTACH, nullptr);
		}
	}

	_DllMain(pBase, DLL_PROCESS_ATTACH, nullptr);

	pData->hMod = reinterpret_cast<HINSTANCE>(pBase);
}

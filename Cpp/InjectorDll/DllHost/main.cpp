#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <string>
#include "CoreClrHost.h"
#include <Windows.h>

#define FS_SEPARATOR "\\"
#define PATH_DELIMITER ";"
#define CORECLR_FILE_NAME "coreclr.dll"

#define BaseAssemblyName "ManagedAssembly"

const char* ManagedAssemblyPath = "C:\\" BaseAssemblyName "\\" BaseAssemblyName ".dll";

const char* EntryPointAssemblyName = BaseAssemblyName ", Version=1.0.0.0";
const char* EntryPointTypeName = BaseAssemblyName ".Program";
const char* EntryPointMethodName = "Run";


typedef int (*Run_ptr)();


void BuildTpaList(const char* directory, const char* extension, std::string& tpaList);
void __stdcall StartHost(LPVOID param);

bool __stdcall DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID lpReserved)
{
    switch (dwReason)
    {
    case DLL_PROCESS_ATTACH:
        CreateThread(
            NULL, // default security attributes
            NULL, // use default stack size
            reinterpret_cast<LPTHREAD_START_ROUTINE>(StartHost), // thread function
            nullptr, // argument to thread function
            0, // use default creation flags
            nullptr); // returns the thread identifier
        break;
    }
    return TRUE;
}

void __stdcall StartHost(LPVOID param)
{
    char runtimePath[MAX_PATH];

    GetFullPathNameA(ManagedAssemblyPath, MAX_PATH, runtimePath, NULL);

    char* last_slash = strrchr(runtimePath, FS_SEPARATOR[0]);
    if (last_slash != NULL)
        *last_slash = 0;


    std::string coreClrPath(runtimePath);
    coreClrPath.append(FS_SEPARATOR);
    coreClrPath.append(CORECLR_FILE_NAME);


    HMODULE coreClr = LoadLibraryExA(coreClrPath.c_str(), NULL, 0);
    if (coreClr == NULL) {
        char errorMsg[64];
        sprintf_s(errorMsg, "ERROR: Failed to load CoreCLR from %s\n", coreClrPath.c_str());

        MessageBox(NULL, errorMsg, "ERROR", NULL);
        return;
    }


    coreclr_initialize_ptr initializeCoreClr = (coreclr_initialize_ptr)GetProcAddress(coreClr, "coreclr_initialize");
    coreclr_create_delegate_ptr createManagedDelegate = (coreclr_create_delegate_ptr)GetProcAddress(coreClr, "coreclr_create_delegate");
    coreclr_shutdown_ptr shutdownCoreClr = (coreclr_shutdown_ptr)GetProcAddress(coreClr, "coreclr_shutdown");
    coreclr_execute_assembly_ptr executeAssembly = (coreclr_execute_assembly_ptr)GetProcAddress(coreClr, "coreclr_execute_assembly");

    if (initializeCoreClr == NULL) {
        MessageBox(NULL, "coreclr_initialize not found", "ERROR", NULL);
        return;
    }

    if (createManagedDelegate == NULL) {
        MessageBox(NULL, "coreclr_create_delegate not found", "ERROR", NULL);
        return;
    }

    if (shutdownCoreClr == NULL) {
        MessageBox(NULL, "coreclr_shutdown not found", "ERROR", NULL);
        return;
    }

    if (executeAssembly == NULL) {
        MessageBox(NULL, "coreclr_execute_assembly not found", "ERROR", NULL);
        return;
    }




    std::string tpaList;
    BuildTpaList(runtimePath, ".dll", tpaList);
   
    const char* propertyKeys[] = {
        "TRUSTED_PLATFORM_ASSEMBLIES"
    };

    const char* propertyValues[] = {
        tpaList.c_str()
    };
 



    void* hostHandle;
    unsigned int domainId;

    int hr = initializeCoreClr(
        runtimePath,        // App base path
        "InjectedDllNetCoreHost",          // AppDomain friendly name
        sizeof(propertyKeys) / sizeof(char*),   // Property count
        propertyKeys,       // Property names
        propertyValues,     // Property values
        &hostHandle,        // Host handle
        &domainId);         // AppDomain ID


    if (hr < 0) {
        char errorMsg[64];
        sprintf_s(errorMsg, "coreclr_initialize failed - status: 0x%08x\n", hr);

        MessageBox(NULL, errorMsg, "ERROR", NULL);
        return;
    }


    Run_ptr managedDelegate;
    hr = createManagedDelegate(
        hostHandle,
        domainId,
        EntryPointAssemblyName,
        EntryPointTypeName,
        EntryPointMethodName,
        (void**)&managedDelegate);


    if (hr < 0) {
        char errorMsg[64];
        sprintf_s(errorMsg, "coreclr_create_delegate failed - status: 0x%08x\n", hr);

        MessageBox(NULL, errorMsg, "ERROR", NULL);
        return;
    }

    
    int ret = managedDelegate();

    char msg[64];
    sprintf_s(msg, "Run returned %d", ret);
    MessageBox(NULL, msg, "Run Returned", NULL);


    hr = shutdownCoreClr(hostHandle, domainId);


    if (hr < 0) {
        char errorMsg[64];
        sprintf_s(errorMsg, "failed failed - status: 0x%08x\n", hr);

        MessageBox(NULL, errorMsg, "ERROR", NULL);
        return;
    }
}


void BuildTpaList(const char* directory, const char* extension, std::string& tpaList)
{
    std::string searchPath(directory);
    searchPath.append(FS_SEPARATOR);
    searchPath.append("*");
    searchPath.append(extension);

    WIN32_FIND_DATAA findData;
    HANDLE fileHandle = FindFirstFileA(searchPath.c_str(), &findData);

    if (fileHandle != INVALID_HANDLE_VALUE)
    {
        do
        {
            tpaList.append(directory);
            tpaList.append(FS_SEPARATOR);
            tpaList.append(findData.cFileName);
            tpaList.append(PATH_DELIMITER);
        } while (FindNextFileA(fileHandle, &findData));

        FindClose(fileHandle);
    }
}

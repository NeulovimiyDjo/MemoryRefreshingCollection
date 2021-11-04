#include <windows.h>
#include <stdio.h>

void ShowMsg(const char* msg) {
    char fullMsg[100];
    strcpy_s(fullMsg, "call from ");
    strcat_s(fullMsg, msg);

    MessageBox(NULL, fullMsg, "From Notepad", NULL);
}

void __stdcall DoWork(LPVOID param) {
    int i = 0;
    while (true)
    {
        char str[100];
        sprintf_s(str, "DLL_PROCESS_ATTACH %d", ++i);

        ShowMsg(str);
        Sleep(5000);
    }
}

BOOL APIENTRY DllMain(HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved
)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        CreateThread(
            NULL, // default security attributes
            0, // use default stack size
            reinterpret_cast<LPTHREAD_START_ROUTINE>(DoWork), // thread function
            nullptr, // argument to thread function
            0, // use default creation flags
            nullptr); // returns the thread identifier
        break;
    case DLL_THREAD_ATTACH:
        ShowMsg("DLL_THREAD_ATTACH");
        break;
    case DLL_THREAD_DETACH:
        ShowMsg("DLL_THREAD_DETACH");
        break;
    case DLL_PROCESS_DETACH:
        ShowMsg("DLL_PROCESS_DETACH");
        break;
    }

    return TRUE;
}
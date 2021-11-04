#include <Windows.h>

#include <iostream>

#include "../InjectorLib/GetProcessId.h"
#include "../InjectorLib/LoadLibraryInject.h"
#include "../InjectorLib/ManualMapInject.h"


int main()
{
    char dll[MAX_PATH];
    GetFullPathName("DllMsg.dll", MAX_PATH, dll, NULL);
    DWORD ID = GetProcessId("notepad.exe");

    int method = 2;
    switch (method)
    {
    case 1:
        if (!LoadLibraryInject(ID, dll))
            std::cout << "failure";
        else
            std::cout << "success";
        break;
    case 2:
        if (!ManualMapInject(ID, dll))
            std::cout << "failure";
        else
            std::cout << "success";
        break;
    default:
        break;
    }
    

    return 0;
}
#include <windows.h>
#include <tlhelp32.h> 

#include "GetProcessId.h"

DWORD GetProcessId(const char* szExeName)
{
    DWORD dwRet = 0;
    DWORD dwCount = 0;

    HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);

    if (hSnapshot != INVALID_HANDLE_VALUE)
    {
        PROCESSENTRY32 pe = { 0 };
        pe.dwSize = sizeof(PROCESSENTRY32);

        BOOL bRet = Process32First(hSnapshot, &pe);

        while (bRet)
        {
            if (!strcmp(szExeName, pe.szExeFile))
            {
                dwCount++;
                dwRet = pe.th32ProcessID;
            }
            bRet = Process32Next(hSnapshot, &pe);
        }

        if (dwCount > 1)
            dwRet = 0xFFFFFFFF;

        CloseHandle(hSnapshot);
    }

    return dwRet;
}

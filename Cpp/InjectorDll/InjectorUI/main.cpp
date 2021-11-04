#include <Windows.h>

#include "../InjectorLib/GetProcessId.h"
#include "../InjectorLib/LoadLibraryInject.h"
#include "../InjectorLib/ManualMapInject.h"


void Inject(const char * dllName, const char* targetName, int method) {
	char dllFullName[MAX_PATH];
	GetFullPathName(dllName, MAX_PATH, dllFullName, NULL);
	DWORD ID = GetProcessId(targetName);

	switch (method)
	{
	case 1:
		if (!LoadLibraryInject(ID, dllFullName))
			MessageBox(nullptr, "Error: Failed LoadLibraryInject", "Error", MB_OK | MB_ICONERROR);
		else
			MessageBox(nullptr, "Succeeded LoadLibraryInject", "Success", MB_OK | MB_ICONINFORMATION);
		break;
	case 2:
		if (!ManualMapInject(ID, dllFullName))
			MessageBox(nullptr, "Error: Failed ManualMapInject", "Error", MB_OK | MB_ICONERROR);
		else
			MessageBox(nullptr, "Succeeded ManualMapInject", "Success", MB_OK | MB_ICONINFORMATION);
		break;
	default:
		break;
	}
}

HWND dllEditControl = NULL;
HWND targetEditControl = NULL;
HWND inject1ButtonControl = NULL;
HWND inject2ButtonControl = NULL;
WORD inject1ButtonControlID = 0xFF00;
WORD inject2ButtonControlID = 0xFF01;

LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
	switch (uMsg) {
		case WM_PAINT:
		{
			PAINTSTRUCT ps;
			HDC hdc = BeginPaint(hwnd, &ps);

			RECT rec;

			SetRect(&rec, 30, 10, 400, 40); // SetRect(rect, x ,y ,width, height)
			//GetClientRect(hwnd, &rec);

			const char* text = "Fill dll name to inject\nand target process name.";
			DrawText(
				hdc,							// HDC
				TEXT(text),		// text
				strlen(text),		// text length
				&rec,							// drawing area
				DT_TOP | DT_LEFT				// parameters "DT_XXX"
			);

			EndPaint(hwnd, &ps);
			ReleaseDC(hwnd, hdc);

			break;
		}
		case WM_COMMAND:
			if (LOWORD(wParam) == inject1ButtonControlID) {
				char dllName[64];
				GetWindowText(dllEditControl, dllName, 64);

				char targetName[64];
				GetWindowText(targetEditControl, targetName, 64);

				Inject(dllName, targetName, 1);
			} else if (LOWORD(wParam) == inject2ButtonControlID) {
				char dllName[64];
				GetWindowText(dllEditControl, dllName, 64);

				char targetName[64];
				GetWindowText(targetEditControl, targetName, 64);

				Inject(dllName, targetName, 2);
			}
			break;
		case WM_CLOSE:
		case WM_DESTROY:			
			PostQuitMessage(0);
			return 0;
		default:
			return DefWindowProc(hwnd, uMsg, wParam, lParam);
	}
}

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int cmdShow)
{
	const char className[] = "InjectorUIMainWindow";

	WNDCLASSEX wcex;
	wcex.cbClsExtra = 0;
	wcex.cbSize = sizeof(WNDCLASSEX);
	wcex.cbWndExtra = 0;
	wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
	wcex.hCursor = LoadCursor(NULL, IDC_ARROW);
	wcex.hIcon = LoadIcon(NULL, IDI_APPLICATION);
	wcex.hIconSm = LoadIcon(NULL, IDI_APPLICATION);
	wcex.hInstance = hInstance;
	wcex.lpfnWndProc = WindowProc;
	wcex.lpszClassName = className;
	wcex.lpszMenuName = NULL;
	wcex.style = CS_HREDRAW | CS_VREDRAW;

	if (!RegisterClassEx(&wcex))
	{
		MessageBox(NULL, TEXT("RegisterClassEx Failed!"), TEXT("Error"),
			MB_ICONERROR);
		return EXIT_FAILURE;
	}


	HWND hwnd = CreateWindowEx(
		0,										// Optional window styles.
		className,								// Window class
		"InjectorUI",								// Window text
		WS_OVERLAPPEDWINDOW,					// Window style
		CW_USEDEFAULT, CW_USEDEFAULT, 800, 600, // Position and size
		NULL,									// Parent window    
		NULL,									// Menu
		hInstance,								// Instance handle
		NULL									// Additional application data
	);

	if (hwnd == NULL)
	{
		MessageBox(NULL, TEXT("CreateWindow Failed!"), TEXT("Error"), MB_ICONERROR);
		return EXIT_FAILURE;
	}


	dllEditControl = CreateWindowEx(WS_EX_CLIENTEDGE, TEXT("Edit"), TEXT("DllWowFunctions.dll"),
		WS_CHILD | WS_VISIBLE, 30, 50, 400,
		20, hwnd, NULL, NULL, NULL);

	targetEditControl = CreateWindowEx(WS_EX_CLIENTEDGE, TEXT("Edit"), TEXT("WoW.exe"),
		WS_CHILD | WS_VISIBLE, 30, 70, 400,
		20, hwnd, NULL, NULL, NULL);

	inject1ButtonControl = CreateWindowEx(WS_EX_CLIENTEDGE, TEXT("BUTTON"), TEXT("LoadLibraryInject"),
		WS_CHILD | WS_VISIBLE, 30, 100, 400,
		20, hwnd, (HMENU)inject1ButtonControlID, NULL, NULL);

	inject2ButtonControl = CreateWindowEx(WS_EX_CLIENTEDGE, TEXT("BUTTON"), TEXT("ManualMapInject"),
		WS_CHILD | WS_VISIBLE, 30, 120, 400,
		20, hwnd, (HMENU)inject2ButtonControlID, NULL, NULL);



	ShowWindow(hwnd, cmdShow);
	UpdateWindow(hwnd);

	MSG msg = { };
	while (GetMessage(&msg, NULL, 0, 0))
	{
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	return EXIT_SUCCESS;
}
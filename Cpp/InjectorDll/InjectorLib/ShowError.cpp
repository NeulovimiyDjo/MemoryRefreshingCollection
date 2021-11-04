#include <windows.h>
#include <string>
#include "ShowError.h"

//Returns the last Win32 error, in string format. Returns an empty string if there is no error.
std::string GetErrorMessage(DWORD errorMessageID)
{
	if (errorMessageID == 0)
		return std::string(); //No error message has been recorded

	char messageBuffer[256];
	size_t size = FormatMessageA(
		FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL,
		errorMessageID,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),//MAKELANGID(LANG_ENGLISH, SUBLANG_ENGLISH_US),
		messageBuffer,
		sizeof(messageBuffer),
		NULL
	);

	std::string message(messageBuffer, size);

	return message;
}

void out::ShowError(const char* msg) {
	DWORD errorMessageID = GetLastError();


	char errNoStr[100];
	sprintf_s(errNoStr, "0x%X", errorMessageID);

	std::string fullMsg =
		msg
		+ std::string("\nErrNo: ") + std::string(errNoStr)
		+ std::string("\nErrMsg: ") + GetErrorMessage(errorMessageID);


	MessageBox(nullptr, fullMsg.c_str(), "Error:", MB_OK | MB_ICONERROR);
}
#include "stdafx.h"
#include "Class.h"
#include <iostream>

HelpClass::HelpClass()
{
	m = 45;
}

void CppClass::print()
{
	std::cout << count1 << " " << count2 << " " << c3 << " " << arr[0] << " " << h->m << "\n";
}


CppClass::~CppClass()
{
	delete h;
	std::cout << " Class Deleted \n";
}

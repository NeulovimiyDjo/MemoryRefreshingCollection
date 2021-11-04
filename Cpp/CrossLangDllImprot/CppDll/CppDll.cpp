// CppDll.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include <iostream>
#include "Class.h"
#include "CppDll.h"

int worksfinally(int x, int y)
{
	return 3 * (x + y);
}

double multi3wrapper(CppClass* cls)
{
	return  cls->multi3();
}

CppClass* createclass(int c1, int c2, double x3)
{
	return new CppClass(c1, c2, x3, "changed", new HelpClass());
}

void deleteclass(CppClass* cls)
{
	if (cls != NULL)
	{
		delete cls;
		cls = NULL;
	}
}

void printclass(CppClass* cls)
{
	cls->print();
}
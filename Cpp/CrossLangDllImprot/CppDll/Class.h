class HelpClass
{
public:
	int m;
	HelpClass();
};

class CppClass
{
public:
	int count1 = 3;
	int count2 = 4;
	double c3;
	char* arr = "rekt";
	HelpClass* h = new HelpClass();

	CppClass()
	{
		c3 = 3.0;
	}

	~CppClass();

	CppClass(int c1, int c2, double x3, char* ar, HelpClass* h1)
	{
		count1 = c1;
		count2 = c2;
		c3 = x3;
		arr = ar;
		h = h1;
	}

	double multi3()
	{
		return count1*count2 / c3;
	}

	void print();
};

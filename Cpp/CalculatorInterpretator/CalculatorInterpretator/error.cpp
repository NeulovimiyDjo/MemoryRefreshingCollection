#include "error.h"
#include <iostream>

int Error::no_of_errors;

double Error::error(const std::string& s) {
	++no_of_errors;
	std::cerr << "error: " << s << '\n';
	return 1;
}
#include "input.h"
#include "error.h"
#include "lexxer.h"

using Error::error;
using namespace Lexxer;

namespace Parser {
	double expr(bool);
	double term(bool);
	double prim(bool);
} // namespace Parser
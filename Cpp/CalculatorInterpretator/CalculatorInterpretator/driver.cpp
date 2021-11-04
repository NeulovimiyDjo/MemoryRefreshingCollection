#include "input.h"
#include "parser.h"
#include "lexxer.h"
#include "driver.h"

void Driver::calculate()
{
	using namespace Lexxer;

	for (;;) {
		ts.get();
		if (ts.current().kind == Kind::end) break;
		if (ts.current().kind == Kind::print) continue;
		std::cout << Parser::expr(false) << '\n';
	}
}
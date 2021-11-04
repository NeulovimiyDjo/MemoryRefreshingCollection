#include "parser_impl.h"

double Parser::prim(bool getnext)
{
	if (getnext) ts.get();

	switch (ts.current().kind) {
	case Kind::number: {
		double v = ts.current().number_value;
		ts.get();
		return v; }
	case Kind::name: {
		double& v = Lexxer::table[ts.current().string_value];
		if (ts.get().kind == Kind::assign) v = expr(true);
		return v; }
	case Kind::minus:
		return -prim(true);
	case Kind::plus:
		return prim(true);
	case Kind::lp: {
		auto e = expr(true);
		if (ts.current().kind != Kind::rp) return error("')' expected");
		ts.get();
		return e; }
	default:
		return error("primary expected");
	}
}

double Parser::term(bool getnext)
{
	double left = prim(getnext);

	for (;;) {
		switch (ts.current().kind) {
		case Kind::mul:
			left *= prim(true);
			break;
		case Kind::div:
			if (auto d = prim(true)) {
				left /= d;
				break;
			}
			return error("divide by 0");
		default:
			return left;
		}
	}
}

double Parser::expr(bool getnext)
{
	double left = term(getnext);

	for (;;) {
		switch (ts.current().kind) {
		case Kind::plus:
			left += term(true);
			break;
		case Kind::minus:
			left -= term(true);
			break;
		default:
			return left;
		}
	}
}
#include "input.h"
#include <string>
#include <map>

namespace Lexxer {
	enum class Kind :char {
		name, number, end,
		plus = '+', minus = '-', mul = '*', div = '/', print = ';', assign = '=', lp = '(', rp = ')'
	};

	struct Token {
		Kind kind;
		std::string string_value;
		double number_value;
	};

	extern std::map<std::string, double> table;

	class Token_stream {
	public:
		Token_stream(Input::IStreamHandler* h) :ish{ h }, ip{ ish->ifs } { table["pi"] = 3.1415; table["e"] = 2.7118; }
		~Token_stream() { delete ish; }

		Token get();
		const Token& current() { return ct; }
		double expr(bool get);

	private:
		double prim(bool get);
		double term(bool get);

		Input::IStreamHandler* ish;
		std::istream* ip;

		Token ct{ Kind::end };
	};

	extern Token_stream ts;
} // namespace Lexxer
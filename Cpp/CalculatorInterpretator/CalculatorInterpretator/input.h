#include <iostream>
#include <sstream>
#include <fstream>

#ifndef CI_INPUT_H
#define CI_INPUT_H

namespace Input {
	using namespace std;

	class IStreamHandler {
	public:
		IStreamHandler(istream* stream, bool shoulddelete) :ifs{ stream }, calldelete{ shoulddelete } { type = StreamType::istream; }
		IStreamHandler(ifstream* stream, bool shoulddelete) :ifs{ stream }, calldelete{ shoulddelete } { type = StreamType::ifstream; }
		IStreamHandler(istringstream* stream, bool shoulddelete) :ifs{ stream }, calldelete{ shoulddelete } { type = StreamType::istringstream; }
		~IStreamHandler() {
			if (type == StreamType::ifstream) static_cast<ifstream*>(ifs)->close();
			if (calldelete) delete ifs;
		}

		istream* ifs;

	private:
		enum class StreamType { istream, ifstream, istringstream };
		StreamType type;
		bool calldelete;
	};
} // namespace Input

#endif // CI_INPUT_H
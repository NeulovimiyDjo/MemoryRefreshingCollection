#include "input.h"
#include "error.h"
#include "lexxer.h"
#include "driver.h"

std::map<std::string, double> Lexxer::table;
Lexxer::Token_stream Lexxer::ts{ new Input::IStreamHandler{ new std::ifstream{ "../CalculatorInterpretator/1.txt" }, true } };

int main(int argc, char* argv[])
{
  /*Input::IStreamHandler* ish;
  switch (argc) {
  case 1:
    ish = new Input::IStreamHandler{ &std::cin , false };
    break;
  case 2:
    ish = new Input::IStreamHandler{ new std::istringstream{ argv[1] }, true };
    break;
  case 3:
    ish = new Input::IStreamHandler{ new std::ifstream{ "1.txt" }, true };
    break;
  default:
    Error::error("too many arguments");
    return 1;
  }*/

  Driver::calculate();


  return Error::no_of_errors;
}


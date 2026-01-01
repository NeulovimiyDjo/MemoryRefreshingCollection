cmake_minimum_required(VERSION 3.5)

project(CalculatorInterpretator)

#include_directories("CalculatorInterpretator")

file(GLOB SRC_LIST "CalculatorInterpretator/*.cpp")

set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -O3")

#set(SRC_LIST CalculatorInterpretator/main.cpp CalculatorInterpretator/driver.cpp CalculatorInterpretator/parser.cpp CalculatorInterpretator/lexxer.cpp CalculatorInterpretator/error.cpp)
add_executable(ci ${SRC_LIST})

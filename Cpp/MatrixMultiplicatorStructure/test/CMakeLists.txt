cmake_minimum_required(VERSION 3.5 FATAL_ERROR)

project(TestAll)

enable_testing()

include(GTest)

if(CMAKE_BUILD_TYPE STREQUAL Release)
  set(TEST_ON_BUILD OFF CACHE BOOL "A flag to enable testing during build")
elseif(CMAKE_BUILD_TYPE STREQUAL Debug)
  set(TEST_ON_BUILD OFF CACHE BOOL "A flag to enable testing during build")
endif()

add_gtest_test(prog_test Prog/prog_test.cpp)
add_gtest_test(mydll_test MyDll/mydll_test.cpp)
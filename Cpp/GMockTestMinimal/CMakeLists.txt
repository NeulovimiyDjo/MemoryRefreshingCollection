cmake_minimum_required(VERSION 3.5)
project(gtest1)

if(MSVC)
  # Has to be declared before adding gtest itself
  option(gtest_force_shared_crt "Use shared (DLL) run-time lib even when Google Test is built as static lib." ON)
endif()

enable_testing()

set(GOOGLETEST_DIR "C:/Program_Files/CppLibs/GTest/googletest")
set(GMOCK_DIR "C:/Program_Files/CppLibs/GTest/googlemock")


include_directories("${GOOGLETEST_DIR}/include")
#add_subdirectory("${GOOGLETEST_DIR}" "${CMAKE_BINARY_DIR}/test")

#include_directories("${GMOCK_DIR}" "${GMOCK_DIR}/include")
add_subdirectory("${GMOCK_DIR}" "${CMAKE_BINARY_DIR}/test")


add_executable(test1 test1.cpp)
target_link_libraries(test1 gmock_main)
add_test(NAME test1 COMMAND test1)
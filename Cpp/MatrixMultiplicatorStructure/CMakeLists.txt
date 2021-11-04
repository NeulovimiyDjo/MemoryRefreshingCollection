cmake_minimum_required(VERSION 3.5 FATAL_ERROR)

project(MatrixMiltiplicator)

list(APPEND CMAKE_MODULE_PATH "${CMAKE_SOURCE_DIR}/cmake")

include(MatrixMultiplicatorConfig)

add_subdirectory(src/Prog)
add_subdirectory(src/MyDll)

add_subdirectory(test)

include(CPack)
cmake_minimum_required(VERSION 3.5)

project(MatrixMultiMingw)

if ("${CMAKE_CXX_COMPILER_ID}" STREQUAL "GNU")
	set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -O3 -I C:/Program_Files/CppLibs/Eigen")
elseif ("${CMAKE_CXX_COMPILER_ID}" STREQUAL "MSVC")
	set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -I C:/Program_Files/CppLibs/Eigen")
endif()

add_library(mydll SHARED mydll.cpp)
set_target_properties(mydll PROPERTIES PREFIX "" OUTPUT_NAME "mydll" SUFFIX ".dll")

if ("${CMAKE_CXX_COMPILER_ID}" STREQUAL "GNU")
	add_library(openblas STATIC IMPORTED)
	set_target_properties(openblas PROPERTIES IMPORTED_LOCATION "C:/Program_Files/CppLibs/OpenBLAS-0.2.20/package/lib/libopenblas.a")
elseif ("${CMAKE_CXX_COMPILER_ID}" STREQUAL "MSVC")
	add_library(openblas SHARED IMPORTED)
	set_target_properties(openblas PROPERTIES IMPORTED_IMPLIB "C:/Program_Files/CppLibs/OpenBLAS-0.2.20/package/lib/libopenblas.dll.a")
endif()

add_executable(prog prog.cpp)
target_link_libraries(prog openblas)

install (TARGETS prog DESTINATION "${CMAKE_SOURCE_DIR}/bin")
install (TARGETS mydll DESTINATION "${CMAKE_SOURCE_DIR}/bin")

if ("${CMAKE_CXX_COMPILER_ID}" STREQUAL "MSVC")
	add_custom_command(TARGET prog POST_BUILD 
               COMMAND ${CMAKE_COMMAND} -E copy_if_different
               "C:/Program_Files/CppLibs/OpenBLAS-0.2.20/package/bin/libopenblas.dll"
                $<TARGET_FILE_DIR:prog>)

	install (FILES "C:/Program_Files/CppLibs/OpenBLAS-0.2.20/package/bin/libopenblas.dll" DESTINATION "${CMAKE_SOURCE_DIR}/bin")
endif()

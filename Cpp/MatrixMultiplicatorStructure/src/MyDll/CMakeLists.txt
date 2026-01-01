cmake_minimum_required(VERSION 3.5)

project(MyDll)

if(MINGW)
	set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} -Wall")
	set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} -Wall")
endif()

add_library(${PROJECT_NAME} MODULE mydll.cpp)
set_target_properties(${PROJECT_NAME} PROPERTIES PREFIX "" OUTPUT_NAME "mydll" SUFFIX ".dll")

set_target_properties(${PROJECT_NAME}
											PROPERTIES
											ARCHIVE_OUTPUT_DIRECTORY "${OUTPUT_DIRECTORY}/out"
											LIBRARY_OUTPUT_DIRECTORY "${OUTPUT_DIRECTORY}/out"
											RUNTIME_OUTPUT_DIRECTORY "${OUTPUT_DIRECTORY}/out")

install (TARGETS ${PROJECT_NAME} DESTINATION "bin")
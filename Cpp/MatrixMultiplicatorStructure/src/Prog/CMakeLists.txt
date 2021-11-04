cmake_minimum_required(VERSION 3.5 FATAL_ERROR)

project(Prog)

if(MINGW)
	set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} -Wall")
	set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} -Wall")
endif()

include(Eigen)
include(OpenBlas)

add_executable(${PROJECT_NAME} prog.cpp)
set_target_properties(${PROJECT_NAME} PROPERTIES PREFIX "" OUTPUT_NAME "prog" SUFFIX ".exe")
link_target_to_openblas(${PROJECT_NAME})

if(MSVC)
  add_subdirectory(../QtGui ${CMAKE_CURRENT_BINARY_DIR}/QtGui)
  target_link_libraries(${PROJECT_NAME} QtGui)
endif()

set_target_properties(${PROJECT_NAME}
											PROPERTIES
											ARCHIVE_OUTPUT_DIRECTORY "${OUTPUT_DIRECTORY}/out"
											LIBRARY_OUTPUT_DIRECTORY "${OUTPUT_DIRECTORY}/out"
											RUNTIME_OUTPUT_DIRECTORY "${OUTPUT_DIRECTORY}/out")


install (TARGETS ${PROJECT_NAME} DESTINATION "bin")
if(MSVC)
	install(FILES ${OPENBLAS_DLL_FILES} DESTINATION "bin")
endif()
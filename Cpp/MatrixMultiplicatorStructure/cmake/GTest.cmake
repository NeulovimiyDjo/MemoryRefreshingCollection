set(GOOGLE_TEST_DIR "${CMAKE_SOURCE_DIR}/../../libs/GTest/googletest")

if(MSVC)
  # Has to be declared before adding gtest itself
  option(gtest_force_shared_crt "Use shared (DLL) run-time lib even when Google Test is built as static lib." ON)
endif()

add_subdirectory(${GOOGLE_TEST_DIR} ${CMAKE_BINARY_DIR}/test/googletest)
include_directories(SYSTEM ${GOOGLE_TEST_DIR}/include)

function(add_gtest_test target)
  add_executable(${target} ${ARGN})
  target_link_libraries(${target} gtest_main)

	set_target_properties(${target}
												PROPERTIES
												ARCHIVE_OUTPUT_DIRECTORY "${OUTPUT_DIRECTORY}/test"
												LIBRARY_OUTPUT_DIRECTORY "${OUTPUT_DIRECTORY}/test"
												RUNTIME_OUTPUT_DIRECTORY "${OUTPUT_DIRECTORY}/test")

	add_test(NAME ${target} COMMAND ${target})
	
	if(TEST_ON_BUILD)
		add_custom_command(TARGET ${target}
											POST_BUILD
											COMMAND ${target}
											VERBATIM)
	endif()
endfunction()
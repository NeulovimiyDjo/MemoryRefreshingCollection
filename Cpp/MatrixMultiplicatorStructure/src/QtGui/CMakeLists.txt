cmake_minimum_required(VERSION 3.5 FATAL_ERROR)

project(QtGui)

add_definitions(-DPROJECT_EXPORTS)

find_package(Qt5 REQUIRED COMPONENTS Core Widgets Gui)

set(CMAKE_AUTOUIC ON)
set(CMAKE_AUTOMOC ON)
set(CMAKE_INCLUDE_CURRENT_DIR ON)

add_library(${PROJECT_NAME} SHARED runner.cpp mainwindow.cpp)


set_target_properties(${PROJECT_NAME}
											PROPERTIES
											ARCHIVE_OUTPUT_DIRECTORY "${OUTPUT_DIRECTORY}/out"
											LIBRARY_OUTPUT_DIRECTORY "${OUTPUT_DIRECTORY}/out"
											RUNTIME_OUTPUT_DIRECTORY "${OUTPUT_DIRECTORY}/out")

target_link_libraries(${PROJECT_NAME} PUBLIC Qt5::Core Qt5::Widgets Qt5::Gui)

install (TARGETS ${PROJECT_NAME}
        RUNTIME DESTINATION "bin" COMPONENT Runtime
        LIBRARY DESTINATION "bin" COMPONENT Runtime)
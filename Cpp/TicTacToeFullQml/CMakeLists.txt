cmake_minimum_required(VERSION 3.5 FATAL_ERROR)

project(TicTacToe)

find_package(Qt5 COMPONENTS Widgets Qml)

set(CMAKE_AUTORCC ON)

add_executable(${PROJECT_NAME} WIN32 src/main.cpp src/resources.qrc)
target_link_libraries(${PROJECT_NAME} Qt5::Widgets Qt5::Qml)

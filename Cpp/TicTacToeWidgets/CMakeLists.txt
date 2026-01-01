cmake_minimum_required(VERSION 3.5 FATAL_ERROR)

project(TicTacToeQtWidgets)

find_package(Qt5 COMPONENTS Widgets REQUIRED)

set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTORCC ON)

add_executable(${PROJECT_NAME} main.cpp ImageButton.cpp GameWindow.cpp resources.qrc)
target_link_libraries(${PROJECT_NAME} Qt5::Widgets)
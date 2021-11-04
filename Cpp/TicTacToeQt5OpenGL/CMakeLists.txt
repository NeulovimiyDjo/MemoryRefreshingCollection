cmake_minimum_required(VERSION 3.0.0 FATAL_ERROR)

project(TicTacToe)

find_package(Qt5 COMPONENTS Widgets REQUIRED)

set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTORCC ON)

add_executable(${PROJECT_NAME} WIN32 resources/resources.qrc src/main.cpp src/GameWindow.cpp
  src/GameEngine.cpp src/FieldWidget.cpp src/FieldModel.cpp src/Square.cpp src/Mesh.cpp)
target_link_libraries(${PROJECT_NAME} Qt5::Widgets)

cmake_minimum_required(VERSION 3.5 FATAL_ERROR)

project(TicTacToe)

set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} -DQT_QML_DEBUG ")

find_package(Qt5 COMPONENTS Widgets Qml Quick)

set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTORCC ON)

add_executable(${PROJECT_NAME} WIN32 src/main.cpp
  src/GameManager.cpp src/GameEngine.cpp src/ImageButton.cpp src/resources.qrc)
target_link_libraries(${PROJECT_NAME} Qt5::Widgets Qt5::Qml Qt5::Quick)

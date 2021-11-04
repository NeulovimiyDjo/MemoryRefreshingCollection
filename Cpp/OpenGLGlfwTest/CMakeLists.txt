cmake_minimum_required(VERSION 3.5 FATAL_ERROR)

project(OpenGLTest)

if(MSVC)
  include_directories("../../libs/glfw-3.2.1/build/MSVC-INSTALL/include")
elseif(MINGW)
  include_directories("../../libs/glfw-3.2.1/build-gcc/package/include")
endif()
include_directories("../../libs/glad/include")
include_directories("../../libs/glm-0.9.9.0")
include_directories("../../libs/stb_image")

add_subdirectory("../../libs/ResourceManager" "${CMAKE_BINARY_DIR}/ResourceManager")
include_directories(${RESOURCE_MANAGER_INCLUDE_DIRS})

rm_embed_resources(RESOURCES cross.png vshader.vs fshader.fs container2.png corten-canfranc.jpg awesomeface.png main.cpp)

add_executable(${PROJECT_NAME} main.cpp "../../libs/glad/src/glad.c" ${RESOURCES})
if(MSVC)
  target_link_libraries(${PROJECT_NAME} "${CMAKE_SOURCE_DIR}/../../libs/glfw-3.2.1/build/MSVC-INSTALL/lib/glfw3.lib")
elseif(MINGW)
  target_link_libraries(${PROJECT_NAME} "${CMAKE_SOURCE_DIR}/../../libs/glfw-3.2.1/build-gcc/package/lib/libglfw3.a")
endif()

if(MSVC)
  target_compile_options(${PROJECT_NAME} PRIVATE -WX)
elseif(MINGW)
  target_compile_options(${PROJECT_NAME} PRIVATE -Werror -Wall)
endif()

cd %~dp0/..
mkdir build\MSVC-Debug
cd build/MSVC-Debug

set msvc_env_path="C:/Program Files (x86)/Microsoft Visual Studio/2017/Community/VC/Auxiliary/Build/vcvarsall.bat"
set cmake_path="C:/Program Files (x86)/Microsoft Visual Studio/2017/Community/Common7/IDE/CommonExtensions/Microsoft/CMake/CMake/bin/cmake.exe"
set cl_path="C:/Program Files (x86)/Microsoft Visual Studio/2017/Community/VC/Tools/MSVC/14.13.26128/bin/Hostx64/x64/cl.exe"
set ninja_path="C:/Program Files (x86)/Microsoft Visual Studio/2017/Community/Common7/IDE/CommonExtensions/Microsoft/CMake/Ninja/ninja.exe"

set compiler_options=-DCMAKE_C_COMPILER=%cl_path% -DCMAKE_CXX_COMPILER=%cl_path% -DCMAKE_MAKE_PROGRAM=%ninja_path%

call %msvc_env_path% x64
call %cmake_path% -G "Ninja" ../.. %compiler_options% -DCMAKE_BUILD_TYPE=Debug
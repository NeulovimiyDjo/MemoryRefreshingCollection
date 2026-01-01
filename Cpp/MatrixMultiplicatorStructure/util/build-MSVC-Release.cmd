cd %~dp0/../build/MSVC-Release

set msvc_env_path="C:/Program Files (x86)/Microsoft Visual Studio/2017/Community/VC/Auxiliary/Build/vcvarsall.bat"
set cmake_path="C:/Program Files (x86)/Microsoft Visual Studio/2017/Community/Common7/IDE/CommonExtensions/Microsoft/CMake/CMake/bin/cmake.exe"

call %msvc_env_path% x64
call %cmake_path% --build .
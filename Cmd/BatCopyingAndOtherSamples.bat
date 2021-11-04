@echo off

setlocal
setlocal EnableExtensions
setlocal EnableDelayedExpansion

cd /D "%~dp0..\.."
set RepoDir=%cd%

set "DefaultFiles=C:\tmp\Files"
set /P Files="Enter files folder [%DefaultFiles%]: "
if "%Files%"=="" set "Files=%DefaultFiles%"
echo [Files] = %Files%
echo;
echo  ^> Press any key to start...
pause>nul

(rmdir "%Files%\tmp" /S /Q)>nul 2>&1
(mkdir "%Files%\tmp")>nul 2>&1


if not exist "SomeDir\subdir" goto :Error
call "test.exe"
if %errorlevel% neq 0 goto :Error


copy /y /b "$(ProjectDir)..\files1\*.txt" "$(ProjectDir)..\total1.txt">nul
type nul>"$(ProjectDir)..\total2.txt"
for /D %%D in ("$(ProjectDir)..\files1\*.*") do (
    if /I not "%%~nxD"=="Dir1" if /I not "%%~nxD"=="Dir2" (
        pushd "%%D"
        for /r %%f in ("*.txt") do type "%%f">>"$(ProjectDir)..\total2.txt"
        popd
    )
)

xcopy "%Files%\tmp\*" "Target\*" /E /Y /R /Q>nul
xcopy "%Files%\tmp\*" "Target2\SubFolder" /E /I /Y /R /Q>nul

set "PYTHON=C:\Program Files\Python39\python.exe"
xcopy "..\.hg" "tmp\hg-repo\.hg\" /E /I /Q /H /Y /R

if exist "%Files%\tmp\file.txt" del "%Files%\tmp\file.txt"
goto :Success


:Error
exit /b 1
:Success
exit /b 0
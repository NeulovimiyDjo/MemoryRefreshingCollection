@echo off
set "FILES_DIR=c:/tmp/some_dir"
(mkdir %FILES_DIR%)>nul 2>&1
FOR /F "skip=2 eol=: delims=" %%F IN ('DIR /B /O:-D %FILES_DIR%\*.zip') DO (
    echo Deleting old zip file: %FILES_DIR%\%%F
    del "%FILES_DIR%\%%F"
) 2>&1

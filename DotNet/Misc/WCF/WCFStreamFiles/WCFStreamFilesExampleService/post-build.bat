cd   /D   %~dp0


xcopy   /Y /Q /I /F  bin\debug\WCFStreamFilesExampleService.dll   C:\inetpub\wwwroot\some_subfolder\WCFStreamFilesExampleService\bin\
xcopy   /Y /Q /I /F  bin\debug\WCFStreamFilesExampleService.pdb   C:\inetpub\wwwroot\some_subfolder\WCFStreamFilesExampleService\bin\

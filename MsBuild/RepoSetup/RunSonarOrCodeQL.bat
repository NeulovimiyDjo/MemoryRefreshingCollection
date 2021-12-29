#docker run -d --name sonarqube -p 9000:9000 -p 9092:9092 sonarqube
#dotnet tool install --global dotnet-sonarscanner


cd c:\repo
dotnet build-server shutdown
dotnet sonarscanner begin /k:"test1" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="123" /d:sonar.cs.opencover.reportsPaths=".\tests\TestResults\coverage.opencover.xml" /d:sonar.coverage.exclusions="\samples\"
dotnet build -c Release
dotnet sonarscanner end /d:sonar.login="123"



cd c:\repo
&"..\..\codeql-home\codeql\codeql.exe" database create ../../qldb --language=csharp --command='dotnet build /t:rebuild'
&"..\..\codeql-home\codeql\codeql.exe" database analyze ../../qldb ../../codeql-home/codeql-repo/csharp/ql/src/codeql-suites/csharp-code-scanning.qls --format=sarif-latest --output=../../qlres.sarif
rm -r ./bin/Desktop
dotnet publish -c Release --force -f netcoreapp3.1 --self-contained true -r win-x64 -o ./bin/Desktop ./UI/QRCopyPaste.Desktop/QRCopyPaste.Desktop.csproj
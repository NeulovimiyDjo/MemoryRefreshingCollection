rm -r ./bin/Desktop
dotnet publish -c Release --force -f netcoreapp3.1 --self-contained false -o ./bin/Desktop ./UI/QRCopyPaste.Desktop/QRCopyPaste.Desktop.csproj
FROM mcr.microsoft.com/windows:2004-amd64
RUN mkdir installers
COPY ./installers/ /installers
RUN ["c:/installers/ChromeSetup.exe", "/silent", "/install"]
CMD ["cmd.exe"]
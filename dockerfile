FROM mcr.microsoft.com/dotnet/sdk:6.0
WORKDIR /reldist

COPY /P6 ./P6
COPY /PythonScripts ./PythonScripts

WORKDIR /reldist/P6/Main
RUN dotnet restore
RUN dotnet publish -c Release 

WORKDIR /reldist/P6/Main/bin/Release/net6.0
ENTRYPOINT ["dotnet", "Main.dll"]
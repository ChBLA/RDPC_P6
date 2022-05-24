FROM mcr.microsoft.com/dotnet/sdk:6.0
WORKDIR /reldist

COPY /P6 ./P6
COPY /PythonScripts ./PythonScripts

RUN apt-get update && \
    apt-get -y install python3 python3-venv python3-pip && \
    pip install -r PythonScripts/requirements.txt

WORKDIR /reldist/P6/Main
RUN dotnet restore
RUN dotnet publish -c Release 

WORKDIR /reldist/P6/Main/bin/Release/net6.0
ENTRYPOINT ["dotnet", "Main.dll"]
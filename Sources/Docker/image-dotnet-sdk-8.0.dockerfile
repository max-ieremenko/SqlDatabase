FROM mcr.microsoft.com/dotnet/sdk:8.0

RUN apt-get update && \
    curl -s -L https://github.com/PowerShell/PowerShell/releases/download/v7.4.12/powershell_7.4.12-1.deb_amd64.deb --output powershell.deb && \
    dpkg -i powershell.deb && \
    apt-get install -f  && \
    rm -f powershell.deb
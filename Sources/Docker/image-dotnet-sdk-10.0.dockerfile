FROM mcr.microsoft.com/dotnet/sdk:10.0

RUN apt-get update && \
    curl -s -L https://github.com/PowerShell/PowerShell/releases/download/v7.6.0-preview.4/powershell-preview_7.6.0-preview.4-1.deb_amd64.deb --output powershell.deb && \
    dpkg -i powershell.deb && \
    apt-get install -f  && \
    rm -f powershell.deb
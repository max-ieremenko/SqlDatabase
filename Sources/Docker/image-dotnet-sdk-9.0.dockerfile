FROM mcr.microsoft.com/dotnet/sdk:9.0

RUN apt-get update && \
    curl -L https://github.com/PowerShell/PowerShell/releases/download/v7.5.0-preview.5/powershell-preview_7.5.0-preview.5-1.deb_amd64.deb --output powershell.deb && \
    dpkg -i powershell.deb && \
    apt-get install -f  && \
    rm -f powershell.deb
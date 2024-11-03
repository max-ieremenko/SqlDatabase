FROM mcr.microsoft.com/dotnet/runtime:8.0

RUN apt-get update && \
    apt-get install -y curl && \
    curl -L https://github.com/PowerShell/PowerShell/releases/download/v7.4.6/powershell_7.4.6-1.deb_amd64.deb --output powershell.deb && \
    dpkg -i powershell.deb && \
    apt-get install -f  && \
    rm -f powershell.deb
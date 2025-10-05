FROM mcr.microsoft.com/dotnet/runtime:9.0

RUN apt-get update && \
    apt-get install -y curl && \
    curl -s -L https://github.com/PowerShell/PowerShell/releases/download/v7.5.3/powershell_7.5.3-1.deb_amd64.deb --output powershell.deb && \
    dpkg -i powershell.deb && \
    apt-get install -f  && \
    rm -f powershell.deb
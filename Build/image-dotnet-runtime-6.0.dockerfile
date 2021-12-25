FROM mcr.microsoft.com/dotnet/runtime:6.0

RUN apt-get update && \
    apt-get install -y liblttng-ust0 curl && \
    curl -L https://github.com/PowerShell/PowerShell/releases/download/v7.2.1/powershell_7.2.1-1.deb_amd64.deb --output powershell.deb && \
    dpkg -i powershell.deb && \
    apt-get install -f  && \
    rm -f powershell.deb
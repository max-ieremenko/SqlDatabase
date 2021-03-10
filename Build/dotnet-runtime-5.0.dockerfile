FROM mcr.microsoft.com/dotnet/runtime:5.0

RUN apt-get update && \
    apt-get install -y liblttng-ust0 curl && \
    curl -L https://github.com/PowerShell/PowerShell/releases/download/v7.1.2/powershell_7.1.2-1.debian.10_amd64.deb --output powershell_7.1.2-1.debian.10_amd64.deb && \
    dpkg -i powershell_7.1.2-1.debian.10_amd64.deb && \
    apt-get install -f  && \
    rm -f powershell_7.1.2-1.debian.10_amd64.deb
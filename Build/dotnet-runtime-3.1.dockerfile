FROM mcr.microsoft.com/dotnet/core/runtime:3.1

RUN apt-get update && \
    apt-get install -y liblttng-ust0 && \
    curl -L https://github.com/PowerShell/PowerShell/releases/download/v7.0.5/powershell_7.0.5-1.debian.10_amd64.deb --output powershell_7.0.5-1.debian.10_amd64.deb && \
    dpkg -i powershell_7.0.5-1.debian.10_amd64.deb && \
    apt-get install -f  && \
    rm -f powershell_7.0.5-1.debian.10_amd64.deb
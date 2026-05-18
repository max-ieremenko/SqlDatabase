ARG BASE_IMAGE=mcr.microsoft.com/dotnet/sdk:10.0-noble-amd64
FROM ${BASE_IMAGE}

ARG PWSH_VERSION=7.6.1
RUN apt-get update && \
    apt-get install --no-install-recommends -y curl && \
    curl -s -L https://github.com/PowerShell/PowerShell/releases/download/v${PWSH_VERSION}/powershell_${PWSH_VERSION}-1.deb_amd64.deb --output powershell.deb && \
    ls -l && \
    dpkg -i powershell.deb && \
    apt-get install -f  && \
    rm -f powershell.deb
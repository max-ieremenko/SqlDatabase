FROM microsoft/dotnet:2.1-sdk

RUN curl -L https://github.com/PowerShell/PowerShell/releases/download/v6.2.7/powershell_6.2.7-1.debian.9_amd64.deb --output powershell_6.2.7-1.debian.9_amd64.deb && \
    dpkg -i powershell_6.2.7-1.debian.9_amd64.deb && \
    apt-get install -f  && \
    rm -f powershell_6.2.7-1.debian.9_amd64.deb
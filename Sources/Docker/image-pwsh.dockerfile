ARG OS_VERSION=24.04
FROM ubuntu:${OS_VERSION}

ARG OS_VERSION=24.04
ARG PWSH_VERSION=7.6.1
ARG PWSH_PACKAGE=powershell
ARG PWSH_ALIAS=powershell
ARG PWSH_ALIAS_COMMAND=pwsh
RUN export DEBIAN_FRONTEND=noninteractive && \
    apt-get update && \
    apt-get install --no-install-recommends -y ca-certificates curl && \
    curl -s -L https://packages.microsoft.com/config/ubuntu/${OS_VERSION}/packages-microsoft-prod.deb --output microsoft-prod.deb && \
    dpkg -i microsoft-prod.deb && \
    rm -f microsoft-prod.deb && \
    apt-get update && \
    apt list -a ${PWSH_PACKAGE} && \
    apt-get install -y --no-install-recommends ${PWSH_PACKAGE}=${PWSH_VERSION}-1.deb && \
    ln -s $(which ${PWSH_ALIAS_COMMAND}) /usr/bin/${PWSH_ALIAS}
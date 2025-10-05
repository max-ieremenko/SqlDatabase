## System requirements

build.ps1 is designed to run on windows

- PowerShell Desktop 5.1
- PowerShell [7.6.0](https://github.com/PowerShell/PowerShell/releases/tag/v7.6.0-preview.4) for .net 10.0 tests
- PowerShell [7.5.3](https://github.com/PowerShell/PowerShell/releases/tag/v7.5.3) for .net 9.0 tests
- PowerShell [7.4.12](https://github.com/PowerShell/PowerShell/releases/tag/v7.4.12) for .net 8.0 tests
- Install-Module -Name [InvokeBuild](https://www.powershellgallery.com/packages/InvokeBuild)
- Install-Module -Name [ThirdPartyLibraries](https://www.powershellgallery.com/packages/ThirdPartyLibraries)
- .net framework 4.7.2+ sdk
- .net 10.0 sdk
- docker, switched to linux containers

## How to build

```powershell
PS> git clone https://github.com/max-ieremenko/SqlDatabase.git

# build required docker images
PS> .\Build\create-images.ps1

# run build
PS> .\Build\build.ps1

# show build output
PS> ls .\bin\artifacts
```

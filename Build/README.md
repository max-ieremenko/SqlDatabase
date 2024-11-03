## System requirements

build.ps1 is designed to run on windows

- PowerShell Desktop 5.1
- PowerShell [7.3.0](https://github.com/PowerShell/PowerShell/releases/tag/v7.3.0) for .net 8.0 tests
- PowerShell [7.2.1](https://github.com/PowerShell/PowerShell/releases/tag/v7.2.1) for .net 6.0 tests
- Install-Module -Name [InvokeBuild](https://www.powershellgallery.com/packages/InvokeBuild/5.11.1) -RequiredVersion 5.11.1
- Install-Module -Name [ThirdPartyLibraries](https://www.powershellgallery.com/packages/ThirdPartyLibraries/3.5.1) -RequiredVersion 3.5.1
- .net framework 4.7.2+ sdk
- .net 8.0 sdk
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

## System requirements

build.ps1 is designed to run on windows

- PowerShell Desktop 5.1
- PowerShell Core 6.* (.net core 2.2 tests), versions 7+ are optional
- InvokeBuild 5.7.2
- ThirdPartyLibraries https://www.nuget.org/packages/ThirdPartyLibraries.GlobalTool/
- .net framework 4.7.2+ sdk
- .net core 2.2 sdk
- .net core 3.1 sdk
- .net 5.0 sdk
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

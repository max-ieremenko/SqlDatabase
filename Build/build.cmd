@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)
 
set PackageVersion=
if "%PackageVersion%" == "" (
   set PackageVersion=1.0.0
)

set nuget=
if "%nuget%" == "" (
	set nuget=nuget
)

set MsBuildExe=
if "%MsBuildExe%" == "" (
	set "MsBuildExe=C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
)

"%MsBuildExe%" /target:main build/build.targets

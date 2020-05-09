@ECHO OFF

ECHO Setting Path...
SET PATH=%PATH%;D:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin

ECHO Restoring NuGet Packages...
nuget.exe restore Chorg.sln

ECHO Cleaning Chorg...
msbuild.exe /p:Configuration=Release /v:n /t:clean

ECHO Building Chorg...
msbuild.exe /p:Configuration=Release /v:n

ECHO Cleaning Artifacts...
CD "Chorg\.out\Release\"

RMDIR /Q /S nl
DEL /F *.xml
DEL /F *.config
DEL /F *.pdb

explorer.exe %CD%
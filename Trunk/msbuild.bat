@echo off
setlocal ENABLEEXTENSIONS

:: Set this to the version of MSBuild you wish to use

set ToolsVersion=14.0

:: Read the MSBuildToolsPath for ToolsVersion specified above

set RegKey=HKLM\SOFTWARE\Microsoft\MSBuild\ToolsVersions\%ToolsVersion%
set RegValue=MSBuildToolsPath
 
FOR /F "tokens=2*" %%A IN ('REG QUERY %RegKey% /v %RegValue% 2^>nul') DO set MSBuildToolsPath=%%B

IF ["%MSBuildToolsPath%"] == [""] GOTO NotFound

:: Actually execute msbuild with the command line parameters specified

"%MSBuildToolsPath%msbuild.exe" %*

goto end

:NotFound

echo Unable to read %RegKey%@%RegValue% from the registry.
echo MSBuild Tools Version %ToolsVersion% might not be installed.

:end

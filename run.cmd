@echo off
setlocal enabledelayedexpansion

:menu
echo.
echo Menu:
echo 1. Build and Publish InnoAndLogic.Shared NuGet Package
echo 2. Build and Publish InnoAndLogic.Persistence NuGet Package
echo Q. Quit
echo.
set /p choice=Enter your choice: 

if /i "!choice!" == "Q" goto quit
if /i "!choice!" == "q" goto quit
if "!choice!" == "1" goto build_and_publish_shared
if "!choice!" == "2" goto build_and_publish_persistence

echo Invalid choice. Please try again.
goto menu

:build_and_publish_shared
for /f "tokens=2 delims=>" %%A in ('findstr /i "<Version>" InnoAndLogic.Shared\InnoAndLogic.Shared.csproj') do set version=%%A
set version=!version:~0,-9!

if not defined version (
    echo Failed to determine version from InnoAndLogic.Shared.csproj.
    goto quit
)

echo Building the solution...
dotnet build -c Release || goto quit

echo Packing InnoAndLogic.Shared...
dotnet pack InnoAndLogic.Shared -c Release || goto quit

echo Pushing NuGet package...
nuget push InnoAndLogic.Shared\bin\Release\InnoAndLogic.Shared.!version!.nupkg -Source https://api.nuget.org/v3/index.json || goto quit

echo.
echo Package published successfully.
goto menu

:build_and_publish_persistence
for /f "tokens=2 delims=>" %%A in ('findstr /i "<Version>" InnoAndLogic.Persistence\InnoAndLogic.Persistence.csproj') do set version=%%A
set version=!version:~0,-9!

if not defined version (
    echo Failed to determine version from InnoAndLogic.Persistence.csproj.
    goto quit
)

echo Building the solution...
dotnet build -c Release || goto quit

echo Packing InnoAndLogic.Persistence...
dotnet pack InnoAndLogic.Persistence -c Release || goto quit

echo Pushing NuGet package...
nuget push InnoAndLogic.Persistence\bin\Release\InnoAndLogic.Persistence.!version!.nupkg -Source https://api.nuget.org/v3/index.json || goto quit

echo.
echo Package published successfully.
goto menu

:quit
echo.
echo Exiting...
endlocal
exit /b
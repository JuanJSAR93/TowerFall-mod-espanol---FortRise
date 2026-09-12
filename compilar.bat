@echo off
setlocal EnableExtensions EnableDelayedExpansion

title Compilar TowerFallLocalization

set "ROOT=%~dp0"
set "GAME=C:\Program Files (x86)\Steam\steamapps\common\TowerFall - FortRise"
if not "%~1"=="" set "GAME=%~1"

set "MOD=%GAME%\Mods\TowerFallLocalization"
set "DOTNET=%ProgramFiles%\dotnet\dotnet.exe"
set "SDK_VERSION=10.0.401"
set "REF_VERSION=10.0.12"
set "CSC=%ProgramFiles%\dotnet\sdk\%SDK_VERSION%\Roslyn\bincore\csc.dll"
set "REF=%ProgramFiles%\dotnet\packs\Microsoft.NETCore.App.Ref\%REF_VERSION%\ref\net10.0"
set "SOURCE=%ROOT%src\TowerFallEspanolModule.cs"
set "JSON=%ROOT%translations.json"
set "BUILD=%ROOT%build"
set "OUTPUT=%BUILD%\TowerFallLocalization.dll"

if not exist "%DOTNET%" goto :missing
if not exist "%CSC%" goto :missing
if not exist "%REF%" goto :missing
if not exist "%SOURCE%" goto :missing
if not exist "%JSON%" goto :missing
if not exist "%GAME%\TowerFall.Patch.dll" goto :missing
if not exist "%GAME%\0Harmony.dll" goto :missing
if not exist "%GAME%\Microsoft.Extensions.Logging.Abstractions.dll" goto :missing

if not exist "%BUILD%" mkdir "%BUILD%"
if not exist "%MOD%" mkdir "%MOD%"

set "REFS="
for %%F in ("%REF%\*.dll") do set "REFS=!REFS! /reference:%%~fF"

echo Compilando TowerFallLocalization.dll...
"%DOTNET%" "%CSC%" /nologo /target:library /out:"%OUTPUT%" "%SOURCE%" !REFS! ^
 /reference:"%GAME%\TowerFall.Patch.dll" ^
 /reference:"%GAME%\0Harmony.dll" ^
 /reference:"%GAME%\Microsoft.Extensions.Logging.Abstractions.dll"

if errorlevel 1 (
    echo.
    echo ERROR: la compilacion fallo.
    pause
    exit /b 1
)

copy /y "%OUTPUT%" "%MOD%\TowerFallLocalization.dll" >nul
if errorlevel 1 goto :install_error
copy /y "%JSON%" "%MOD%\translations.json" >nul
if errorlevel 1 goto :install_error

echo.
echo Compilacion e instalacion completadas.
echo DLL: %MOD%\TowerFallLocalization.dll
echo JSON: %MOD%\translations.json
echo El juego no se ha ejecutado.
pause
exit /b 0

:missing
echo.
echo ERROR: no se encontro un archivo o ruta necesaria.
echo Ruta del juego usada: %GAME%
echo Puedes pasar otra ruta como primer argumento, entre comillas.
pause
exit /b 1

:install_error
echo.
echo ERROR: se compilo, pero no se pudo copiar el mod instalado.
echo Comprueba que TowerFall no este ejecutandose y que la ruta sea correcta.
pause
exit /b 1

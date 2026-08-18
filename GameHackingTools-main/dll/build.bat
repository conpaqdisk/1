@echo off
:: ============================================================================
:: GameHackingTools - Build Script
:: Visual Studio 2026 için DLL derleme
:: ============================================================================

setlocal

:: VS 2026 (18) Professional
if exist "C:\Program Files\Microsoft Visual Studio\18\Professional\Common7\Tools\VsDevCmd.bat" (
    call "C:\Program Files\Microsoft Visual Studio\18\Professional\Common7\Tools\VsDevCmd.bat" -arch=x86 >nul 2>&1
    goto build
)

:: VS 2026 (18) Community
if exist "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\Tools\VsDevCmd.bat" (
    call "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\Tools\VsDevCmd.bat" -arch=x86 >nul 2>&1
    goto build
)

echo Visual Studio bulunamadi!
exit /b 1

:build
echo Derleniyor...
msbuild GameHackingTools.vcxproj /p:Configuration=Release /p:Platform=Win32 /v:m
echo.
echo Tamamlandi! Cikti: Release\GameHackingTools.dll

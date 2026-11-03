@echo off
rem Starts Uninstall.ps1 of this folder, whatever the execution policy of PowerShell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Uninstall.ps1" %*
pause

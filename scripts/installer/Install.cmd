@echo off
rem Starts Install.ps1 of this folder, whatever the execution policy of PowerShell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Install.ps1"
pause

@echo off
cd /d "%~dp0\BnSVN-Discord-Bot"
SET DOTNET_CLI_TELEMETRY_OPTOUT=1
echo Compiling source code...
dotnet run
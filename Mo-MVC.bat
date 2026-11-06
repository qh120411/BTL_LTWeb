@echo off
cd /d "%~dp0"
echo Mo website tai http://localhost:5124 sau khi console hien Now listening.
dotnet run --launch-profile http
pause

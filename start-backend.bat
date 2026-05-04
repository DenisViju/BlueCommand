@echo off
cd /d "%~dp0backend"
set DB_CONNECTION_STRING=Host=localhost;Port=5433;Database=bluecommand;Username=postgres;Password=postgres
set JWT_SECRET=bluecommand-secret-key-schimba-asta-in-productie
dotnet run --project BlueCommand.API\BlueCommand.API.csproj
pause
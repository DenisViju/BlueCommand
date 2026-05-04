FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/BlueCommand.sln backend/BlueCommand.sln
COPY backend/BlueCommand.Domain/BlueCommand.Domain.csproj backend/BlueCommand.Domain/
COPY backend/BlueCommand.Application/BlueCommand.Application.csproj backend/BlueCommand.Application/
COPY backend/BlueCommand.Infrastructure/BlueCommand.Infrastructure.csproj backend/BlueCommand.Infrastructure/
COPY backend/BlueCommand.API/BlueCommand.API.csproj backend/BlueCommand.API/

RUN dotnet restore backend/BlueCommand.sln

COPY backend/ backend/
WORKDIR /src/backend/BlueCommand.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "BlueCommand.API.dll"]


# Multi-stage Dockerfile for Backend (.NET 8 Web API)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/backend/BIDADYUManagement.Api/BIDADYUManagement.Api.csproj", "src/backend/BIDADYUManagement.Api/"]
COPY ["src/backend/BIDADYUManagement.Application/BIDADYUManagement.Application.csproj", "src/backend/BIDADYUManagement.Application/"]
COPY ["src/backend/BIDADYUManagement.Domain/BIDADYUManagement.Domain.csproj", "src/backend/BIDADYUManagement.Domain/"]
COPY ["src/backend/BIDADYUManagement.Infrastructure/BIDADYUManagement.Infrastructure.csproj", "src/backend/BIDADYUManagement.Infrastructure/"]

RUN dotnet restore "src/backend/BIDADYUManagement.Api/BIDADYUManagement.Api.csproj"

# Copy full source and publish
COPY src/backend/ src/backend/
WORKDIR "/src/src/backend/BIDADYUManagement.Api"
RUN dotnet publish "BIDADYUManagement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose HTTP port
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "BIDADYUManagement.Api.dll"]

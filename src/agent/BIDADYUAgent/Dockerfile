# Multi-stage Dockerfile for Agent (Windows)
FROM mcr.microsoft.com/dotnet/sdk:8.0-windowsservercore-ltsc2022 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY src/agent/BIDADYUAgent/BIDADYUAgent.csproj ./
RUN dotnet restore BIDADYUAgent.csproj

# Copy the rest of the source code
COPY src/agent/BIDADYUAgent/. ./

# Publish as single file (no self-contained)
RUN dotnet publish BIDADYUAgent.csproj -c Release -o /app/publish /p:PublishSingleFile=true /p:SelfContained=false /p:UseAppHost=true

FROM mcr.microsoft.com/dotnet/runtime:8.0-windowsservercore-ltsc2022 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose a dummy port if needed (agent runs without listening)
EXPOSE 5001

ENTRYPOINT ["BIDADYUAgent.exe"]

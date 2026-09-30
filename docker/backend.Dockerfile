FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/backend/BIDADYUManagement.Api/BIDADYUManagement.Api.csproj", "backend/BIDADYUManagement.Api/"]
COPY ["src/backend/BIDADYUManagement.Application/BIDADYUManagement.Application.csproj", "backend/BIDADYUManagement.Application/"]
COPY ["src/backend/BIDADYUManagement.Domain/BIDADYUManagement.Domain.csproj", "backend/BIDADYUManagement.Domain/"]
COPY ["src/backend/BIDADYUManagement.Infrastructure/BIDADYUManagement.Infrastructure.csproj", "backend/BIDADYUManagement.Infrastructure/"]

RUN dotnet restore "backend/BIDADYUManagement.Api/BIDADYUManagement.Api.csproj"

COPY src/backend/ backend/
WORKDIR "/src/backend/BIDADYUManagement.Api"
RUN dotnet build "BIDADYUManagement.Api.csproj" -c Release -o /app/build
RUN dotnet publish "BIDADYUManagement.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BIDADYUManagement.Api.dll"]

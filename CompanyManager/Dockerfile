# Backend Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar arquivos de projeto
COPY ["CompanyManager.API/CompanyManager.API.csproj", "CompanyManager.API/"]
COPY ["CompanyManager.Application/CompanyManager.Application.csproj", "CompanyManager.Application/"]
COPY ["CompanyManager.Domain/CompanyManager.Domain.csproj", "CompanyManager.Domain/"]
COPY ["CompanyManager.Infrastructure/CompanyManager.Infrastructure.csproj", "CompanyManager.Infrastructure/"]

# Restaurar dependências
RUN dotnet restore "CompanyManager.API/CompanyManager.API.csproj"

# Copiar código fonte
COPY . .
WORKDIR "/src/CompanyManager.API"

# Build da aplicação
RUN dotnet build "CompanyManager.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CompanyManager.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CompanyManager.API.dll"]

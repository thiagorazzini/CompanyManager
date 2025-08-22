FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CompanyManager.API/CompanyManager.API.csproj", "CompanyManager.API/"]
COPY ["CompanyManager.Application/CompanyManager.Application.csproj", "CompanyManager.Application/"]
COPY ["CompanyManager.Domain/CompanyManager.Domain.csproj", "CompanyManager.Domain/"]
COPY ["CompanyManager.Infrastructure/CompanyManager.Infrastructure.csproj", "CompanyManager.Infrastructure/"]

RUN dotnet restore "CompanyManager.API/CompanyManager.API.csproj"
COPY . .
WORKDIR "/src/CompanyManager.API"
RUN dotnet build "CompanyManager.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CompanyManager.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CompanyManager.API.dll"]


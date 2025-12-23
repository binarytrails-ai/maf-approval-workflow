# Dockerfile for .NET API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ContosoBikestore.Agent.Host/ContosoBikestore.Agent.Host.csproj", "ContosoBikestore.Agent.Host/"]
RUN dotnet restore "ContosoBikestore.Agent.Host/ContosoBikestore.Agent.Host.csproj"
COPY . .
WORKDIR "/src/ContosoBikestore.Agent.Host"
RUN dotnet build "ContosoBikestore.Agent.Host.csproj" -c Release -o /app/build
RUN dotnet publish "ContosoBikestore.Agent.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ContosoBikestore.Agent.Host.dll"]

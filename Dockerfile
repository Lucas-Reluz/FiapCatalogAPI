# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/CatalogAPI.Api/CatalogAPI.Api.csproj", "CatalogAPI.Api/"]
COPY ["src/CatalogAPI.Application/CatalogAPI.Application.csproj", "CatalogAPI.Application/"]
COPY ["src/CatalogAPI.Domain/CatalogAPI.Domain.csproj", "CatalogAPI.Domain/"]
COPY ["src/CatalogAPI.Infrastructure/CatalogAPI.Infrastructure.csproj", "CatalogAPI.Infrastructure/"]

RUN dotnet restore "CatalogAPI.Api/CatalogAPI.Api.csproj"

# Copy all source code
COPY src/ .

# Build and publish
WORKDIR "/src/CatalogAPI.Api"
RUN dotnet build "CatalogAPI.Api.csproj" -c Release -o /app/build
RUN dotnet publish "CatalogAPI.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Entry point
ENTRYPOINT ["dotnet", "CatalogAPI.Api.dll"]

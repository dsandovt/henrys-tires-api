FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and projects
COPY ["HenryTires.Inventory.sln", "./"]
COPY ["src/HenryTires.Inventory.Domain/HenryTires.Inventory.Domain.csproj", "src/HenryTires.Inventory.Domain/"]
COPY ["src/HenryTires.Inventory.Application/HenryTires.Inventory.Application.csproj", "src/HenryTires.Inventory.Application/"]
COPY ["src/HenryTires.Inventory.Infrastructure/HenryTires.Inventory.Infrastructure.csproj", "src/HenryTires.Inventory.Infrastructure/"]
COPY ["src/HenryTires.Inventory.Api/HenryTires.Inventory.Api.csproj", "src/HenryTires.Inventory.Api/"]

# Restore dependencies
RUN dotnet restore "HenryTires.Inventory.sln"

# Copy all source code
COPY . .

# Build
WORKDIR "/src/src/HenryTires.Inventory.Api"
RUN dotnet build "HenryTires.Inventory.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HenryTires.Inventory.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HenryTires.Inventory.Api.dll"]

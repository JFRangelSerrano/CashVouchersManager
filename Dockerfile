# Use .NET 8 SDK for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["CashVouchersManager.API/CashVouchersManager.API.csproj", "CashVouchersManager.API/"]
COPY ["CashVouchersManager.Application/CashVouchersManager.Application.csproj", "CashVouchersManager.Application/"]
COPY ["CashVouchersManager.Domain/CashVouchersManager.Domain.csproj", "CashVouchersManager.Domain/"]
COPY ["CashVouchersManager.DTO/CashVouchersManager.DTO.csproj", "CashVouchersManager.DTO/"]
COPY ["CashVouchersManager.Infrastructure/CashVouchersManager.Infrastructure.csproj", "CashVouchersManager.Infrastructure/"]
RUN dotnet restore "CashVouchersManager.API/CashVouchersManager.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/CashVouchersManager.API"
RUN dotnet publish "CashVouchersManager.API.csproj" -c Release -o /app/publish --no-restore

# Use .NET 8 Runtime for final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Create data directory for SQLite
RUN mkdir -p /data

ENV ASPNETCORE_URLS=http://+:${PORT:-5000}
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "CashVouchersManager.API.dll"]

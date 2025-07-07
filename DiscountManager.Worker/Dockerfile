# Base runtime image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app
# Optional non-root user for security
RUN useradd -m appuser
USER appuser

# SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy and restore as separate layers for cache
COPY ["DiscountManager.Worker/DiscountManager.Worker.csproj", "DiscountManager.Worker/"]
RUN dotnet restore "DiscountManager.Worker/DiscountManager.Worker.csproj"

# Copy all source
COPY . .

# Build the application
WORKDIR "/src/DiscountManager.Worker"
RUN dotnet build "DiscountManager.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the app (framework-dependent)
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DiscountManager.Worker.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-self-contained

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Entrypoint
ENTRYPOINT ["dotnet", "DiscountManager.Worker.dll"]

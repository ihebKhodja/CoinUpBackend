# # See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# # This stage is used when running from VS in fast mode (Default for Debug configuration)
# FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# USER $APP_UID
# WORKDIR /app
# EXPOSE 8080
# EXPOSE 8081


# # This stage is used to build the service project
# FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# ARG BUILD_CONFIGURATION=Release
# WORKDIR /src

# COPY CoinUp.Shared/CoinUp.Shared.csproj CoinUp.Shared/
# COPY CoinUpAPI/CoinUpAPI.csproj CoinUpAPI/
# RUN dotnet restore "CoinUpAPI/CoinUpAPI.csproj"

# COPY . .
# WORKDIR /src/CoinUpAPI
# RUN dotnet build "CoinUpAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# # This stage is used to publish the service project to be copied to the final stage
# FROM build AS publish
# ARG BUILD_CONFIGURATION=Release
# RUN dotnet publish "CoinUpAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# # This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
# FROM base AS final
# WORKDIR /app
# COPY --from=publish /app/publish .
# ENTRYPOINT ["dotnet", "CoinUpAPI.dll"]

# ---------------------------
# Base runtime image
# ---------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ---------------------------
# Build stage
# ---------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy shared project first
COPY CoinUp.Shared/CoinUp.Shared.csproj CoinUp.Shared/

# Copy API project
COPY CoinUpAPI/CoinUpAPI.csproj CoinUpAPI/
RUN dotnet restore CoinUpAPI/CoinUpAPI.csproj

# Copy full source
COPY . .

# Build and publish API
WORKDIR /src/CoinUpAPI
RUN dotnet build CoinUpAPI.csproj -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish CoinUpAPI.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---------------------------
# Final image
# ---------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CoinUpAPI.dll"]

# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
# GitVersion can't run in this container (no .git), so the version is computed on the host and passed in.
ARG BUILD_VERSION=0.0.0
RUN apk add --no-cache bash curl && curl -fsSL https://vite.plus | bash
ENV PATH="/root/.local/share/vite-plus/bin:${PATH}"
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["LadirchenApp.Webhost/LadirchenApp.Webhost.csproj", "LadirchenApp.Webhost/"]
RUN dotnet restore "./LadirchenApp.Webhost/LadirchenApp.Webhost.csproj"
COPY . .
WORKDIR "/src/LadirchenApp.Webhost"
RUN dotnet build "./LadirchenApp.Webhost.csproj" -c $BUILD_CONFIGURATION -o /app/build /p:Version=$BUILD_VERSION

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG BUILD_VERSION=0.0.0
RUN dotnet publish "./LadirchenApp.Webhost.csproj" \
  -c $BUILD_CONFIGURATION \
  -o /app/publish \
  /p:UseAppHost=false \
  /p:Version=$BUILD_VERSION

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LadirchenApp.Webhost.dll"]

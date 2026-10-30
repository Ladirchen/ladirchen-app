# syntax=docker/dockerfile:1.27.1@sha256:4edf897a3ffa55b89f906fc8cc78afdb3f1834cc9c7083565e611a8a7d5fe99e

FROM node:24.22.1-alpine3.24@sha256:06d7adf3e0ef61a13f1d28682726366f97a15aa1888c934d3ecec86c33918b53 AS node

FROM mcr.microsoft.com/dotnet/sdk:10.0.403-alpine3.24@sha256:cc02b814237a76c4735fb6eeee87181597b5e56f2ce458a435e527674d91f4d7 AS build
ARG BUILD_CONFIGURATION=Release
ARG BUILD_VERSION=0.0.0
ARG ENABLE_VUE_DEVTOOLS=false
ENV CI=true \
  ENABLE_VUE_DEVTOOLS=$ENABLE_VUE_DEVTOOLS

COPY --from=node /usr/local/ /usr/local/
RUN npm install --global pnpm@12.15.0

WORKDIR /src
COPY ["nuget.config", "Directory.Build.props", "Build.GitVersion.props", "Build.NSwag.props", "./"]
COPY ["LadirchenApp.Prototypes/LadirchenApp.Prototypes.csproj", "LadirchenApp.Prototypes/"]
COPY ["LadirchenApp.Domain/LadirchenApp.Domain.csproj", "LadirchenApp.Domain/"]
COPY ["LadirchenApp.Application/LadirchenApp.Application.csproj", "LadirchenApp.Application/"]
COPY ["LadirchenApp.Webhost/LadirchenApp.Webhost.csproj", "LadirchenApp.Webhost/"]
RUN --mount=type=cache,target=/root/.nuget/packages \
  dotnet restore "LadirchenApp.Webhost/LadirchenApp.Webhost.csproj"

COPY . .
RUN --mount=type=cache,target=/root/.nuget/packages \
  --mount=type=cache,target=/root/.local/share/pnpm/store \
  dotnet publish "LadirchenApp.Webhost/LadirchenApp.Webhost.csproj" \
  --configuration $BUILD_CONFIGURATION \
  --output /app/publish \
  --no-restore \
  /p:UseAppHost=false \
  /p:Version=$BUILD_VERSION

FROM mcr.microsoft.com/dotnet/aspnet:10.0.14-alpine3.24@sha256:254fe017495996d3abcc487ffacdcce2b73affb380d3af87af12c8dff41fe0b0 AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production \
  PORT=8080
EXPOSE 8080 8081
RUN apk add --no-cache krb5-libs
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "LadirchenApp.Webhost.dll"]

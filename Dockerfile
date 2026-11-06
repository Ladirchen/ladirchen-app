FROM mcr.microsoft.com/dotnet/sdk:10.0.403-alpine3.24 AS build
ARG BUILD_CONFIGURATION=Release
ARG BUILD_VERSION=0.0.0
ARG ENABLE_VUE_DEVTOOLS=false
ENV CI=true \
  ENABLE_VUE_DEVTOOLS=$ENABLE_VUE_DEVTOOLS \
  VP_HOME=/root/.vite-plus \
  VP_NODE_MANAGER=yes
ENV PATH="${VP_HOME}/bin:${PATH}"

RUN apk add --no-cache bash curl libstdc++ \
  && curl -fsSL https://vite.plus -o /tmp/install-vite-plus.sh \
  && bash /tmp/install-vite-plus.sh \
  && rm /tmp/install-vite-plus.sh

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
  /p:UseAppHost=false \
  /p:Version=$BUILD_VERSION

FROM mcr.microsoft.com/dotnet/aspnet:10.0.14-alpine3.24 AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production \
  DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
  PORT=8080
EXPOSE 8080 8081
RUN apk add --no-cache icu-data-full icu-libs krb5-libs tzdata
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "LadirchenApp.Webhost.dll"]

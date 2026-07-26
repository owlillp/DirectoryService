FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5209

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG NUGET_USERNAME
ARG NUGET_PASSWORD
WORKDIR /app

COPY ["nuget.config", "."]
COPY [".editorconfig", "."]

RUN dotnet nuget update source github \
    --username $NUGET_USERNAME \
    --password $NUGET_PASSWORD \
    --store-password-in-clear-text \
    --configfile /app/nuget.config

COPY ["DirectoryService/", "DirectoryService/"]

RUN dotnet restore "DirectoryService/src/DirectoryService.Presentation/DirectoryService.Presentation.csproj"

WORKDIR /app/DirectoryService/src/DirectoryService.Presentation

RUN dotnet build "./DirectoryService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DirectoryService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DirectoryService.Presentation.dll"]


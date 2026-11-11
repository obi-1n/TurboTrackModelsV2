FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TurboTrackModels-Dotnet Core/TurboTrackModels-Dotnet Core.csproj", "TurboTrackModels-Dotnet Core/"]
RUN dotnet restore "TurboTrackModels-Dotnet Core/TurboTrackModels-Dotnet Core.csproj"
COPY . .
WORKDIR "/src/TurboTrackModels-Dotnet Core"
RUN dotnet build "./TurboTrackModels-Dotnet Core.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TurboTrackModels-Dotnet Core.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TurboTrackModels-Dotnet Core.dll"]

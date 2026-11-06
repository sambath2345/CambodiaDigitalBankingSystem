FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
RUN dotnet restore "CambodiaDigitalBankingSystem.API/CambodiaDigitalBankingSystem.API.csproj"
WORKDIR "/src/CambodiaDigitalBankingSystem.API"
RUN dotnet build "./CambodiaDigitalBankingSystem.API.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/build

FROM build AS development
WORKDIR /src/CambodiaDigitalBankingSystem.API
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "watch", "run", "--no-launch-profile", "--urls", "http://0.0.0.0:8080"]

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CambodiaDigitalBankingSystem.API.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "CambodiaDigitalBankingSystem.API.dll"]

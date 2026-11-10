FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore separately so dependency layers are reusable between source changes.
COPY RaceDay.Api/RaceDay.Api.csproj RaceDay.Api/
COPY RaceDay.Contracts/RaceDay.Contracts.csproj RaceDay.Contracts/
RUN dotnet restore RaceDay.Api/RaceDay.Api.csproj

COPY RaceDay.Api/ RaceDay.Api/
COPY RaceDay.Contracts/ RaceDay.Contracts/
RUN dotnet publish RaceDay.Api/RaceDay.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "RaceDay.Api.dll"]

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore separately so dependency layers are reusable between source changes.
COPY RaceDay.Mvc/RaceDay.Mvc.csproj RaceDay.Mvc/
COPY RaceDay.Contracts/RaceDay.Contracts.csproj RaceDay.Contracts/
RUN dotnet restore RaceDay.Mvc/RaceDay.Mvc.csproj

COPY RaceDay.Mvc/ RaceDay.Mvc/
COPY RaceDay.Contracts/ RaceDay.Contracts/
RUN dotnet publish RaceDay.Mvc/RaceDay.Mvc.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0 \
    ApiSettings__BaseUrl=http://api:8080/
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "RaceDay.Mvc.dll"]

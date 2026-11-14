FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["RaceDay.MVC/RaceDay.MVC.csproj", "RaceDay.MVC/"]
RUN dotnet restore "RaceDay.MVC/RaceDay.MVC.csproj"

COPY . .
WORKDIR /src/RaceDay.MVC
RUN dotnet publish "RaceDay.MVC.csproj" \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build --chown=$APP_UID:$APP_UID /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ApiSettings__BaseUrl=http://host.docker.internal:5013/

EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["dotnet", "RaceDay.MVC.dll"]

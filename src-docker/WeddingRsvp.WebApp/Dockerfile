FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src-docker/WeddingRsvp.WebApp/WeddingRsvp.WebApp.csproj", "src-docker/WeddingRsvp.WebApp/"]
COPY ["src-dotnet/WeddingRsvp.Client/WeddingRsvp.Client.csproj", "src-dotnet/WeddingRsvp.Client/"]
COPY ["src-dotnet/WeddingRsvp.Abstractions/WeddingRsvp.Abstractions.csproj", "src-dotnet/WeddingRsvp.Abstractions/"]
COPY ["src-dotnet/WeddingRsvp.ServiceDefaults/WeddingRsvp.ServiceDefaults.csproj", "src-dotnet/WeddingRsvp.ServiceDefaults/"]
RUN dotnet restore "src-docker/WeddingRsvp.WebApp/WeddingRsvp.WebApp.csproj"
COPY . .
WORKDIR "/src/src-docker/WeddingRsvp.WebApp"
RUN dotnet build "./WeddingRsvp.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WeddingRsvp.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WeddingRsvp.WebApp.dll"]

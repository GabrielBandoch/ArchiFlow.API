FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/ArchiFlow.API/ArchiFlow.API.csproj", "src/ArchiFlow.API/"]
COPY ["src/ArchiFlow.Application/ArchiFlow.Application.csproj", "src/ArchiFlow.Application/"]
COPY ["src/ArchiFlow.Domain/ArchiFlow.Domain.csproj", "src/ArchiFlow.Domain/"]
COPY ["src/ArchiFlow.Infrastructure/ArchiFlow.Infrastructure.csproj", "src/ArchiFlow.Infrastructure/"]

RUN dotnet restore "src/ArchiFlow.API/ArchiFlow.API.csproj"

COPY . .
WORKDIR "/src/src/ArchiFlow.API"
RUN dotnet build "ArchiFlow.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ArchiFlow.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArchiFlow.API.dll"]

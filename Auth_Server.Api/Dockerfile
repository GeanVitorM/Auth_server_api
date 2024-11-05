FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copie o arquivo .csproj e restaure dependências
COPY ["Auth_Server.Api.csproj", "./"]
RUN dotnet restore "Auth_Server.Api.csproj"

# Copie o restante dos arquivos
COPY . .

# Defina o diretório de trabalho
WORKDIR "/src"
RUN dotnet build "Auth_Server.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Auth_Server.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth_Server.Api.dll"]

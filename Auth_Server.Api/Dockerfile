# Definir a imagem base para o runtime (onde o app será executado)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 9002

# Definir a imagem de construção com o SDK do .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar o arquivo .csproj e restaurar dependências
COPY ["Auth_Server.Api.csproj", "./"]
RUN dotnet restore "Auth_Server.Api.csproj"

# Copiar o restante dos arquivos do projeto
COPY . .

# Definir o diretório de trabalho correto e compilar o projeto
WORKDIR "/src"
RUN dotnet build "Auth_Server.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicar o projeto para a pasta /app/publish
FROM build AS publish
RUN dotnet publish "Auth_Server.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# A partir da imagem base, copiar os arquivos publicados e definir a entrada do contêiner
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth_Server.Api.dll"]

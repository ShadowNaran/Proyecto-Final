# syntax=docker/dockerfile:1

# ---------- Etapa 1: compilacion ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar solo el .csproj primero para aprovechar la cache de capas en dotnet restore
COPY ["CatalogoApi/CatalogoApi.csproj", "CatalogoApi/"]
RUN dotnet restore "CatalogoApi/CatalogoApi.csproj"

# Copiar el resto del codigo y publicar en Release
COPY CatalogoApi/ CatalogoApi/
WORKDIR /src/CatalogoApi
RUN dotnet publish "CatalogoApi.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---------- Etapa 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# La imagen aspnet de .NET 8+ trae el usuario sin privilegios "app".
# Se deja explicito para garantizar permisos de escritura sobre /app y las
# llaves persistentes de Data Protection en /app/keys.
USER root
COPY --from=build --chown=app:app /app/publish ./
RUN mkdir -p /app/keys && chown -R app:app /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DATA_PROTECTION_KEYS_PATH=/app/keys

EXPOSE 8080

USER app

ENTRYPOINT ["dotnet", "CatalogoApi.dll"]

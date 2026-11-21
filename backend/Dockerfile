# ------------------------------------------------------------------------------
# Etapa 1: Build y compilación con el SDK oficial de .NET 10
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copiar solución y proyectos primero: si no cambian, Docker reutiliza la capa del restore
# (cache de dependencias NuGet) y el build es mucho más rápido.
COPY Mockly.slnx ./
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/WebApi/WebApi.csproj src/WebApi/

# Restaurar paquetes
RUN dotnet restore Mockly.slnx

# Copiar el código fuente completo y compilar en modo Release
COPY src/ src/
WORKDIR /app/src/WebApi
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false --no-restore

# ------------------------------------------------------------------------------
# Etapa 2: Imagen final optimizada y ligera con ASP.NET Core Runtime
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Variables de entorno por defecto para producción
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080

# Usuario no root por seguridad
USER app

COPY --from=build /app/publish .

EXPOSE 8080

# El ensamblado toma el nombre del .csproj (WebApi.csproj → WebApi.dll), no del namespace.
ENTRYPOINT ["dotnet", "WebApi.dll"]

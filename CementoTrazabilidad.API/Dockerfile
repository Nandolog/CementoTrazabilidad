FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["CementoTrazabilidad.API/CementoTrazabilidad.API.csproj", "CementoTrazabilidad.API/"]
COPY ["CementoTrazabilidad.Core/CementoTrazabilidad.Core.csproj", "CementoTrazabilidad.Core/"]
COPY ["CementoTrazabilidad.Infrastructure/CementoTrazabilidad.Infrastructure.csproj", "CementoTrazabilidad.Infrastructure/"]
COPY ["CementoTrazabilidad.shared/CementoTrazabilidad.shared.csproj", "CementoTrazabilidad.shared/"]

# Restaurar dependencias
RUN dotnet restore "CementoTrazabilidad.API/CementoTrazabilidad.API.csproj"

# Copiar todo el código
COPY . .

# Compilar
WORKDIR "/src/CementoTrazabilidad.API"
RUN dotnet build "CementoTrazabilidad.API.csproj" -c Release -o /app/build

# Publicar
FROM build AS publish
RUN dotnet publish "CementoTrazabilidad.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CementoTrazabilidad.API.dll"]
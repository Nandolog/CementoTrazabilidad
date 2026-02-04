FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["CementoTrazabilidad.Blazor/CementoTrazabilidad.Blazor.csproj", "CementoTrazabilidad.Blazor/"]
COPY ["CementoTrazabilidad.shared/CementoTrazabilidad.shared.csproj", "CementoTrazabilidad.shared/"]

# Restaurar dependencias
RUN dotnet restore "CementoTrazabilidad.Blazor/CementoTrazabilidad.Blazor.csproj"

# Copiar todo el código
COPY . .

# Compilar y publicar
WORKDIR "/src/CementoTrazabilidad.Blazor"
RUN dotnet publish "CementoTrazabilidad.Blazor.csproj" -c Release -o /app/publish

# Nginx
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

COPY --from=build /app/publish/wwwroot .
COPY CementoTrazabilidad.Blazor/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80
<div align="center">

# 🏗️ CementoTrazabilidad

### Sistema Integral de Trazabilidad para Producción de Cemento

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?style=for-the-badge&logo=blazor)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?style=for-the-badge&logo=docker)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-Private-red?style=for-the-badge)](LICENSE)

**[🚀 Demo en Vivo](#) • [📖 Documentación](#) • [🐛 Reportar Bug](https://github.com/Nandolog/CementoTrazabilidad/issues)**

![Screenshot](https://via.placeholder.com/800x400/1a1a2e/ffffff?text=Dashboard+Preview)

</div>

---

## 🌟 **Características Principales**

<table>
<tr>
<td>

### 🎯 **Trazabilidad Completa**
- Seguimiento de lotes en tiempo real
- Historial de producción detallado
- Control de calidad integrado

</td>
<td>

### 👥 **Gestión de Personal**
- Control de turnos automatizado
- Asignación de roles y permisos
- Registro de actividades

</td>
</tr>
<tr>
<td>

### 📊 **Reportes Avanzados**
- Exportación a Excel
- Métricas de producción
- Dashboard interactivo

</td>
<td>

### 🔒 **Seguridad Robusta**
- Autenticación JWT
- Encriptación BCrypt
- Control de acceso por roles

</td>
</tr>
</table>

---

## 🛠️ **Stack Tecnológico**
graph LR A[Blazor WASM] -->|REST API| B[ASP.NET Core 9] B -->|Entity Framework| C[SQL Server] B -->|JWT Auth| D[BCrypt] A -->|Docker Compose| E[Nginx] B -->|Docker Compose| E C -->|Docker Compose| E

<div align="center">

| Frontend | Backend | Base de Datos | DevOps |
|:--------:|:-------:|:-------------:|:------:|
| ![Blazor](https://img.shields.io/badge/Blazor-WASM-512BD4?logo=blazor) | ![ASP.NET](https://img.shields.io/badge/ASP.NET-Core_9-512BD4?logo=dotnet) | ![SQL Server](https://img.shields.io/badge/SQL-Server-CC2927?logo=microsoftsqlserver) | ![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker) |
| ![HTML5](https://img.shields.io/badge/HTML5-E34F26?logo=html5&logoColor=white) | ![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white) | ![EF Core](https://img.shields.io/badge/EF-Core_9-512BD4?logo=dotnet) | ![GitHub Actions](https://img.shields.io/badge/GitHub-Actions-2088FF?logo=githubactions&logoColor=white) |

</div>

---

## 🚀 **Inicio Rápido**

### **Prerequisitos**

- [Docker Desktop](https://www.docker.com/products/docker-desktop) instalado
- Puertos `5165` y `5198` disponibles

### **Instalación en 3 Pasos**
1️⃣ Clonar el repositorio
git clone https://github.com/Nandolog/CementoTrazabilidad.git cd CementoTrazabilidad
2️⃣ Levantar los servicios
docker-compose up --build
3️⃣ ¡Listo! Accede a la aplicación

<div align="center">

### **🌐 URLs de Acceso**

| Servicio | URL | Descripción |
|----------|-----|-------------|
| 🎨 **Frontend** | http://localhost:5165 | Interfaz de usuario Blazor |
| 📡 **API** | http://localhost:5198/swagger | Documentación interactiva |
| 🗄️ **Base de Datos** | `localhost:1433` | SQL Server (interno) |

### **🔐 Credenciales Iniciales**
👤 Usuario: ADMIN001 🔑 Contraseña: admin123

</div>

---

## 📁 **Arquitectura del Proyecto**
CementoTrazabilidad/ │ ├── 🎨 CementoTrazabilidad.Blazor/      # Frontend (Blazor WebAssembly) │   ├── Pages/                          # Páginas de la aplicación │   ├── Services/                       # Servicios HTTP │   └── wwwroot/                        # Archivos estáticos │ ├── 📡 CementoTrazabilidad.API/         # Backend (ASP.NET Core Web API) │   ├── Controllers/                    # Endpoints REST │   ├── Services/                       # Lógica de negocio │   └── Program.cs                      # Configuración principal │ ├── 💼 CementoTrazabilidad.Core/        # Capa de dominio │   ├── Entidades/                      # Modelos de datos │   └── Interfaces/                     # Contratos de servicios │ ├── 🗄️ CementoTrazabilidad.Infrastructure/ # Capa de persistencia │   ├── Data/                           # Contexto EF Core │   ├── Repositories/                   # Acceso a datos │   └── Migrations/                     # Migraciones de BD │ ├── 🔗 CementoTrazabilidad.shared/      # DTOs compartidos │   └── Models/                         # Modelos de transferencia │ └── 🐳 docker-compose.yml               # Orquestación de contenedores

---

## 💻 **Desarrollo Local**

### **Sin Docker**

#### **1. Ejecutar la API**
cd CementoTrazabilidad.API dotnet restore dotnet run

#### **2. Ejecutar Blazor**
cd CementoTrazabilidad.Blazor dotnet restore dotnet run

### **Migraciones de Base de Datos**
Crear nueva migración
dotnet ef migrations add NombreMigracion --project CementoTrazabilidad.Infrastructure --startup-project CementoTrazabilidad.API
Aplicar migraciones
dotnet ef database update --project CementoTrazabilidad.API
Revertir última migración
dotnet ef migrations remove --project CementoTrazabilidad.Infrastructure

---

## 🎯 **Funcionalidades**

<details>
<summary><b>📦 Gestión de Producción</b></summary>

- ✅ Registro de lotes de producción
- ✅ Control de turnos y horarios
- ✅ Seguimiento de materiales
- ✅ Historial de cambios

</details>

<details>
<summary><b>👥 Administración de Personal</b></summary>

- ✅ Registro de operarios
- ✅ Asignación de turnos
- ✅ Control de roles y permisos
- ✅ Seguimiento de actividades

</details>

<details>
<summary><b>🚚 Control de Despachos</b></summary>

- ✅ Registro de envíos
- ✅ Trazabilidad de lotes despachados
- ✅ Validación de cantidades
- ✅ Reportes de entregas

</details>

<details>
<summary><b>📊 Reportes y Métricas</b></summary>

- ✅ Dashboard con KPIs
- ✅ Exportación a Excel
- ✅ Gráficos interactivos
- ✅ Métricas en tiempo real

</details>

---

## 🔒 **Seguridad**
sequenceDiagram participant U as Usuario participant B as Blazor participant A as API participant D as Base de Datos
U->>B: Login (usuario/contraseña)
B->>A: POST /api/auth/login
A->>D: Validar usuario
D->>A: Usuario encontrado
A->>A: Verificar BCrypt.Hash
A->>B: JWT Token
B->>B: Guardar en LocalStorage
B->>A: Request con Authorization: Bearer {token}
A->>A: Validar JWT
A->>B: Respuesta autorizada

### **Características de Seguridad**

- 🔐 **Autenticación JWT** con expiración configurable
- 🔒 **Contraseñas hasheadas** con BCrypt (salt aleatorio)
- 🛡️ **Autorización basada en roles** (Admin, Operario, etc.)
- 🔑 **CORS configurado** para entornos de desarrollo/producción
- 📝 **Logs de auditoría** de accesos y cambios

---

## ☁️ **Deployment**

### **Azure Container Apps** *(Recomendado)*
Login en Azure
az login
Crear grupo de recursos
az group create --name CementoTrazabilidad-RG --location brazilsouth
Deploy automático
az containerapp up 
--name cementotrazabilidad 
--resource-group CementoTrazabilidad-RG 
--location brazilsouth 
--source . 
--ingress external 
--target-port 8080

### **Variables de Entorno en Azure**
az containerapp update 
--name cementotrazabilidad 
--resource-group CementoTrazabilidad-RG 
--set-env-vars 
"ConnectionStrings__DefaultConnection=Server=..." 
"Jwt__Key=YourSecretKey" 
"Jwt__Issuer=CementoTrazabilidad.API"

---

## 🧪 **Testing**
Ejecutar tests unitarios
dotnet test
Con coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
Ver reporte de coverage
dotnet reportgenerator -reports:coverage.opencover.xml -targetdir:coverage

---

## 📸 **Screenshots**

<div align="center">

### **Dashboard Principal**
![Dashboard](https://via.placeholder.com/800x400/1a1a2e/0ea5e9?text=Dashboard+de+Metricas)

### **Gestión de Lotes**
![Lotes](https://via.placeholder.com/800x400/1a1a2e/10b981?text=Gestion+de+Lotes)

### **Reportes**
![Reportes](https://via.placeholder.com/800x400/1a1a2e/f59e0b?text=Reportes+Excel)

</div>

---

## 🤝 **Contribuir**

¡Las contribuciones son bienvenidas! Por favor:

1. 🍴 Fork el proyecto
2. 🌿 Crea tu rama (`git checkout -b feature/AmazingFeature`)
3. 💾 Commit tus cambios (`git commit -m 'Add: nueva característica'`)
4. 📤 Push a la rama (`git push origin feature/AmazingFeature`)
5. 🔀 Abre un Pull Request

### **Guías de Estilo**

- ✅ Seguir convenciones de C# (.NET)
- ✅ Comentar código complejo
- ✅ Agregar tests para nuevas funcionalidades
- ✅ Actualizar documentación

---

## 📝 **Roadmap**

- [x] Sistema de autenticación JWT
- [x] Gestión de lotes de producción
- [x] Reportes en Excel
- [ ] Notificaciones en tiempo real (SignalR)
- [ ] App móvil (MAUI)
- [ ] Integración con sensores IoT
- [ ] Machine Learning para predicciones

---

## 📄 **Licencia**

Este proyecto es de uso **privado** y **propietario**.

© 2024-2026 **Fernando Evers**. Todos los derechos reservados.

---

## 👨‍💻 **Autor**

<div align="center">

### **Fernando Evers**

[![GitHub](https://img.shields.io/badge/GitHub-Nandolog-181717?style=for-the-badge&logo=github)](https://github.com/Nandolog)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Fernando_Evers-0A66C2?style=for-the-badge&logo=linkedin)](https://linkedin.com/in/fernando-evers)
[![Email](https://img.shields.io/badge/Email-Contacto-EA4335?style=for-the-badge&logo=gmail)](mailto:tu@email.com)

**💼 Full Stack Developer | 🏗️ .NET Specialist | ☁️ Cloud Enthusiast**

</div>

---

## 🙏 **Agradecimientos**

- [Microsoft](https://dotnet.microsoft.com/) por .NET y Blazor
- [Docker](https://www.docker.com/) por la containerización
- Comunidad de GitHub por las herramientas open source

---

<div align="center">

### ⭐ **Si este proyecto te resulta útil, dale una estrella!**

[![Star History Chart](https://api.star-history.com/svg?repos=Nandolog/CementoTrazabilidad&type=Date)](https://star-history.com/#Nandolog/CementoTrazabilidad&Date)

**Made with ❤️ and ☕ by Fernando Evers**

</div>

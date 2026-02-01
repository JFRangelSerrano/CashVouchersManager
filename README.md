# Cash Vouchers Manager API

## 📋 Descripción General

**Cash Vouchers Manager** es una API RESTful robusta para la gestión integral de vales canjeables por dinero. El sistema permite la generación, consulta, canje y control de vales con códigos EAN13 únicos, implementando características avanzadas como control de concurrencia, limpieza automática de registros antiguos y autenticación segura.

El proyecto ha sido desarrollado siguiendo los principios de **Domain-Driven Design (DDD)** y las mejores prácticas de desarrollo de software, garantizando mantenibilidad, escalabilidad y testabilidad del código.

### Características Principales

- ✅ **Generación de vales** con códigos EAN13 únicos y validados
- ✅ **Sistema de estados** calculado en memoria (Active, Redeemed, Expired, InUse)
- ✅ **Control de concurrencia** mediante flags InUse para prevenir race conditions
- ✅ **Limpieza automática** de vales antiguos (más de 1 año)
- ✅ **Autenticación HTTP Basic** configurable
- ✅ **Persistencia con SQLite** y migraciones automáticas
- ✅ **62 tests** (unitarios e integración) con cobertura completa
- ✅ **Documentación interactiva** con Swagger/OpenAPI

---

## 🛠️ Stack Tecnológico

### Backend
- **.NET 8 SDK** - Framework principal
- **ASP.NET Core 8** - Web API Framework
- **C# 12** - Lenguaje de programación

### Base de Datos
- **SQLite** - Motor de base de datos embebido
- **Entity Framework Core 8.0** - ORM
- **EF Core Migrations** - Gestión de esquema de base de datos

### Testing
- **xUnit 2.9.2** - Framework de testing
- **Microsoft.AspNetCore.Mvc.Testing 8.0** - Testing de integración
- **SQLite In-Memory** - Base de datos para tests

### Documentación
- **Swashbuckle.AspNetCore 6.5.0** - Generación de documentación Swagger/OpenAPI

### Arquitectura
- **Domain-Driven Design (DDD)** - Arquitectura en capas
- **Repository Pattern** - Abstracción de acceso a datos
- **Dependency Injection** - Inyección de dependencias nativa de .NET
- **Background Services** - Tareas en segundo plano

---

## 📁 Estructura del Proyecto

El proyecto sigue una arquitectura **DDD en capas** con separación clara de responsabilidades:

```
CashVouchersManager/
├── CashVouchersManager.API/              # 🌐 Capa de Presentación (API REST)
│   ├── Controllers/                      # Controladores de la API
│   │   └── CashVoucherController.cs
│   ├── Middleware/                       # Middleware personalizado
│   │   └── BasicAuthenticationMiddleware.cs
│   ├── BackgroundServices/               # Servicios en segundo plano
│   │   └── VoucherCleanupService.cs
│   ├── Configuration/                    # Clases de configuración
│   │   └── AppSettings.cs
│   ├── appsettings.json                  # Configuración de la aplicación
│   └── Program.cs                        # Punto de entrada
│
├── CashVouchersManager.Application/      # 💼 Capa de Aplicación
│   ├── Services/                         # Servicios de aplicación
│   │   └── CashVoucherService.cs
│   └── Interfaces/                       # Contratos de servicios
│       └── ICashVoucherService.cs
│
├── CashVouchersManager.Domain/           # 🎯 Capa de Dominio (Lógica de Negocio)
│   ├── Entities/                         # Entidades de dominio
│   │   └── CashVoucher.cs
│   ├── Services/                         # Servicios de dominio
│   │   └── VoucherCodeGenerator.cs
│   └── Interfaces/                       # Contratos de repositorios
│       └── ICashVoucherRepository.cs
│
├── CashVouchersManager.Infrastructure/   # 🗄️ Capa de Infraestructura (Datos)
│   ├── Data/                             # Contexto de base de datos
│   │   └── CashVouchersDbContext.cs
│   ├── Repositories/                     # Implementaciones de repositorios
│   │   └── CashVoucherRepository.cs
│   └── Migrations/                       # Migraciones de EF Core
│
├── CashVouchersManager.DTO/              # 📦 DTOs y Enumeraciones
│   ├── DTOs/                             # Objetos de transferencia
│   │   ├── CashVoucherDTO.cs
│   │   ├── GenerateCashVoucherRequestDTO.cs
│   │   ├── RedeemCashVoucherRequestDTO.cs
│   │   └── SetInUseRequestDTO.cs
│   └── Enums/                            # Enumeraciones
│       ├── CashVoucherStatusEnum.cs
│       └── CashVoucherDateTypeEnum.cs
│
├── CashVouchersManager.Tests/            # 🧪 Tests Unitarios e Integración
│   ├── API/                              # Tests de API (integración)
│   ├── Application/                      # Tests de servicios
│   ├── Domain/                           # Tests de dominio
│   └── Infrastructure/                   # Tests de repositorios
│
├── Context/                              # 📄 Documentación del proyecto
│   ├── 1.- Core.md
│   ├── 2.- InUse Property.md
│   ├── 3.- Background Service.md
│   └── 4.- Security.md
│
├── CashVouchersManager.sln               # Solución de Visual Studio
├── README.md                             # Este archivo
├── TESTING_GUIDE.md                      # Guía de testing
└── AGENTS.md                             # Reglas de desarrollo

```

### Descripción de Capas

#### 🌐 API Layer (CashVouchersManager.API)
Contiene los controladores REST, middleware de autenticación, servicios de fondo y configuración de la aplicación. Es el punto de entrada para las peticiones HTTP.

#### 💼 Application Layer (CashVouchersManager.Application)
Implementa los casos de uso de la aplicación y orquesta las operaciones entre la capa de dominio e infraestructura.

#### 🎯 Domain Layer (CashVouchersManager.Domain)
Contiene la lógica de negocio pura, entidades del dominio, servicios de dominio y las interfaces que definen los contratos.

#### 🗄️ Infrastructure Layer (CashVouchersManager.Infrastructure)
Implementa la persistencia de datos, repositorios y acceso a la base de datos mediante Entity Framework Core.

#### 📦 DTO Layer (CashVouchersManager.DTO)
Define los objetos de transferencia de datos y enumeraciones compartidas entre capas.

#### 🧪 Tests Layer (CashVouchersManager.Tests)
Contiene 62 tests que validan todas las funcionalidades: generación de códigos, estados, operaciones de repositorio, servicios, limpieza automática y autenticación.

---

## 🚀 Instalación y Ejecución

### Requisitos Previos

- **.NET 8 SDK** - [Descargar aquí](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Editor de código** (opcional):
  - Visual Studio 2022
  - Visual Studio Code
  - JetBrains Rider

### Instalación

1. **Clonar o descargar el proyecto**
   ```bash
   cd CashVouchersManager
   ```

2. **Restaurar dependencias**
   ```bash
   dotnet restore
   ```

3. **Compilar la solución**
   ```bash
   dotnet build
   ```

### Ejecución

#### Ejecutar la API

```bash
cd CashVouchersManager.API
dotnet run
```

La aplicación estará disponible en:
- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger

#### Ejecutar Tests

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar con información detallada
dotnet test --verbosity normal

# Ejecutar con cobertura (si está configurado)
dotnet test --collect:"XPlat Code Coverage"
```

### Configuración

La aplicación se configura a través del archivo `appsettings.json`:

```json
{
  "AppSettings": {
    "Port": 5000,                           // Puerto de escucha
    "UseHttpsRedirection": false,           // Activar/desactivar HTTPS
    "Authentication": {
      "Username": "admin",                  // Usuario para autenticación
      "Password": "admin123"                // Contraseña para autenticación
    }
  }
}
```

---

## 🎯 Funcionalidades Principales

### 1. Generación de Vales

**Endpoint**: `POST /api/GenerateCashVoucher`

Crea un nuevo vale con código EAN13 único y validado.

**Características**:
- Generación automática de código EAN13 con dígito de control
- Validación de unicidad del código entre vales activos
- Fecha de expiración configurable
- Vinculación con venta emisora

**Ejemplo**:
```bash
POST /api/GenerateCashVoucher
Authorization: Basic YWRtaW46YWRtaW4xMjM=
Content-Type: application/json

{
  "amount": 50.00,
  "issuingStoreId": 1234,
  "expirationDate": "2026-12-31T23:59:59Z",
  "issuingSaleId": "SALE-123"
}
```

### 2. Consulta de Vales

**Endpoints**:
- `GET /api/GetCashVoucherByCode/{code}` - Obtener por código
- `GET /api/GetFilteredCashVouchers` - Búsqueda avanzada con filtros

**Filtros disponibles**:
- Estado (Active, Redeemed, Expired, InUse)
- ID de establecimiento emisor
- Rango de fechas (creación, canje, expiración)
- Vales activos/inactivos

**Ejemplo**:
```bash
GET /api/GetCashVoucherByCode/1234567890123?onlyActives=true
Authorization: Basic YWRtaW46YWRtaW4xMjM=
```

### 3. Canje de Vales

**Endpoint**: `PUT /api/RedeemCashVoucher/{code}`

Canjea todos los vales activos asociados a un código.

**Características**:
- Valida que los vales no estén ya canjeados
- Valida que no estén expirados
- Registra fecha y venta de canje
- Establece automáticamente InUse=false

**Ejemplo**:
```bash
PUT /api/RedeemCashVoucher/1234567890123
Authorization: Basic YWRtaW46YWRtaW4xMjM=
Content-Type: application/json

{
  "redemptionDate": "2026-02-01T10:30:00Z",
  "redemptionSaleId": "REDEMPTION-456"
}
```

### 4. Control de Concurrencia

**Endpoint**: `POST /api/SetCashVouchersInUse/{code}`

Marca o desmarca vales como "en uso" para prevenir condiciones de carrera.

**Flujo recomendado**:
1. Marcar vales como InUse=true antes de validaciones
2. Realizar operaciones de negocio
3. Canjear (automáticamente InUse=false) o liberar manualmente

**Comportamiento**:
- **InUse=true**: Solo actualiza vales activos (no canjeados ni expirados)
- **InUse=false**: Actualiza todos los vales con el código

**Ejemplo**:
```bash
POST /api/SetCashVouchersInUse/1234567890123
Authorization: Basic YWRtaW46YWRtaW4xMjM=
Content-Type: application/json

{
  "inUse": true
}
```

### 5. Sistema de Estados

Los vales tienen estados **calculados dinámicamente** con precedencia definida:

1. **Redeemed** (Canjeado) - Máxima precedencia
2. **Expired** (Expirado) - Ha superado su fecha de expiración
3. **InUse** (En Uso) - Marcado para control de concurrencia
4. **Active** (Activo) - Estado por defecto

**Ejemplo**: Un vale canjeado siempre mostrará estado `Redeemed`, aunque también esté expirado o marcado como InUse.

### 6. Limpieza Automática

Un **Background Service** ejecuta diariamente la limpieza de vales antiguos:

**Criterios de eliminación**:
- Vales canjeados con más de **1 año** desde su canje
- Vales expirados con más de **1 año** desde su expiración

**Características**:
- Ejecución automática cada 24 horas
- No interfiere con operaciones de la API
- Registro completo en logs
- Operación transaccional

### 7. Reutilización de Códigos

Los códigos EAN13 pueden reutilizarse bajo ciertas condiciones:

**Un código está disponible** si todos los vales con ese código están:
- Canjeados (sin importar cuándo), O
- Expirados hace más de 30 días

Esto optimiza el espacio de códigos disponibles sin comprometer la integridad.

---

## 🔒 Seguridad y Autenticación

### Autenticación HTTP Basic

Toda la API (excepto Swagger) está protegida con autenticación básica HTTP.

**Credenciales por defecto**:
- Usuario: `admin`
- Contraseña: `admin123`

**Uso**:
```bash
# Header de autenticación
Authorization: Basic YWRtaW46YWRtaW4xMjM=

# Generar en PowerShell
$credentials = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("admin:admin123"))
```

**Configuración**:
Las credenciales se configuran en `appsettings.json`:
```json
{
  "AppSettings": {
    "Authentication": {
      "Username": "tu-usuario",
      "Password": "tu-contraseña-segura"
    }
  }
}
```

### Endpoints Públicos

Solo los endpoints de **Swagger** (`/swagger/*`) son accesibles sin autenticación para facilitar la exploración de la API.

---

## 📊 Base de Datos

### Motor y Configuración

- **Motor**: SQLite
- **Archivo**: `CashVouchers.db` (se crea automáticamente)
- **Ubicación**: Directorio de ejecución de la aplicación
- **Migraciones**: Aplicadas automáticamente al iniciar

### Esquema

La tabla principal `CashVouchers` **no tiene clave primaria** por diseño, permitiendo múltiples vales con el mismo código:

```sql
CREATE TABLE CashVouchers (
    Code TEXT NOT NULL,
    Amount REAL NOT NULL,
    CreationDate TEXT NOT NULL,
    IssuingStoreId INTEGER NOT NULL,
    RedemptionDate TEXT NULL,
    ExpirationDate TEXT NULL,
    IssuingSaleId TEXT NULL,
    RedemptionSaleId TEXT NULL,
    InUse INTEGER NOT NULL DEFAULT 0
);
```

### Gestión de Migraciones

```bash
# Crear una nueva migración
dotnet ef migrations add NombreMigracion --project CashVouchersManager.Infrastructure --startup-project CashVouchersManager.API

# Aplicar migraciones manualmente (normalmente automático)
dotnet ef database update --project CashVouchersManager.Infrastructure --startup-project CashVouchersManager.API
```

---

## 🧪 Testing

El proyecto incluye **62 tests** que cubren:

### Tests Unitarios
- ✅ Generación y validación de códigos EAN13
- ✅ Cálculo de estados con precedencia
- ✅ Operaciones de repositorio (CRUD, filtros)
- ✅ Lógica de servicios de aplicación
- ✅ Control de concurrencia InUse
- ✅ Limpieza automática de vales antiguos

### Tests de Integración
- ✅ Autenticación HTTP Basic
- ✅ Endpoints de la API
- ✅ Flujos completos end-to-end

### Ejecutar Tests

```bash
# Todos los tests
dotnet test

# Tests con salida detallada
dotnet test --verbosity normal

# Tests de un proyecto específico
dotnet test CashVouchersManager.Tests/CashVouchersManager.Tests.csproj
```

Para más información, consulta [TESTING_GUIDE.md](TESTING_GUIDE.md).

---

## 📚 Documentación Adicional

- **[TESTING_GUIDE.md](TESTING_GUIDE.md)** - Guía completa de testing con ejemplos de PowerShell
- **[AGENTS.md](AGENTS.md)** - Convenciones de código y reglas de desarrollo
- **Context/** - Documentación técnica de características específicas:
  - `1.- Core.md` - Funcionalidad base del sistema
  - `2.- InUse Property.md` - Control de concurrencia
  - `3.- Background Service.md` - Servicio de limpieza
  - `4.- Security.md` - Autenticación y configuración

---

## 🔧 Características Técnicas Destacadas

### Entidad sin Clave Primaria
La entidad `CashVoucher` no tiene clave primaria, lo que requiere operaciones especiales:
- Uso de `ExecuteSqlRaw` para operaciones de escritura
- Transacciones explícitas para garantizar consistencia
- Configuración especial en EF Core con `HasNoKey()`

### Códigos EAN13
- Generación automática basada en ID de establecimiento
- Cálculo de dígito de control según estándar EAN13
- Validación de unicidad en vales activos

### Operaciones Transaccionales
Las operaciones críticas como `SetInUse` se ejecutan dentro de transacciones para garantizar consistencia de datos incluso con múltiples vales.

### Fechas en UTC
Todas las fechas se manejan y almacenan en **UTC** para evitar problemas de zonas horarias.

---

## 👥 Convenciones de Código

El proyecto sigue convenciones estrictas definidas en [AGENTS.md](AGENTS.md):

- **Nomenclatura**:
  - Variables locales y parámetros: `camelCase`
  - Clases, métodos, interfaces: `PascalCase`
  
- **Idioma**:
  - Código fuente: Inglés
  - Comentarios de documentación: Inglés
  - Documentación de usuario: Español

- **Comentarios**:
  - Todas las clases y métodos públicos incluyen documentación XML
  - Descripción de funcionalidad, parámetros y valores de retorno

---

## 📖 Swagger / OpenAPI

La API incluye documentación interactiva generada automáticamente:

**Acceso**: http://localhost:5000/swagger

Desde Swagger puedes:
- 📄 Ver todos los endpoints disponibles
- 🔍 Consultar esquemas de request/response
- 🧪 Probar la API directamente (con autenticación)
- 📥 Descargar la especificación OpenAPI

---

## 🏗️ Arquitectura y Patrones

### Domain-Driven Design (DDD)
- Separación clara de capas (Domain, Application, Infrastructure, API)
- Entidades de dominio con lógica de negocio
- Repositorios para abstracción de datos
- Servicios de aplicación para casos de uso

### Repository Pattern
Abstrae el acceso a datos mediante interfaces:
```csharp
public interface ICashVoucherRepository
{
    Task<List<CashVoucher>> GetByCodeAsync(string code, bool onlyActives);
    Task AddAsync(CashVoucher cashVoucher);
    Task<int> DeleteOldVouchersAsync();
    // ...
}
```

### Dependency Injection
Inyección de dependencias nativa de .NET Core para:
- Servicios de aplicación
- Repositorios
- Servicios de dominio
- Configuración

### Background Services
Servicio de limpieza como `IHostedService` que se ejecuta en segundo plano.

---

## 📝 Licencia

Este proyecto es un ejercicio académico/profesional y no incluye licencia específica.

---

## ✨ Contacto y Soporte

Para consultas, problemas o sugerencias:
- 📧 Revisar la documentación en `/Context`
- 📖 Consultar [TESTING_GUIDE.md](TESTING_GUIDE.md)
- 🐛 Reportar issues o consultas según el proceso establecido

---

**Última actualización**: Febrero 2026  
**Versión**: 1.0.0  
**Framework**: .NET 8

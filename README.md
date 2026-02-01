# Cash Vouchers Manager API

API RESTful para la gestión de vales canjeables por dinero, desarrollada con ASP.NET Core 8 y siguiendo los principios de Domain-Driven Design (DDD).

## Arquitectura

El proyecto sigue una arquitectura DDD limpia con los siguientes proyectos:

- **CashVouchersManager.DTO**: DTOs y enumeraciones compartidas
- **CashVouchersManager.Domain**: Entidades de dominio, interfaces y servicios
- **CashVouchersManager.Infrastructure**: Implementación de persistencia con Entity Framework Core
- **CashVouchersManager.Application**: Servicios de aplicación y lógica de negocio
- **CashVouchersManager.API**: API Web con controladores ASP.NET Core
- **CashVouchersManager.Tests**: Tests unitarios con xUnit

## Requisitos

- .NET 8 SDK
- Visual Studio 2022, VS Code, o Rider

## Ejecutar la aplicación

1. Restaurar paquetes NuGet:
```bash
dotnet restore
```

2. Compilar la solución:
```bash
dotnet build
```

3. Ejecutar la API:
```bash
cd CashVouchersManager.API
dotnet run
```

La API estará disponible en:
- http://localhost:5000 (por defecto, configurable en appsettings.json)

**Swagger UI** estará disponible en: http://localhost:5000/swagger

## Autenticación

Toda la API está protegida con **Autenticación Básica HTTP (Basic Authentication)**.

### Credenciales por defecto

- **Usuario**: `admin`
- **Contraseña**: `admin123`

Las credenciales se pueden configurar en el archivo `appsettings.json`:

```json
{
  "AppSettings": {
    "Authentication": {
      "Username": "admin",
      "Password": "admin123"
    }
  }
}
```

### Uso de autenticación

Para acceder a cualquier endpoint (excepto Swagger), debe incluir el header de autenticación:

```
Authorization: Basic YWRtaW46YWRtaW4xMjM=
```

Donde el valor es la codificación Base64 de `username:password`.

## Configuración

La aplicación permite configurar los siguientes aspectos desde `appsettings.json`:

### Puerto de escucha

```json
{
  "AppSettings": {
    "Port": 5000
  }
}
```

### Redirección HTTPS

```json
{
  "AppSettings": {
    "UseHttpsRedirection": false
  }
}
```

**Nota**: Por defecto está deshabilitado. Activar solo si se usa con certificados SSL/TLS.

## Tests Unitarios

El proyecto incluye 62 tests (unitarios e integración) que validan todas las reglas de negocio, incluyendo la funcionalidad de control de concurrencia con la propiedad InUse, el servicio de limpieza automática y la autenticación básica.

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar con más detalle
dotnet test --verbosity normal
```

Para más información sobre los tests, consulta [TESTING_GUIDE.md](TESTING_GUIDE.md).

## Base de datos

- **Motor**: SQLite
- **Archivo**: Se crea automáticamente en el directorio de ejecución como `CashVouchers.db`
- **Migraciones**: Se aplican automáticamente al arrancar la aplicación

## Endpoints

### POST /api/GenerateCashVoucher
Crea un nuevo vale con código EAN13 único.

**Request Body:**
```json
{
  "amount": 50.00,
  "issuingStoreId": 1234,
  "expirationDate": "2026-12-31T23:59:59Z",
  "issuingSaleId": "SALE-123"
}
```

### GET /api/GetCashVoucherByCode/{code}
Obtiene todos los vales con el código especificado.

**Query Parameters:**
- `onlyActives` (bool, default: true): Si es true, devuelve solo vales activos

### GET /api/GetFilteredCashVouchers
Obtiene vales filtrados por múltiples criterios.

**Query Parameters:**
- `status` (CashVoucherStatusEnum): Active, Redeemed, o Expired
- `issuingStoreId` (int): ID del establecimiento emisor
- `dateFrom` (DateTime): Fecha inicial
- `dateTo` (DateTime): Fecha final
- `dateType` (CashVoucherDateTypeEnum, default: Creation): Creation, Redemption, o Expiration

### PUT /api/RedeemCashVoucher/{code}
Canjea todos los vales activos con el código especificado. Al canjear un vale, se establece automáticamente InUse=false.

**Request Body:**
```json
{
  "redemptionDate": "2026-02-01T10:30:00Z",
  "redemptionSaleId": "REDEMPTION-456"
}
```

### POST /api/SetCashVouchersInUse/{code}
Establece o quita la marca InUse en todos los vales con el código especificado. Se utiliza para control de concurrencia al reservar vales durante un proceso de canje.

**Request Body:**
```json
{, InUse) se calcula en memoria con precedencia definida
- **Control de concurrencia**: La propiedad InUse permite reservar vales durante procesos de canje, evitando condiciones de carrera
- **Operaciones transaccionales**: Las operaciones críticas como SetInUse se ejecutan dentro de transacciones para garantizar consistenc
  "inUse": true
}
```

**Restricciones:**
- No se puede establecer InUse=true en vales canjeados o expirados
- Se puede establecer InUse=false en cualquier vale

## Características técnicas

- **Entidad sin clave primaria**: La entidad CashVoucher no tiene clave primaria, lo que requiere operaciones especiales con EF Core
- **Código EAN13**: Los códigos se generan automáticamente siguiendo el estándar EAN13
- **Unicidad de códigos**: Se garantiza que no haya colisiones con vales activos
- **Estados calculados**: El estado del vale (Active, Redeemed, Expired, InUse) se calcula en memoria con precedencia definida
- **Control de concurrencia**: La propiedad InUse permite reservar vales durante procesos de canje, evitando condiciones de carrera
- **Operaciones transaccionales**: Las operaciones críticas como SetInUse se ejecutan dentro de transacciones para garantizar consistencia
- **Autenticación básica HTTP**: Toda la API está protegida con autenticación básica configurable
- **Configuración flexible**: Puerto, HTTPS y credenciales configurables desde appsettings.json
- **Limpieza automática**: Un servicio en segundo plano elimina diariamente vales antiguos (más de 1 año)
- **Fechas UTC**: Todas las fechas se manejan en UTC

## Servicio de Limpieza Automática

La aplicación incluye un servicio en segundo plano que se ejecuta diariamente y realiza limpieza automática de vales antiguos.

### Criterios de eliminación

El servicio elimina permanentemente vales que cumplan **alguna** de estas condiciones:
- Vales **canjeados** con `RedemptionDate` anterior a **1 año** desde la fecha actual
- Vales **expirados** con `ExpirationDate` anterior a **1 año** desde la fecha actual

### Características del servicio

- **Ejecución automática**: Se inicia al arrancar la aplicación y se ejecuta cada 24 horas
- **Operación en segundo plano**: No interfiere con las operaciones normales de la API
- **Registro de actividad**: Todas las ejecuciones y eliminaciones se registran en los logs
- **Fechas en UTC**: Todas las comparaciones de fechas se realizan en UTC

## Reglas de negocio

### Disponibilidad de códigos para generación

Un vale es considerado **inactivo** (su código está disponible para ser reutilizado) si:
- Está canjeado (independientemente del tiempo que lleve canjeado), O
- Está caducado desde hace más de 30 días

Al generar un nuevo vale, el sistema garantiza que el código EAN13 generado no exista en ningún vale activo.

### Estados de los vales

Un vale puede tener uno de estos estados calculados, con la siguiente precedencia:
1. **Redeemed**: Ha sido canjeado (máxima precedencia)
2. **Expired**: Ha pasado su fecha de expiración
3. **InUse**: Está marcado como en uso para control de concurrencia
4. **Active**: No está canjeado, no ha expirado, y no está en uso

La precedencia significa que si un vale está canjeado, su estado será Redeemed independientemente de si está expirado o marcado como InUse.

### Control de concurrencia con InUse

La propiedad InUse permite implementar un mecanismo de bloqueo optimista para evitar condiciones de carrera durante el proceso de canje:

1. Antes de iniciar un proceso de canje, establecer InUse=true en los vales
2. Realizar las validaciones y operaciones necesarias
3. Al completar el canje exitosamente, InUse se establece automáticamente en false
4. Si el canje falla, establecer manualmente InUse=false para liberar los vales

**Restricciones:**
- No se puede establecer InUse=true en vales canjeados o expiradosplica a TODOS los vales con el mismo código (o lleva expirado menos de 30 días)
- **Redeemed**: Ha sido canjeado
- **Expired**: Ha pasado su fecha de expiración

## Convenciones de código

- Variables locales y parámetros: camelCase
- Clases, métodos, interfaces, enumeraciones: PascalCase
- Comentarios de documentación: Inglés
- Código: Inglés

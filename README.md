# Cash Vouchers Manager API

API RESTful para la gestión de vales canjeables por dinero, desarrollada con ASP.NET Core 8 y siguiendo los principios de Domain-Driven Design (DDD).

## Arquitectura

El proyecto sigue una arquitectura DDD limpia con los siguientes proyectos:

- **CashVouchersManager.DTO**: DTOs y enumeraciones compartidas
- **CashVouchersManager.Domain**: Entidades de dominio, interfaces y servicios
- **CashVouchersManager.Infrastructure**: Implementación de persistencia con Entity Framework Core
- **CashVouchersManager.Application**: Servicios de aplicación y lógica de negocio
- **CashVouchersManager.API**: API Web con controladores ASP.NET Core

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
- http://localhost:5000

**Swagger UI** estará disponible en: http://localhost:5000/swagger

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
Canjea todos los vales activos con el código especificado.

**Request Body:**
```json
{
  "redemptionDate": "2026-02-01T10:30:00Z",
  "redemptionSaleId": "REDEMPTION-456"
}
```

## Características técnicas

- **Entidad sin clave primaria**: La entidad CashVoucher no tiene clave primaria, lo que requiere operaciones especiales con EF Core
- **Código EAN13**: Los códigos se generan automáticamente siguiendo el estándar EAN13
- **Unicidad de códigos**: Se garantiza que no haya colisiones con vales activos
- **Estados calculados**: El estado del vale (Active, Redeemed, Expired) se calcula en memoria
- **Fechas UTC**: Todas las fechas se manejan en UTC

## Reglas de negocio

Un vale es considerado **inactivo** si:
- Está canjeado, O
- Está caducado desde hace más de 30 días

Un vale activo puede tener uno de estos estados:
- **Active**: No está canjeado ni caducado
- **Redeemed**: Ha sido canjeado
- **Expired**: Ha pasado su fecha de expiración

## Convenciones de código

- Variables locales y parámetros: camelCase
- Clases, métodos, interfaces, enumeraciones: PascalCase
- Comentarios de documentación: Inglés
- Código: Inglés

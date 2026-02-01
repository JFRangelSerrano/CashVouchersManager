# Guía de Pruebas - Cash Vouchers Manager API

Esta guía proporciona ejemplos de cómo probar la API y ejecutar los tests unitarios.

## Tests Unitarios

El proyecto incluye un conjunto completo de tests unitarios usando xUnit que validan todas las reglas de negocio.

### Ejecutar todos los tests

```powershell
dotnet test
```

### Ejecutar tests con más detalle

```powershell
dotnet test --verbosity normal
```

### Cobertura de tests

Los tests cubren:

1. **VoucherCodeGeneratorTests**: Generación de códigos EAN13
   - Formato correcto (13 dígitos)
   - Código comienza con ID de establecimiento
   - Dígito de control válido
   - Unicidad de códigos generados

2. **CashVoucherStatusTests**: Estados calculados de vales
   - Estado Active cuando no está canjeado ni expirado
   - Estado Redeemed cuando está canjeado
   - Estado Expired cuando ha expirado
   - Precedencia de Redeemed sobre Expired
   - **Precedencia de InUse en estados**
   - **InUse tiene precedencia sobre Active**
   - **Redeemed tiene precedencia sobre InUse**

3. **CashVoucherRepositoryTests**: Operaciones de persistencia
   - Inserción y consulta de vales
   - Filtrado por código con/sin vales activos
   - Filtrado por estado, establecimiento y fechas
   - Actualización de vales
   - Validación de disponibilidad de códigos (regla de 30 días)

4. **CashVoucherInUseTests**: Operaciones con la propiedad InUse
   - **Establecer InUse en vales activos**
   - **Quitar InUse de vales**
   - **Filtrado automático al establecer InUse=true**
   - **Actualización sin filtrado al establecer InUse=false**
   - **Operaciones transaccionales con múltiples vales**
   - **GetAllByCodeAsync retorna todos los vales**

5. **VoucherCleanupTests**: Limpieza automática de vales antiguos
   - **Eliminación de vales canjeados con más de 1 año**
   - **Eliminación de vales expirados con más de 1 año**
   - **Eliminación combinada de vales antiguos**
   - **Preservación de vales recientes**
   - **Validación del límite exacto de 1 año**

6. **CashVoucherServiceTests**: Lógica de aplicación
   - Generación de vales con códigos únicos
   - Consulta de vales por código
   - Filtrado dinámico de vales
   - Canje de vales activos
   - Manejo correcto de fechas UTC
   - **SetCashVouchersInUseAsync establece y quita la marca InUse**
   - **RedeemCashVoucherAsync establece InUse=false automáticamente**
   - **Filtrado por estado al establecer InUse**

## Pruebas de API

## Requisitos previos

1. La API debe estar ejecutándose en http://localhost:5000
2. Ejecutar desde PowerShell o PowerShell Core

## 1. Generar un nuevo vale

Crea un vale nuevo con código EAN13 único.

```powershell
$body = @{
    amount = 50.00
    issuingStoreId = 1234
    expirationDate = '2026-12-31T23:59:59Z'
    issuingSaleId = 'SALE-123'
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri 'http://localhost:5000/api/GenerateCashVoucher' `
    -Method POST `
    -Body $body `
    -ContentType 'application/json' `
    -UseBasicParsing

$response.Content | ConvertFrom-Json | Format-List
```

**Respuesta esperada:**
```
Code            : 1234XXXXXXXX (13 dígitos EAN13)
Amount          : 50.00
CreationDate    : 2026-02-01T...
IssuingStoreId  : 1234
RedemptionDate  : 
ExpirationDate  : 2026-12-31T23:59:59Z
IssuingSaleId   : SALE-123
RedemptionSaleId: 
Status          : Active
```

## 2. Obtener vale por código

Obtiene todos los vales con un código específico. Guarda el código del vale generado en el paso anterior.

```powershell
# Reemplaza XXXXXXXXXXXXX con el código del vale generado
$code = '1234XXXXXXXXX'

$response = Invoke-WebRequest -Uri "http://localhost:5000/api/GetCashVoucherByCode/$code" `
    -Method GET `
    -UseBasicParsing

$response.Content | ConvertFrom-Json | Format-List
```

### Solo vales activos (por defecto)

```powershell
$response = Invoke-WebRequest -Uri "http://localhost:5000/api/GetCashVoucherByCode/$code?onlyActives=true" `
    -Method GET `
    -UseBasicParsing

$response.Content | ConvertFrom-Json | Format-List
```

### Todos los vales (activos e inactivos)

```powershell
$response = Invoke-WebRequest -Uri "http://localhost:5000/api/GetCashVoucherByCode/$code?onlyActives=false" `
    -Method GET `
    -UseBasicParsing

$response.Content | ConvertFrom-Json | Format-List
```

## 3. Obtener vales filtrados

### Filtrar por estado (Active)

```powershell
$response = Invoke-WebRequest -Uri 'http://localhost:5000/api/GetFilteredCashVouchers?status=0' `
    -Method GET `
    -UseBasicParsing

$response.Content | ConvertFrom-Json | Format-List
```

### Filtrar por establecimiento emisor

```powershell
$response = Invoke-WebRequest -Uri 'http://localhost:5000/api/GetFilteredCashVouchers?issuingStoreId=1234' `
    -Method GET `
    -UseBasicParsing

$response.Content | ConvertFrom-Json | Format-List
```

### Filtrar por rango de fechas de creación

```powershell
$dateFrom = '2026-01-01T00:00:00Z'
$dateTo = '2026-12-31T23:59:59Z'

$response = Invoke-WebRequest -Uri "http://localhost:5000/api/GetFilteredCashVouchers?dateFrom=$dateFrom&dateTo=$dateTo&dateType=0" `
    -Method GET `
    -UseBasicParsing

$response.Content | ConvertFrom-Json | Format-List
```

### Filtros combinados

```powershell
$response = Invoke-WebRequest -Uri 'http://localhost:5000/api/GetFilteredCashVouchers?status=0&issuingStoreId=1234' `
    -Method GET `
    -UseBasicParsing

$response.Content | ConvertFrom-Json | Format-List
```

## 4. Establecer/Quitar marca InUse

Marca vales como "en uso" para control de concurrencia durante procesos de canje.

### Establecer InUse=true

```powershell
# Reemplaza XXXXXXXXXXXXX con el código del vale
$code = '1234XXXXXXXXX'

$body = @{
    inUse = $true
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri "http://localhost:5000/api/SetCashVouchersInUse/$code" `
    -Method POST `
    -Body $body `
    -ContentType 'application/json' `
    -UseBasicParsing

$response.Content | ConvertFrom-Json | Format-List
```

**Respuesta esperada:**
```
Code            : 1234XXXXXXXX
Amount          : 50.00
...
InUse           : True
Status          : InUse
```

### Quitar InUse (establecer en false)

```powershell
$body = @{
    inUse = $false
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri "http://localhost:5000/api/SetCashVouchersInUse/$code" `
    -Method POST `
    -Body $body `
    -ContentType 'application/json' `
    -UseBasicPa
- `3` = InUse

**Precedencia de estados:**
1. Redeemed (máxima precedencia)
2. Expired
3. InUse
4. Activersing

$response.Content | ConvertFrom-Json | Format-List
```

**Nota:** No se puede establecer InUse=true en vales canjeados o expirados. Intentarlo resultará en un error HTTP 400.

## 5. Canjear vale

Canjea todos los vales activos con un código específico. El canje automáticamente establece InUse=false.

```powershell
# Reemplaza XXXXXXXXXXXXX con el código del vale a canjear
$code = '1234XXXXXXXXX'

$body = @{
    redemptionDate = '2026-02-01T15:30:00Z'
    redemptionSaleId = 'REDEMPTION-456'
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri "http://localhost:5000/api/RedeemCashVoucher/$code" `
    -Method PUT `
    -Body $body `
    -ContentType 'application/json' `
    -UseBasicParsing

$response.Content | ConvertFrom-Json | Format-List
```

### Canjear sin especificar fecha (usa fecha actual UTC)

```powershell
$body = @{
    redemptionSaleId = 'REDEMPTION-789'
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri "http://localhost:5000/api/RedeemCashVoucher/$code" `
    -Method PUT `
    -Body $body `
    -ContentType 'application/json' `
    -UseBasicParsing

$response.Content | ConvertFrom-Json | Format-List
```

## Valores de enumeraciones

### CashVoucherStatusEnum
- `0` = Active
- `1` = Redeemed
- `2` = Expired

### CashVoucherDateTypeEnum
- `0` = Creation con control de concurrencia InUse
Write-Host "=== 1. Generando vale ===" -ForegroundColor Green
$body = @{
    amount = 50.00
    issuingStoreId = 1234
    expirationDate = '2026-12-31T23:59:59Z'
    issuingSaleId = 'SALE-123'
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri 'http://localhost:5000/api/GenerateCashVoucher' `
    -Method POST -Body $body -ContentType 'application/json' -UseBasicParsing
$voucher = $response.Content | ConvertFrom-Json
$code = $voucher.Code
Write-Host "Vale generado con código: $code" -ForegroundColor Yellow
$voucher | Format-List

Write-Host "`n=== 2. Consultando vale por código ===" -ForegroundColor Green
$response = Invoke-WebRequest -Uri "http://localhost:5000/api/GetCashVoucherByCode/$code" `
    -Method GET -UseBasicParsing
$response.Content | ConvertFrom-Json | Format-List

Write-Host "`n=== 3. Marcando vale como InUse ===" -ForegroundColor Green
$body = @{ inUse = $true } | ConvertTo-Json
$response = Invoke-WebRequest -Uri "http://localhost:5000/api/SetCashVouchersInUse/$code" `
    -Method POST -Body $body -ContentType 'application/json' -UseBasicParsing
$voucher = $response.Content | ConvertFrom-Json
Write-Host "Estado después de SetInUse: $($voucher.Status)" -ForegroundColor Yellow
$voucher | Format-List

Write-Host "`n=== 4. Filtrando vales activos ===" -ForegroundColor Green
$response = Invoke-WebRequest -Uri 'http://localhost:5000/api/GetFilteredCashVouchers?status=0' `
    -Method GET -UseBasicParsing
$vouchers = $response.Content | ConvertFrom-Json
Write-Host "Vales activos encontrados: $($vouchers.Count)" -ForegroundColor Yellow

Write-Host "`n=== 5. Canjeando vale (InUse se establece automáticamente en false) ===" -ForegroundColor Green
$body = @{
    redemptionSaleId = 'REDEMPTION-456'
} | ConvertTo-Json
$response = Invoke-WebRequest -Uri "http://localhost:5000/api/RedeemCashVoucher/$code" `
    -Method PUT -Body $body -ContentType 'application/json' -UseBasicParsing
$response.Content | ConvertFrom-Json | Format-List

Write-Host "`n=== 6. Verificando estado después del canje ===" -ForegroundColor Green
$response = Invoke-WebRequest -Uri "http://localhost:5000/api/GetCashVoucherByCode/$code?onlyActives=false" `
    -Method GET -UseBasicParsing
$voucher = $response.Content | ConvertFrom-Json
Write-Host "Estado final: $($voucher.Status), InUse: $($voucher.InUse)" -ForegroundColor Yellow
$voucherhttp://localhost:5000/api/RedeemCashVoucher/$code" `
    -Method PUT -Body $body -ContentType 'application/json' -UseBasicParsing
$response.Content | ConvertFrom-Json | Format-List

Write-Host "`n=== 5. Verificando estado después del canje ===" -ForegroundColor Green
$response = Invoke-WebRequest -Uri "http://localhost:5000/api/GetCashVoucherByCode/$code?onlyActives=false" `
    -Method GET -UseBasicParsing
$response.Content | ConvertFrom-Json | Format-List
```

## Acceso a Swagger UI

La documentación interactiva de la API está disponible en:
- http://localhost:5000/swagger (cuando la API se ejecuta en modo Development)

Para ejecutar en modo Development:
```powershell
dotnet run --project .\CashVouchersManager.API\CashVouchersManager.API.csproj --environment Development
```

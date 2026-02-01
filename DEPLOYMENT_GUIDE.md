# 🚀 Guía de Despliegue en Railway.app

Esta guía te muestra cómo desplegar **Cash Vouchers Manager API** en Railway.app de forma **GRATUITA** y accesible desde internet.

---

## 🌐 Demo en Vivo

**API ya desplegada y funcionando:**

🔗 **Swagger UI**: [https://cashvouchersmanager-production.up.railway.app/swagger/index.html](https://cashvouchersmanager-production.up.railway.app/swagger/index.html)

**Credenciales de prueba**:
- Usuario: `testuser`
- Contraseña: `testpassword`

---

## 📋 Requisitos Previos

1. **Cuenta de GitHub** (gratuita) - [Registrarse aquí](https://github.com/signup)
2. **Cuenta de Railway** (gratuita) - [Registrarse aquí](https://railway.app)
3. Tu proyecto debe estar en un repositorio de GitHub

---

## ✅ Archivos de Configuración Ya Incluidos

Tu proyecto **ya está listo** con todos los archivos necesarios para Railway:

- ✅ **nixpacks.toml** - Fuerza a Railway a usar .NET SDK 8
- ✅ **global.json** - Especifica la versión del SDK (.NET 8.0)
- ✅ **railway.json** - Configuración de despliegue
- ✅ **Procfile** - Comando de inicio alternativo
- ✅ **Endpoint DeleteAllVouchers** - Para resetear la base de datos

**No necesitas crear nada más.** Solo sigue los pasos de despliegue.

---

## 🔧 PASO 1: Subir el Proyecto a GitHub

### 1.1 Crear Repositorio en GitHub

1. Ve a [github.com](https://github.com) e inicia sesión
2. Haz clic en el botón **"+"** (arriba derecha) → **"New repository"**
3. Configura el repositorio:
   - **Repository name**: `cash-vouchers-manager` (o el nombre que prefieras)
   - **Description**: "API REST para gestión de vales canjeables con .NET 8"
   - **Visibility**: **Public** (necesario para Railway free tier)
   - ❌ **NO** marques "Add a README file" (ya lo tienes)
   - ❌ **NO** marques "Add .gitignore" (ya lo tienes)
4. Haz clic en **"Create repository"**

### 1.2 Subir tu Código a GitHub

Abre PowerShell en la carpeta de tu proyecto y ejecuta:

```powershell
# Inicializar repositorio Git (si no está inicializado)
git init

# Agregar todos los archivos (incluye nixpacks.toml y global.json)
git add .

# Hacer el primer commit
git commit -m "Ready for Railway deployment with .NET 8 configuration"

# Agregar el repositorio remoto (reemplaza TU-USUARIO con tu usuario de GitHub)
git remote add origin https://github.com/TU-USUARIO/cash-vouchers-manager.git

# Subir el código
git branch -M main
git push -u origin main
```

**⚠️ IMPORTANTE**: Asegúrate de que `nixpacks.toml` y `global.json` estén incluidos en el commit. Estos archivos son **esenciales** para que Railway use .NET 8.

**🔑 Autenticación**: GitHub te pedirá credenciales. Usa tu usuario y un **Personal Access Token** (no la contraseña):

**Crear Token**:
1. GitHub → Settings (tu perfil) → Developer settings → Personal access tokens → Tokens (classic)
2. "Generate new token (classic)"
3. Selecciona el scope `repo` (acceso completo a repositorios)
4. Copia el token y úsalo como contraseña al hacer `git push`

---

## 🚂 PASO 2: Desplegar en Railway.app

### 2.1 Crear Cuenta en Railway

1. Ve a [railway.app](https://railway.app)
2. Haz clic en **"Start a New Project"** o **"Login"**
3. Inicia sesión con tu cuenta de **GitHub** (recomendado)
4. Railway te dará **$5 de crédito gratuito al mes** (suficiente para tu API)

### 2.2 Crear Nuevo Proyecto

1. En el dashboard de Railway, haz clic en **"New Project"**
2. Selecciona **"Deploy from GitHub repo"**
3. Railway pedirá permisos para acceder a tus repositorios de GitHub
4. Autoriza a Railway
5. Selecciona el repositorio **`cash-vouchers-manager`** (o el nombre que le pusiste)
6. Railway detectará automáticamente que es un proyecto .NET 8

### 2.3 Configurar Variables de Entorno (Opcional)

Si quieres cambiar las credenciales de autenticación o el puerto:

1. En el dashboard del proyecto en Railway, selecciona tu servicio
2. Ve a la pestaña **"Variables"**
3. Agrega las variables (Railway lee `appsettings.json` automáticamente):
   ```
   AppSettings__Authentication__Username=tu-usuario
   AppSettings__Authentication__Password=tu-contraseña-segura
   ```

**Nota**: Railway asigna automáticamente la variable `PORT`, no necesitas configurarla.

### 2.4 Desplegar

1. Railway comenzará el **build automáticamente**
2. Verás los logs en tiempo real en la pestaña **"Deployments"**
3. El proceso tarda **2-5 minutos**:
   - Detecta .NET 8
   - Ejecuta `dotnet restore`
   - Ejecuta `dotnet build`
   - Ejecuta `dotnet run`
4. Cuando termine, verás **"Success"** ✅

### 2.5 Obtener la URL Pública

1. En el dashboard, ve a la pestaña **"Settings"**
2. En la sección **"Networking"**, haz clic en **"Generate Domain"**
3. Railway generará una URL pública como:
   ```
   https://cash-vouchers-manager-production.up.railway.app
   ```
4. **¡Copia esta URL!** Es tu API pública

---

## 🧪 PASO 3: Probar la API en Internet

### 3.1 Acceder a Swagger

Abre tu navegador y ve a:
```
https://TU-URL.railway.app/swagger
```

Ejemplo:
```
https://cash-vouchers-manager-production.up.railway.app/swagger
```

Verás la interfaz de Swagger con todos los endpoints.

### 3.2 Probar un Endpoint desde PowerShell

```powershell
# Codificar credenciales (admin:admin123)
$credentials = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("admin:admin123"))

# Generar un vale
$body = @{
    amount = 100.00
    issuingStoreId = 1234
    expirationDate = "2027-12-31T23:59:59Z"
    issuingSaleId = "SALE-DEMO-001"
} | ConvertTo-Json

$response = Invoke-WebRequest `
    -Uri 'https://TU-URL.railway.app/api/GenerateCashVoucher' `
    -Method POST `
    -Headers @{ Authorization = "Basic $credentials" } `
    -Body $body `
    -ContentType 'application/json' `
    -UseBasicParsing

$response.Content | ConvertFrom-Json | ConvertTo-Json
```

### 3.3 Resetear la Base de Datos

Para vaciar la base de datos cuando quieras:

```powershell
# Desde PowerShell
$credentials = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("admin:admin123"))

Invoke-WebRequest `
    -Uri 'https://TU-URL.railway.app/api/DeleteAllVouchers' `
    -Method DELETE `
    -Headers @{ Authorization = "Basic $credentials" } `
    -UseBasicParsing
```

O desde Swagger:
1. Ve a `https://TU-URL.railway.app/swagger`
2. Busca el endpoint **DELETE /api/DeleteAllVouchers**
3. Haz clic en **"Try it out"** → **"Execute"**
4. Introduce las credenciales cuando te las pida

---

## 📊 PASO 4: Monitorear y Gestionar

### 4.1 Ver Logs en Tiempo Real

En Railway dashboard:
1. Selecciona tu servicio
2. Ve a la pestaña **"Deployments"**
3. Haz clic en el deployment activo
4. Verás logs en tiempo real de la aplicación

### 4.2 Persistencia de SQLite

Railway tiene **sistema de archivos efímero** por defecto. Para que SQLite sea persistente:

**Opción A: Usar Volumen Persistente (Recomendado)**

1. En Railway dashboard, ve a tu servicio
2. Pestaña **"Settings"** → **"Volumes"**
3. Haz clic en **"Add Volume"**
4. Configura:
   - **Mount Path**: `/app/CashVouchersManager.API` (donde se crea el .db)
   - **Name**: `sqlite-data`
5. Haz clic en **"Add"**
6. Redeploy: `git commit --allow-empty -m "Add volume" && git push`

**Nota**: Los volúmenes en Railway free tier pueden tener límites. Si quieres 100% persistencia sin preocupaciones, considera los **$5/mes** del plan Hobby.

**Opción B: Aceptar Reseteo (Para Demos/Testing)**

Si es solo para testear/demos, puedes aceptar que la BD se resetee al reiniciar. Usa el endpoint `DeleteAllVouchers` para limpiar cuando quieras.

### 4.3 Actualizar el Código

Cuando hagas cambios en tu código local:

```powershell
# Agregar cambios
git add .

# Commit
git commit -m "Descripción de tus cambios"

# Push a GitHub
git push

# Railway detectará el push y redeployará automáticamente
```

---

## 🎯 PASO 5: Compartir tu API

Ahora puedes compartir tu API con quien quieras:

**URL de la API**:
```
https://TU-URL.railway.app
```

**URL de Swagger (documentación interactiva)**:
```
https://TU-URL.railway.app/swagger
```

**Credenciales de acceso**:
- Usuario: `admin`
- Contraseña: `admin123`

**Instrucciones para otros usuarios**:
1. Ir a la URL de Swagger
2. Probar endpoints directamente desde el navegador
3. Para autenticarse en Swagger:
   - Hacer clic en el botón **"Authorize"** (candado)
   - Ingresar usuario y contraseña
   - Hacer clic en **"Authorize"**

---

## 💡 Consejos Adicionales

### Cambiar Credenciales

Edita `appsettings.json` en tu proyecto local:

```json
{
  "AppSettings": {
    "Authentication": {
      "Username": "nuevo-usuario",
      "Password": "contraseña-segura-123"
    }
  }
}
```

Luego haz push a GitHub:
```powershell
git add appsettings.json
git commit -m "Update credentials"
git push
```

### Personalizar el Dominio

En Railway (plan de pago):
1. Settings → Networking → Custom Domain
2. Agrega tu dominio propio (ej: `api.miempresa.com`)

### Límites del Free Tier

Railway free tier incluye:
- **$5 USD de crédito mensual**
- Suficiente para ~500 horas de ejecución ligera
- Si se acaba el crédito, la app se pausará hasta el siguiente mes
- Ideal para demos, prototipos, y testing

---

## 🆘 Solución de Problemas

### Error: "NETSDK1045: The current .NET SDK does not support targeting .NET 8.0"

**Causa**: Railway está usando .NET SDK 6 en lugar de .NET 8.

**Solución** (YA IMPLEMENTADA):
1. ✅ Tu proyecto ya incluye `nixpacks.toml` que fuerza .NET 8
2. ✅ Tu proyecto ya incluye `global.json` que especifica .NET 8.0
3. Haz commit de estos archivos:
   ```powershell
   git add nixpacks.toml global.json railway.json Program.cs
   git commit -m "Fix: Railway deployment with .NET 8 and proper port configuration"
   git push
   ```
4. Railway detectará el push y volverá a desplegar automáticamente
5. Esta vez usará .NET 8 correctamente

**Verificar que los archivos estén en GitHub**:
1. Ve a tu repositorio en GitHub
2. Busca `nixpacks.toml` en la raíz
3. Busca `global.json` en la raíz
4. Si no están, agrégalos y haz push de nuevo

### Error: "Application crashed" después de desplegar

**Causa**: La aplicación no está escuchando en el puerto correcto o en la interfaz correcta para Railway.

**Solución** (YA IMPLEMENTADA):
1. ✅ `Program.cs` ya está configurado para:
   - Leer el puerto de la variable de entorno `PORT` (Railway lo establece automáticamente)
   - Escuchar en `0.0.0.0` en producción (necesario para Railway)
   - Escuchar en `localhost` en desarrollo (para tu máquina local)
2. ✅ La base de datos SQLite se crea en `/data` en producción (compatible con volúmenes de Railway)
3. ✅ Los logs ahora muestran información de inicio detallada

**Si ya desplegaste antes de estos cambios**:
```powershell
# Hacer commit de los cambios en Program.cs
git add CashVouchersManager.API/Program.cs
git commit -m "Fix: Configure proper host and port for Railway"
git push
```

**Verificar en los logs de Railway** que ahora veas:
```
info: Starting Cash Vouchers Manager API
info: Environment: Production
info: Listening on: http://0.0.0.0:XXXX
info: Database path: /data/CashVouchers.db
info: Now listening on: http://0.0.0.0:XXXX
```

### Error: "Build failed"

**Causa**: Faltan archivos o configuración incorrecta.

**Solución**:
1. Verifica que `nixpacks.toml`, `global.json` y `railway.json` estén en la raíz del repositorio
2. Revisa los logs de build en Railway
3. Asegúrate de que `dotnet build` funcione localmente
4. Verifica que estés usando .NET 8 SDK localmente

### Error: "Application crashed"

**Causa**: Error al iniciar la aplicación.

**Solución**:
1. Revisa los logs en Railway (pestaña Deployments)
2. Verifica que el puerto se lea de la variable `PORT`
3. Comprueba que las migraciones de EF Core se apliquen correctamente
4. Asegúrate de que la variable de entorno `PORT` esté configurada (Railway lo hace automáticamente)

### La Base de Datos se Borra al Reiniciar

**Causa**: Sistema de archivos efímero sin volumen.

**Solución**:
- Agrega un volumen persistente (ver Paso 4.2)
- O acepta el comportamiento para demos/testing

### No Puedo Hacer Push a GitHub

**Causa**: Credenciales incorrectas.

**Solución**:
- Usa un **Personal Access Token** en lugar de tu contraseña
- Revisa la sección 1.2 para crear el token

---

## 📚 Recursos Adicionales

- [Documentación de Railway](https://docs.railway.app)
- [.NET en Railway](https://docs.railway.app/guides/dotnet)
- [GitHub Docs](https://docs.github.com)

---

## ✅ Checklist Final

- [ ] Código subido a GitHub
- [ ] Proyecto creado en Railway
- [ ] Deploy exitoso (status: Success)
- [ ] URL pública generada
- [ ] Swagger accesible desde navegador
- [ ] Endpoint de prueba funciona
- [ ] Endpoint DeleteAllVouchers funciona
- [ ] URL compartida con otros usuarios

---

**¡Listo! Tu API está en internet y accesible para todo el mundo.** 🎉

Cualquier persona con la URL y las credenciales puede probar tu API desde cualquier parte del mundo.

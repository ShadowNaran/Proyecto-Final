# Catálogo Audiovisual API — Proyecto Final Tecnología Web 1

Sistema web para gestionar un catálogo de películas y series, su talento, producción, plataformas de streaming y la actividad de la comunidad. Expone una API REST en ASP.NET Core (.NET 10) sobre PostgreSQL, autenticación delegada con OAuth 2.0/OpenID Connect, frontend estático, migraciones automáticas y despliegue en contenedores.

| Recurso | URL |
|---|---|
| Repositorio público | https://github.com/emanuelkXD/Proyecto-Final |
| Despliegue público (Render) | https://proyecto-finalupds.onrender.com |
| Documentación interactiva (Scalar) | https://proyecto-finalupds.onrender.com/scalar/v1 |

---

## 1. Integrantes y reparto de responsabilidades

| Integrante | Tablas asignadas | Reporte autorizado | Política | Proveedores por defecto | Dockerfile individual |
|---|---|---|---|---|---|
| Abraham Valeriano | `categorias`, `peliculas`, `series`, `temporadas` | `GET /api/reportes/catalogo-abraham` | `ReporteAbrahamPolicy` | `Auth0`, `Discord`, `Spotify`, `TikTok` | `Dockerfile.AbrahamValeriano` |
| Marco Antonio Arias Giron | `resenas`, `listas_reproduccion`, `favoritos`, `premios` | `GET /api/reportes/comunidad-marco` | `ReporteMarcoPolicy` | `Dropbox`, `Slack` | `Dockerfile.MarcoArias` |
| Emanuel Rafael Kilibarda Rocha | `directores`, `actores`, `productoras`, `plataformas` | `GET /api/reportes/talento-emanuel` | `ReporteEmanuelPolicy` | `Auth0`, `TikTok` | `Dockerfile.EmanuelKilibarda` |

### 1.1 Abraham Valeriano — catálogo

- **Autor:** Abraham Valeriano
- **Endpoint:** `GET /api/reportes/catalogo-abraham`
- **Propósito:** consolidar el catálogo activo por categoría, combinando películas, series, temporadas y episodios.
- **Tablas cruzadas:** `categorias`, `peliculas`, `series`, `temporadas`.
- **Respuesta:** totales generales (categorías, películas, series, temporadas, episodios y duración promedio) y desglose por categoría con títulos, cantidades y episodios.
- **Autorización backend:** `ReporteAbrahamPolicy` en `CatalogoApi/Program.cs`.
- **Dockerfile:** `Dockerfile.AbrahamValeriano`, usado por `docker-compose.dev.yml`.

### 1.2 Marco Antonio Arias Giron — comunidad

- **Autor:** Marco Antonio Arias Giron
- **Endpoint:** `GET /api/reportes/comunidad-marco`
- **Propósito:** medir la actividad de la comunidad por obra: reseñas, calificación promedio, favoritos, premios y listas de reproducción activas.
- **Tablas cruzadas:** `resenas`, `listas_reproduccion`, `favoritos`, `premios`.
- **Respuesta:** resumen global, desglose por obra y listado de listas de reproducción con propietario, visibilidad y descripción.
- **Autorización backend:** `ReporteMarcoPolicy` en `CatalogoApi/Program.cs`.
- **Dockerfile:** `Dockerfile.MarcoArias`.
- **Demostración de `403 Forbidden`:** la política solo admite `Dropbox` y `Slack`. Una sesión iniciada con `Auth0` puede consultar los reportes de Abraham y Emanuel, pero recibe `403` al invocar `comunidad-marco`.

### 1.3 Emanuel Rafael Kilibarda Rocha — talento y producción

- **Autor:** Emanuel Rafael Kilibarda Rocha
- **Endpoint:** `GET /api/reportes/talento-emanuel`
- **Propósito:** consolidar directores, actores, productoras y plataformas activas, agrupando talento y producción por país y calculando el precio promedio de suscripción.
- **Tablas cruzadas:** `directores`, `actores`, `productoras`, `plataformas`.
- **Respuesta:** totales de talento, productoras, plataformas y países; desglose por país con listas nominales; plataformas de distribución con sitio, país y precio.
- **Autorización backend:** `ReporteEmanuelPolicy` en `CatalogoApi/Program.cs`.
- **Dockerfile:** `Dockerfile.EmanuelKilibarda`; es el Dockerfile utilizado para publicar la imagen final en Docker Hub.

---

## 2. Arquitectura

```text
Navegador
   │ HTTP/HTTPS
   ├── Frontend estático (wwwroot o Nginx en desarrollo)
   │
   └── ASP.NET Core .NET 10
         ├── Cookie de sesión CatalogoApi.Auth
         ├── OAuth 2.0 / OpenID Connect (Auth0, TikTok, Discord, Spotify,
         │   Dropbox, Slack, Twitch y X/Twitter)
         ├── Políticas de autorización por reporte
         ├── EF Core 10 + Npgsql
         └── PostgreSQL 17
```

- **Backend:** `CatalogoApi/`, ASP.NET Core `net10.0`, controladores con DTOs, EF Core y migraciones.
- **Base de datos:** PostgreSQL 17 (imagen `postgres:17-alpine` en Compose).
- **Frontend:** `CatalogoApi/wwwroot/index.html`, HTML/CSS/JS sin framework; en desarrollo también se sirve mediante Nginx.
- **Documentación:** OpenAPI en `/openapi/v1.json` y Scalar en `/scalar/v1`.
- **Sesión:** cookie `CatalogoApi.Auth`, `HttpOnly` y `SameSite=Lax`; expiración gestionada por el handler de cookies.
- **Contenedores:** Dockerfiles multi-etapa .NET 10 y Compose para desarrollo y release.

---

## 3. Modelo de datos

Todas las tablas incluyen `id`, `activo`, `creado_en` y `eliminado_en`. El borrado es lógico: los `GET` filtran `activo = true` y `DELETE` marca `activo = false` y `eliminado_en`.

| Tabla | Responsable | Campos principales | Restricciones |
|---|---|---|---|
| `categorias` | Abraham | `nombre`, `descripcion` | `nombre` único |
| `peliculas` | Abraham | `titulo`, `sinopsis`, `anio_estreno`, `duracion_min`, `clasificacion`, `poster_url`, `categoria_id` | FK opcional a `categorias`; al eliminar categoría, `SET NULL` |
| `series` | Abraham | `titulo`, `sinopsis`, `anio_inicio`, `en_emision`, `poster_url`, `categoria_id` | FK opcional a `categorias`; al eliminar categoría, `SET NULL` |
| `temporadas` | Abraham | `numero`, `titulo`, `anio_estreno`, `cantidad_episodios`, `serie_id` | FK obligatoria a `series`; borrado `RESTRICT` |
| `directores` | Emanuel | `nombre`, `nacionalidad`, `fecha_nacimiento`, `biografia` | — |
| `actores` | Emanuel | `nombre`, `nacionalidad`, `fecha_nacimiento`, `biografia` | — |
| `productoras` | Emanuel | `nombre`, `pais`, `anio_fundacion`, `sitio_web` | `nombre` único |
| `plataformas` | Emanuel | `nombre`, `sitio_web`, `precio_mensual`, `pais_disponible` | `nombre` único |
| `resenas` | Marco | `autor`, `tipo_contenido`, `titulo_obra`, `puntuacion`, `comentario` | `puntuacion` entre 1 y 5 |
| `listas_reproduccion` | Marco | `nombre`, `descripcion`, `es_publica`, `propietario` | — |
| `favoritos` | Marco | `usuario_email`, `tipo_contenido`, `titulo_obra`, `nota_personal` | — |
| `premios` | Marco | `nombre`, `categoria_premio`, `anio`, `obra_ganadora` | — |

La migración inicial es `20261123214251_InitialPostgres`. Al iniciar, la API ejecuta `db.Database.Migrate()` con hasta 5 intentos separados por 2 segundos para tolerar el arranque de PostgreSQL.

---

## 4. API REST

### 4.1 Endpoints de autenticación

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/auth/login/sso` | Inicia OpenID Connect con Auth0. |
| `GET` | `/api/auth/login/tiktok` | Inicia OAuth 2.0 con TikTok Login Kit v2. |
| `GET` | `/api/auth/login/discord` | Inicia OAuth 2.0 con Discord. |
| `GET` | `/api/auth/login/spotify` | Inicia OAuth 2.0 con Spotify. |
| `GET` | `/api/auth/login/dropbox` | Inicia OAuth 2.0 con Dropbox. |
| `GET` | `/api/auth/login/slack` | Inicia OAuth 2.0 con Slack. |
| `GET` | `/api/auth/login/twitch` | Inicia OAuth 2.0 con Twitch. |
| `GET` | `/api/auth/login/twitter` | Inicia OAuth 2.0 con X/Twitter. |
| `GET` | `/api/auth/providers` | Indica qué proveedores tienen credenciales configuradas. |
| `GET` | `/api/auth/me` | Devuelve `{ autenticado, proveedor, usuario, id }` o `{ autenticado: false }`. |
| `POST` | `/api/auth/logout` | Elimina la cookie de sesión. |

### 4.2 CRUD protegido

Todos los controladores de catálogo llevan `[Authorize]`: sin cookie válida, las rutas `/api/*` responden `401 Unauthorized` (no redirigen a HTML).

| Recurso | Rutas | Operaciones |
|---|---|---|
| Categorías | `/api/categorias` | `GET`, `GET/{id}`, `POST`, `PUT/{id}`, `DELETE/{id}` |
| Películas | `/api/peliculas` | `GET`, `GET/{id}`, `POST`, `PUT/{id}`, `DELETE/{id}` |
| Series | `/api/series` | `GET`, `GET/{id}`, `POST`, `PUT/{id}`, `DELETE/{id}` |
| Temporadas | `/api/temporadas` | `GET`, `GET/{id}`, `POST`, `PUT/{id}`, `DELETE/{id}` |
| Directores | `/api/directores` | `GET`, `GET/{id}`, `POST`, `PUT/{id}`, `DELETE/{id}` |
| Actores | `/api/actores` | `GET`, `GET/{id}`, `POST`, `PUT/{id}`, `DELETE/{id}` |
| Productoras | `/api/productoras` | `GET`, `GET/{id}`, `POST`, `PUT/{id}`, `DELETE/{id}` |
| Plataformas | `/api/plataformas` | `GET`, `GET/{id}`, `POST`, `PUT/{id}`, `DELETE/{id}` |
| Reseñas | `/api/resenas` | `GET`, `GET/{id}`, `POST`, `PUT/{id}`, `DELETE/{id}` |
| Listas de reproducción | `/api/listas-reproduccion` | `GET`, `GET/{id}`, `POST`, `PUT/{id}`, `DELETE/{id}` |
| Favoritos | `/api/favoritos` | `GET`, `GET/{id}`, `POST`, `PUT/{id}`, `DELETE/{id}` |
| Premios | `/api/premios` | `GET`, `GET/{id}`, `POST`, `PUT/{id}`, `DELETE/{id}` |

Los `GET` devuelven solo registros activos; los `DELETE` aplican soft delete. Los recursos relacionados validan que la categoría o serie exista y esté activa antes de guardar.

### 4.3 Endpoints de reportes

| Método | Ruta | Política | Resultado sin sesión | Resultado con proveedor no autorizado |
|---|---|---|---|---|
| `GET` | `/api/reportes/catalogo-abraham` | `ReporteAbrahamPolicy` | `401` | `403` |
| `GET` | `/api/reportes/comunidad-marco` | `ReporteMarcoPolicy` | `401` | `403` |
| `GET` | `/api/reportes/talento-emanuel` | `ReporteEmanuelPolicy` | `401` | `403` |

Cada política exige un usuario autenticado y valida el claim `provider` contra la lista configurada. Opcionalmente, `*_ALLOWED_USERS` permite autorizar usuarios concretos por nombre, email, `sub` o id.

---

## 5. Autenticación y autorización

### 5.1 Flujo OAuth 2.0 / OIDC

1. El usuario pulsa un botón de login del frontend.
2. `AuthController` emite `Challenge` con el esquema del proveedor y la URL de retorno configurada.
3. El middleware redirige al proveedor y el usuario concede permisos.
4. El proveedor vuelve a la ruta de callback (`/signin-auth0`, `/signin-tiktok`, `/signin-discord`, etc.).
5. El handler valida el estado/PKCE cuando corresponde, canjea el código por tokens y consulta el perfil.
6. Se agrega el claim `provider` y se emite la cookie `CatalogoApi.Auth`.
7. `GET /api/auth/me` expone el estado de la sesión y `POST /api/auth/logout` la elimina.

Detalles de implementación:

- **Auth0:** OpenID Connect con scopes `openid`, `profile` y `email`; mapea `sub`, `name` y `email`.
- **TikTok:** handler propio `TikTokHandler` para Login Kit v2; usa `client_key`, scope `user.info.basic` y el endpoint `open.tiktokapis.com`. `ALLOWED_TIKTOK_OPEN_ID` permite restringir el acceso a una cuenta.
- **Slack:** obtiene el user token desde `authed_user.access_token` y consulta `users.identity` para completar nombre, id y email.
- **Discord, Spotify, Dropbox, Twitch y X/Twitter:** se registran con los paquetes `AspNet.Security.OAuth.*` y sus callbacks `/signin-*`.

### 5.2 Políticas de reportes

Las políticas se registran en `CatalogoApi/Program.cs`:

```text
ReporteAbrahamPolicy  -> REPORTE_ALLOWED_PROVIDERS          (Auth0,Discord,Spotify,TikTok)
ReporteMarcoPolicy    -> REPORTE_MARCO_ALLOWED_PROVIDERS    (Dropbox,Slack)
ReporteEmanuelPolicy  -> REPORTE_EMANUEL_ALLOWED_PROVIDERS  (Auth0,TikTok)
```

Cada política acepta también las variables `*_ALLOWED_USERS`. Si el proveedor está autorizado, el acceso se concede; si no, se evalúa la lista de usuarios. La cookie de sesión no se comparte entre `localhost` y `127.0.0.1` (son hosts distintos), así que para probar los 4 puertos hay que usar el mismo hostname.

### 5.3 Respuestas de autorización

- **Sin sesión:** `401 Unauthorized` en `/api/*`.
- **Sesión válida pero proveedor no permitido:** `403 Forbidden`.
- **Sesión válida y proveedor permitido:** `200 OK`.

Ejemplo de la demostración solicitada:

1. Iniciar sesión con Auth0 en `http://localhost:5001`.
2. Consultar `http://localhost:5001/api/reportes/catalogo-abraham` → `200`.
3. Consultar `http://localhost:5002/api/reportes/comunidad-marco` → `403`, porque Marco solo admite `Dropbox` y `Slack`.
4. Consultar `http://localhost:5003/api/reportes/talento-emanuel` → `200`, porque Emanuel admite `Auth0`.
5. Consultar `http://localhost:5004/api/auth/me` → sesión activa, sin volver a iniciar sesión.

---

## 6. Puertos

| Puerto host | Servicio | Puerto interno | Entorno |
|---|---|---|---|
| `3000` | Frontend Nginx | `80` | Desarrollo |
| `5174` | Backend API / Scalar | `8080` | Desarrollo |
| `5433` | PostgreSQL | `5432` | Desarrollo |
| `5001` | `api-1` | `8080` | Release |
| `5002` | `api-2` | `8080` | Release |
| `5003` | `api-3` | `8080` | Release |
| `5004` | `api-4` | `8080` | Release |
| No publicado | PostgreSQL `db` | `5432` | Release (solo red interna) |
| `443` | Render (HTTPS) | `8080` | Producción |
| `80` | Nginx | `80` | Desarrollo (contenedor) |

El puerto interno del backend es siempre `8080`; los Dockerfiles lo exponen y Compose lo publica en el host.

---

## 7. Entorno de desarrollo (`docker-compose.dev.yml`)

Servicios:

- **`db`:** PostgreSQL 17 Alpine, puerto `5433:5432`, volumen `pgdata` y healthcheck `pg_isready`.
- **`backend`:** se construye con `Dockerfile.AbrahamValeriano`, publica `5174:8080`, usa `dpkeys:/app/keys` y espera a que `db` esté saludable.
- **`frontend`:** Nginx Alpine, publica `3000:80`, monta `CatalogoApi/wwwroot/index.html` y `frontend/config.js`; llama a la API en `http://localhost:5174`.

```bash
cp .env.example .env
# completar credenciales en .env
docker compose -f docker-compose.dev.yml up --build
```

URLs:

- Frontend: http://localhost:3000
- API: http://localhost:5174
- Scalar: http://localhost:5174/scalar/v1
- PostgreSQL: `localhost:5433`

El backend aplica las migraciones al iniciar. El frontend de Nginx es una vista estática; la API también sirve el mismo `index.html` en su raíz.

---

## 8. Dockerfiles individuales

Los tres Dockerfiles individuales usan compilación multi-etapa con `mcr.microsoft.com/dotnet/sdk:10.0` y `mcr.microsoft.com/dotnet/aspnet:10.0`, publican en `Release` y ejecutan con el usuario sin privilegios `app`.

| Dockerfile | Integrante | Uso |
|---|---|---|
| `Dockerfile.AbrahamValeriano` | Abraham Valeriano | Build del backend en `docker-compose.dev.yml`; crea `/app/keys` y define `DATA_PROTECTION_KEYS_PATH=/app/keys`. |
| `Dockerfile.MarcoArias` | Marco Antonio Arias Giron | Imagen individual multi-etapa; mismas garantías de puerto `8080`, usuario `app` y claves en `/app/keys`. |
| `Dockerfile.EmanuelKilibarda` | Emanuel Rafael Kilibarda Rocha | Imagen final publicada en Docker Hub: `emanuelxd/proyecto-final:1.0.0`. |

`Dockerfile.EmanuelKilibarda`:

- **Etapa `build`:** restaura `CatalogoApi.csproj` y ejecuta `dotnet publish -c Release -o /app/publish --no-restore /p:UseAppHost=false`.
- **Etapa `final`:** parte de `aspnet:10.0`, copia la publicación, crea `/app/keys`, asigna permisos a `app`, define `ASPNETCORE_URLS=http://+:8080`, `DATA_PROTECTION_KEYS_PATH=/app/keys` y ejecuta con `USER app`.
- **Puerto interno:** `8080`.

Construcción y publicación de la imagen:

```bash
docker build -f Dockerfile.EmanuelKilibarda -t emanuelxd/proyecto-final:1.0.0 .
docker push emanuelxd/proyecto-final:1.0.0
```

`Dockerfile` (sin sufijo) es un Dockerfile genérico adicional del proyecto; no es el usado para generar la imagen publicada.

---

## 9. Release multi-instancia — Punto 7 (20 pts)

`docker-compose.release.yml` descarga la imagen publicada y **no compila código fuente**. No contiene ninguna sección `build:`.

### 9.1 Topología

```text
                         Docker Compose (catalogo_release_net)
                         │
   localhost:5001 ── api-1 ──┐
   localhost:5002 ── api-2 ──┼── db:5432 (PostgreSQL 17)
   localhost:5003 ── api-3 ──┤      └── pgdata_release
   localhost:5004 ── api-4 ──┘
                         │
                         └── volumen compartido dpkeys_release:/app/keys
```

| Servicio | Imagen | Puertos | Dependencia |
|---|---|---|---|
| `api-1` | `${DOCKER_IMAGE:-emanuelxd/proyecto-final:1.0.0}` | `5001:8080` | `db` saludable |
| `api-2` | misma imagen | `5002:8080` | `db` saludable |
| `api-3` | misma imagen | `5003:8080` | `db` saludable |
| `api-4` | misma imagen | `5004:8080` | `db` saludable |
| `db` | `postgres:17-alpine` | solo interno `5432` | — |

Características verificadas:

- **Imagen por defecto:** `${DOCKER_IMAGE:-emanuelxd/proyecto-final:1.0.0}` con `pull_policy: always`.
- **Sin build:** los 5 servicios (`api-1` a `api-4` y `db`) no tienen `build:`.
- **Cuatro instancias exactas:** `api-1`, `api-2`, `api-3` y `api-4`, publicadas en `5001`, `5002`, `5003` y `5004`.
- **Misma base de datos:** las 4 usan `DB_CONNECTION_STRING=Host=db;Port=5432;...` y el volumen persistente `pgdata_release`.
- **Misma red:** todas en `catalogo_release_net`.
- **Mismas claves:** las 4 montan `dpkeys_release:/app/keys` y `DATA_PROTECTION_KEYS_PATH=/app/keys`.

### 9.2 Sesión compartida entre los 4 puertos

La cookie de autenticación se llama `CatalogoApi.Auth`. Las cookies no incluyen el puerto en su ámbito, por lo que una cookie emitida para `localhost` se envía también a `localhost:5002`, `localhost:5003` y `localhost:5004`.

Para que la cookie sea descifrable por otra instancia, las 4 comparten el mismo anillo de claves de ASP.NET Core Data Protection:

- `Program.cs` configura `.PersistKeysToFileSystem(new DirectoryInfo(keysPath))` y `.SetApplicationName("CatalogoApi")`.
- `keysPath` proviene de `DATA_PROTECTION_KEYS_PATH` (`/app/keys` dentro de los contenedores).
- El volumen `dpkeys_release` se monta en el mismo punto en las 4 instancias.

Resultado: si se inicia sesión en `http://localhost:5001`, `GET /api/auth/me` en `http://localhost:5002` reconoce la misma sesión sin volver a autenticarse. No mezclar `localhost` con `127.0.0.1` durante la prueba: son hosts distintos y el navegador no comparte la cookie entre ambos.

### 9.3 CORS y redirección posterior al login

- `CORS_ORIGINS` incluye `http://localhost:3000`, `http://localhost:5174` y `http://localhost:5001` hasta `http://localhost:5004` (también las variantes `127.0.0.1`). El backend configura `AllowCredentials()` para aceptar la cookie de sesión.
- En release, `FRONTEND_URL` se define como `${RELEASE_FRONTEND_URL:-}`. Al quedar vacío, `AuthController` redirige a `/` y cada instancia vuelve a su propio `host:puerto`; así no hay conflicto entre `5001`, `5002`, `5003` y `5004`.
- Si se despliega un frontend separado, definir `RELEASE_FRONTEND_URL` con su URL pública. En ese caso, el login siempre vuelve a ese frontend, no a la instancia de origen.
- Los proveedores OAuth calculan el `redirect_uri` a partir del host y puerto de la petición. Antes de probar el login en los 4 puertos hay que registrar en cada consola los callbacks correspondientes:
  - `http://localhost:5001/signin-auth0` … `http://localhost:5004/signin-auth0`
  - `http://localhost:5001/signin-tiktok` … `http://localhost:5004/signin-tiktok`
  - equivalentes para Discord, Spotify, Dropbox, Slack, Twitch y X/Twitter.
- En TikTok, `RELEASE_TIKTOK_CALLBACK_URL` se deja como ruta relativa (`/signin-tiktok`) para que cada instancia construya su propio callback.

### 9.4 Comandos de release

```bash
# validar sintaxis e interpolación
docker compose -f docker-compose.release.yml config

# levantar las 4 instancias y PostgreSQL
docker compose -f docker-compose.release.yml up -d

# estado y logs
docker compose -f docker-compose.release.yml ps
docker compose -f docker-compose.release.yml logs -f api-1

# detener sin borrar volúmenes
docker compose -f docker-compose.release.yml down
```

Verificación rápida:

```bash
for p in 5001 5002 5003 5004; do
  echo -n "$p: "
  curl -s "http://localhost:$p/api/auth/me"
  echo
done
```

Salida esperada sin sesión: `{"autenticado":false}` en los cuatro puertos.

---

## 10. Despliegue público en Render

- **URL:** https://proyecto-finalupds.onrender.com
- **Frontend/API:** el backend sirve el `index.html` en `/` y la documentación en `/scalar/v1`.
- **Salud verificada:** `GET /api/auth/me` responde `200` con `{"autenticado":false}`; `GET /api/reportes/catalogo-abraham` sin sesión responde `401`; `/scalar/v1` responde `200`.
- **Variables:** se configuran en el panel de Render, nunca en el repositorio. Como mínimo: `DB_CONNECTION_STRING`, `AUTH0_DOMAIN`, `AUTH0_CLIENT_ID`, `AUTH0_CLIENT_SECRET`, `TIKTOK_CLIENT_KEY`, `TIKTOK_CLIENT_SECRET`, `FRONTEND_URL=https://proyecto-finalupds.onrender.com`, `CORS_ORIGINS`, `DATA_PROTECTION_KEYS_PATH` y las políticas `REPORTE_*`.
- **Proxy TLS:** Render termina HTTPS y la API usa `UseForwardedHeaders()` para reconocer `X-Forwarded-Proto` y `X-Forwarded-For`.
- **Callbacks de producción:** registrar en Auth0/TikTok `https://proyecto-finalupds.onrender.com/signin-auth0` y `https://proyecto-finalupds.onrender.com/signin-tiktok`.
- **Proveedores activos en la instancia pública:** Auth0 y TikTok están configurados. Discord, Spotify, Dropbox, Slack, Twitch y X/Twitter están implementados y se habilitan al cargar sus credenciales y callbacks.
- **Cold start:** el plan gratuito de Render puede suspender la instancia por inactividad; la primera petición puede tardar unos segundos.

La imagen de Docker Hub `emanuelxd/proyecto-final:1.0.0` es el artefacto del entorno release local. El despliegue de Render se configura con las variables de entorno del panel y expone la misma aplicación en la URL pública indicada.

---

## 11. Variables de entorno

El archivo canónico es `.env.example`; copiarlo a `.env` y completar solo las credenciales necesarias. `.env` está excluido por `.gitignore`.

| Grupo | Variables | Uso |
|---|---|---|
| PostgreSQL | `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`, `PG_HOST_PORT` | Credenciales y puerto de desarrollo. |
| Conexión API | `DB_CONNECTION_STRING`, `ConnectionStrings__DefaultConnection` | Cadena para ejecución local fuera de Docker. En Compose se sobrescribe con `Host=db`. |
| Imagen release | `DOCKER_IMAGE` | Tag descargado por `docker-compose.release.yml`; por defecto `emanuelxd/proyecto-final:1.0.0`. |
| Puertos dev | `API_PORT`, `FRONTEND_PORT` | `5174` y `3000` por defecto. |
| Frontend/CORS | `FRONTEND_URL`, `CORS_ORIGINS` | URL de retorno del login y orígenes permitidos con credenciales. |
| Data Protection | `DATA_PROTECTION_KEYS_PATH` | Ruta del anillo de claves; `/app/keys` en contenedores. |
| Release | `RELEASE_FRONTEND_URL`, `RELEASE_TIKTOK_CALLBACK_URL` | Permiten fijar un frontend o un callback TikTok. Para los 4 puertos, dejar `RELEASE_FRONTEND_URL` vacío y `RELEASE_TIKTOK_CALLBACK_URL` relativo (`/signin-tiktok`). |
| Reporte Abraham | `REPORTE_ALLOWED_PROVIDERS`, `REPORTE_ALLOWED_USERS` | Defaults: `Auth0,Discord,Spotify,TikTok`; lista de usuarios opcional. |
| Reporte Marco | `REPORTE_MARCO_ALLOWED_PROVIDERS`, `REPORTE_MARCO_ALLOWED_USERS` | Defaults: `Dropbox,Slack`; lista de usuarios opcional. |
| Reporte Emanuel | `REPORTE_EMANUEL_ALLOWED_PROVIDERS`, `REPORTE_EMANUEL_ALLOWED_USERS` | Defaults: `Auth0,TikTok`; lista de usuarios opcional. |
| Auth0 | `AUTH0_DOMAIN`, `AUTH0_CLIENT_ID`, `AUTH0_CLIENT_SECRET`, `AUTH0_CALLBACK_PATH` | OpenID Connect. `AUTH0_CALLBACK_PATH=/signin-auth0`. |
| TikTok | `TIKTOK_CLIENT_KEY`, `TIKTOK_CLIENT_SECRET`, `TIKTOK_CALLBACK_URL`, `ALLOWED_TIKTOK_OPEN_ID` | Login Kit v2; el open_id vacío permite cualquier cuenta. |
| Discord | `DISCORD_CLIENT_ID`, `DISCORD_CLIENT_SECRET`, `DISCORD_CALLBACK_PATH` | OAuth 2.0. |
| Spotify | `SPOTIFY_CLIENT_ID`, `SPOTIFY_CLIENT_SECRET`, `SPOTIFY_CALLBACK_PATH` | OAuth 2.0. |
| Dropbox | `DROPBOX_CLIENT_ID`, `DROPBOX_CLIENT_SECRET`, `DROPBOX_CALLBACK_PATH` | OAuth 2.0. |
| Slack | `SLACK_CLIENT_ID`, `SLACK_CLIENT_SECRET`, `SLACK_CALLBACK_PATH` | OAuth 2.0. |
| Twitch | `TWITCH_CLIENT_ID`, `TWITCH_CLIENT_SECRET`, `TWITCH_CALLBACK_PATH` | OAuth 2.0. |
| X/Twitter | `TWITTER_CLIENT_ID`, `TWITTER_CLIENT_SECRET`, `TWITTER_CALLBACK_PATH` | OAuth 2.0. |

Los valores vacíos deshabilitan el proveedor correspondiente; `GET /api/auth/providers` permite comprobar el estado sin exponer secretos.

---

## 12. Puesta en marcha y pruebas

### 12.1 Desarrollo

```bash
cp .env.example .env
docker compose -f docker-compose.dev.yml up --build
```

Abrir http://localhost:3000, iniciar sesión y probar los endpoints desde Scalar o desde la propia interfaz.

### 12.2 Release

```bash
docker compose -f docker-compose.release.yml up -d
```

Abrir http://localhost:5001, iniciar sesión y repetir la consulta en los otros puertos.

### 12.3 Pruebas de autorización

```bash
# sin cookie: 401
curl -i http://localhost:5001/api/peliculas

# sin cookie: 401
curl -i http://localhost:5001/api/reportes/catalogo-abraham

# estado público
curl -s http://localhost:5001/api/auth/me

# proveedores configurados
curl -s http://localhost:5001/api/auth/providers
```

Para el `403` de Marco: iniciar sesión con Auth0 en el puerto 5001 y consultar `http://localhost:5002/api/reportes/comunidad-marco` desde el mismo navegador. La cookie se comparte entre puertos del mismo host, pero la política rechaza el proveedor `Auth0`.

### 12.4 Pruebas de persistencia

```bash
# el volumen de PostgreSQL sobrevive a un down/up
docker compose -f docker-compose.release.yml down
docker compose -f docker-compose.release.yml up -d

# las 4 instancias ven el mismo anillo de claves
docker compose -f docker-compose.release.yml exec api-2 ls -l /app/keys
```

### 12.5 Validación del Compose de release

```bash
docker compose -f docker-compose.release.yml config --quiet
docker compose -f docker-compose.release.yml config --services
# db
# api-1
# api-2
# api-3
# api-4
```

---

## 13. Cobertura de los 8 puntos de la consigna

| # | Punto | Implementación / evidencia |
|---|---|---|
| 1 | Repositorio público y trabajo colaborativo | Repositorio GitHub, ramas por aporte, Pull Requests #2–#8 y reglas en `CONTRIBUTING.md`. |
| 2 | API REST .NET con PostgreSQL, CRUD y borrado lógico | `CatalogoApi/` en .NET 10, EF Core 10 + Npgsql, 12 tablas y migración `InitialPostgres`; 12 controladores CRUD con filtros `activo` y soft delete; Scalar/OpenAPI. |
| 3 | Autenticación OAuth 2.0 / SSO | Auth0 OIDC, TikTok Login Kit v2 y 6 proveedores OAuth adicionales; cookie `CatalogoApi.Auth` y `AuthController`. |
| 4 | Reportes autorizados por integrante | `catalogo-abraham`, `comunidad-marco` y `talento-emanuel`; 3 políticas con proveedores y listas de usuarios configurables; `401`/`403` demostrables. |
| 5 | Dockerfiles individuales | `Dockerfile.AbrahamValeriano`, `Dockerfile.MarcoArias` y `Dockerfile.EmanuelKilibarda`; este último genera la imagen de Docker Hub. |
| 6 | Docker Compose de desarrollo | `docker-compose.dev.yml` con PostgreSQL 17, backend .NET 10 y frontend Nginx en `3000`/`5174`/`5433`. |
| 7 | Docker Compose release, 4 instancias — **Punto 7, 20 pts** | Imagen `emanuelxd/proyecto-final:1.0.0`, sin `build`, `api-1`–`api-4` en `5001`–`5004`, PostgreSQL compartido, `dpkeys_release:/app/keys`, CORS y redirección por instancia. |
| 8 | Despliegue público y documentación | Render en `https://proyecto-finalupds.onrender.com`; este `README.md`, `.env.example`, tabla de puertos, variables, endpoints, políticas y comandos reproducibles. |

---

## 14. Estructura del repositorio

```text
.
├── CatalogoApi/
│   ├── Authentication/
│   │   ├── TikTokAuthenticationExtensions.cs
│   │   ├── TikTokHandler.cs
│   │   └── TikTokOptions.cs
│   ├── Controllers/
│   │   ├── ActoresController.cs
│   │   ├── AuthController.cs
│   │   ├── CategoriasController.cs
│   │   ├── DirectoresController.cs
│   │   ├── FavoritosController.cs
│   │   ├── ListasReproduccionController.cs
│   │   ├── PeliculasController.cs
│   │   ├── PlataformasController.cs
│   │   ├── PremiosController.cs
│   │   ├── ProductorasController.cs
│   │   ├── ReportesController.cs
│   │   ├── ResenasController.cs
│   │   ├── SeriesController.cs
│   │   └── TemporadasController.cs
│   ├── Data/AppDbContext.cs
│   ├── Dtos/
│   ├── Migrations/
│   ├── Models/
│   ├── Properties/launchSettings.json
│   ├── wwwroot/
│   │   ├── index.html
│   │   ├── privacy.html
│   │   ├── terms.html
│   │   └── tiktokCUvaprQ4nvxYRoqXnsZEBYWRZTEY5dGo.txt
│   ├── CatalogoApi.csproj
│   ├── CatalogoApi.http
│   ├── Program.cs
│   ├── appsettings.json
│   └── appsettings.Example.json
├── frontend/config.js
├── docker-compose.dev.yml
├── docker-compose.release.yml
├── Dockerfile
├── Dockerfile.AbrahamValeriano
├── Dockerfile.MarcoArias
├── Dockerfile.EmanuelKilibarda
├── .dockerignore
├── .env.example
├── .gitignore
├── CONTRIBUTING.md
└── README.md
```

---

## 15. Seguridad y notas de operación

- **Secretos:** solo en `.env` o en el panel del proveedor. `.env` está en `.gitignore`; `.env.example` no contiene secretos reales.
- **Cookie:** `CatalogoApi.Auth`, `HttpOnly=true`, `SameSite=Lax`; en Render viaja por HTTPS. En local sobre HTTP se usa el mismo esquema de la petición.
- **Autorización:** los reportes no dependen del frontend; la validación se hace en el backend con políticas. Un `403` no se puede evitar desde la interfaz.
- **Contenedores:** la imagen final corre con `USER app`, no como root, y escribe las claves en `/app/keys`.
- **Release:** no compila código ni publica PostgreSQL al host; las 4 API se comunican por la red interna `catalogo_release_net`.
- **Callbacks:** cada proveedor requiere registrar las URLs exactas de desarrollo, los 4 puertos release y Render. Un callback no registrado produce error del proveedor antes de llegar a la API.
- **Migraciones:** se ejecutan al arrancar con reintentos. En un entorno con réplicas, la primera aplicación de esquema debe observarse en los logs; una vez creada la tabla de historial, las demás instancias no vuelven a aplicarla.
- **Imagen inmutable:** si se cambia el código, reconstruir y publicar una nueva etiqueta; no reutilizar `1.0.0` para contenido distinto.

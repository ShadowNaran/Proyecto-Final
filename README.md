Catálogo Audiovisual API - Proyecto Final Tecnología Web 1

API REST en ASP.NET Core (.NET 10) sobre PostgreSQL 17 con autenticación OAuth 2.0 / OpenID Connect, reportes autorizados por políticas y despliegue en contenedores.

Repositorio GitHub: https://github.com/emanuelkXD/Proyecto-Final
Docker Hub: https://hub.docker.com/r/emanuelxd/proyecto-final (emanuelxd/proyecto-final:1.0.0)
Despliegue público (Render): https://proyecto-finalupds.onrender.com
Documentación API (Scalar): https://proyecto-finalupds.onrender.com/scalar/v1


1. Cómo ejecutar el proyecto

Paso previo: Variables de entorno
Copia la plantilla y completa las credenciales de base de datos y proveedores OAuth:

cp .env.example .env

Opción A: Entorno de desarrollo (Frontend + Backend + PostgreSQL)
Inicia toda la aplicación con un solo comando:

docker compose -f docker-compose.dev.yml up --build -d

Frontend: http://localhost:3000
API / Scalar: http://localhost:5174 y http://localhost:5174/scalar/v1

Opción B: Entorno Release con 4 instancias (sin compilar código)
Descarga la imagen emanuelxd/proyecto-final:1.0.0 desde Docker Hub y levanta cuatro réplicas de la API compartiendo base de datos y claves de sesión (dpkeys_release:/app/keys):

docker compose -f docker-compose.release.yml up -d
docker compose -f docker-compose.release.yml ps

Instancias activas: http://localhost:5001, http://localhost:5002, http://localhost:5003 y http://localhost:5004.

Opción C: Ejecución local directa con .NET CLI
Para ejecutar el backend sin Docker (requiere PostgreSQL activo y DB_CONNECTION_STRING configurado en .env o appsettings.Development.json):

dotnet restore CatalogoApi/CatalogoApi.csproj
dotnet run --project CatalogoApi/CatalogoApi.csproj

Preparación de la base de datos
Al arrancar, la API ejecuta db.Database.Migrate() automáticamente con reintentos para aplicar la migración inicial (20261123214251_InitialPostgres) y crear las 12 tablas. Para aplicarla manualmente fuera de Docker:

dotnet ef database update --project CatalogoApi/CatalogoApi.csproj


2. Puertos

Puerto Host | Servicio | Puerto Interno | Entorno
3000 | Frontend (Nginx) | 80 | Desarrollo
5174 | Backend API + Scalar | 8080 | Desarrollo
5433 | PostgreSQL 17 | 5432 | Desarrollo
5001 | api-1 (Release) | 8080 | Release
5002 | api-2 (Release) | 8080 | Release
5003 | api-3 (Release) | 8080 | Release
5004 | api-4 (Release) | 8080 | Release


3. Autenticación y pruebas de acceso

Auth0 (SSO): Opera tanto en local (3000, 5174, 5001-5004) como en Render. Registra las URLs /signin-auth0 en Allowed Callback URLs de Auth0.
TikTok Login Kit: Exige dominio HTTPS verificado; opera en https://proyecto-finalupds.onrender.com/signin-tiktok.
Sesión compartida en Release: Las cuatro instancias montan el mismo volumen en /app/keys. Al iniciar sesión en http://localhost:5001, la cookie CatalogoApi.Auth sigue siendo válida en 5002, 5003 y 5004 (usar siempre localhost sin mezclar con 127.0.0.1).

Verificación rápida por consola:

# Sin sesión devuelve 401 Unauthorized
curl -i http://localhost:5001/api/reportes/talento-emanuel

# Comprobar respuesta en las 4 instancias
for p in 5001 5002 5003 5004; do
  echo -n "Puerto $p -> "
  curl -s "http://localhost:$p/api/auth/me"
  echo
done


4. Reparto de responsabilidades, reportes y Dockerfiles

Cada integrante desarrolló su módulo CRUD, un reporte protegido en el backend y un Dockerfile multi-etapa en .NET 10 (USER app, puerto interno 8080).

Integrante: Abraham Valeriano
Reporte y Endpoint: GET /api/reportes/catalogo-abraham - Consolidado del catálogo activo por categoría.
Tablas combinadas: categorias, peliculas, series, temporadas
Política y acceso: ReporteAbrahamPolicy (Auth0, Discord, Spotify, TikTok)
Dockerfile: Dockerfile.AbrahamValeriano (usado en Compose dev)

Integrante: Marco Antonio Arias Giron
Reporte y Endpoint: GET /api/reportes/comunidad-marco - Métricas de reseñas, favoritos, premios y listas.
Tablas combinadas: resenas, listas_reproduccion, favoritos, premios
Política y acceso: ReporteMarcoPolicy (Dropbox, Slack - devuelve 403 con Auth0)
Dockerfile: Dockerfile.MarcoArias

Integrante: Emanuel Rafael Kilibarda Rocha
Reporte y Endpoint: GET /api/reportes/talento-emanuel - Talento, productoras por país y plataformas activas.
Tablas combinadas: directores, actores, productoras, plataformas
Política y acceso: ReporteEmanuelPolicy (Auth0, TikTok)
Dockerfile: Dockerfile.EmanuelKilibarda (imagen final en Docker Hub)

Imagen final en Docker Hub: Dockerfile.EmanuelKilibarda compila CatalogoApi.csproj en modo Release, configura /app/keys para Data Protection y genera emanuelxd/proyecto-final:1.0.0.

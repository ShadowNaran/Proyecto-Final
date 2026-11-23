Tarea 6 - API de Catalogo Audiovisual (.NET)

Proyecto grupal en .NET para gestionar peliculas, series, produccion, plataformas de streaming y reseñas. Implementa inicio de sesion con proveedores externos, controladores protegidos y borrado logico (soft delete) en base de datos.

Integrantes y reparto

Todo el equipo trabaja desde sus propios forks y envia los aportes por Pull Request. Ningun integrante esta agregado como colaborador directo en este repositorio.

Emanuel Kilibarda (Lider)
- Tablas: directores, actores, productoras, plataformas
- Proveedores: Twitch y X

Abraham Valeriano
- Tablas: categorias, peliculas, series, temporadas
- Proveedores: Discord y Spotify

Marco Antonio Arias Giron
- Tablas: resenas, listas_reproduccion, favoritos, premios
- Proveedores: Dropbox y Slack

Esquema de tablas

Todas las tablas incluyen id, activo, creado_en y eliminado_en para controlar el soft delete.

Produccion y distribucion (Emanuel):
- directores: nombre, nacionalidad, fecha_nacimiento, biografia
- actores: nombre, nacionalidad, fecha_nacimiento, biografia
- productoras: nombre (unique), pais, anio_fundacion, sitio_web
- plataformas: nombre (unique), sitio_web, precio_mensual, pais_disponible

Nucleo del catalogo (Abraham):
- categorias: nombre (unique), descripcion
- peliculas: titulo, sinopsis, anio_estreno, duracion_min, clasificacion, poster_url, categoria_id (FK opcional a categorias)
- series: titulo, sinopsis, anio_inicio, en_emision, poster_url, categoria_id (FK opcional a categorias)
- temporadas: numero, titulo, anio_estreno, cantidad_episodios, serie_id (FK a series)

Comunidad y reconocimiento (Marco):
- resenas: autor, tipo_contenido (pelicula/serie), titulo_obra, puntuacion (1-5), comentario
- listas_reproduccion: nombre, descripcion, es_publica, propietario
- favoritos: usuario_email, tipo_contenido, titulo_obra, nota_personal
- premios: nombre, categoria_premio, anio, obra_ganadora

---

## Autenticación OAuth 2.0 (Twitch y X / Twitter)

El sistema implementa autenticación externa delegada mediante el protocolo OAuth 2.0:

1. **Inicio de autorización**: El cliente hace clic en el botón de login correspondiente (`/api/auth/login/twitch` o `/api/auth/login/twitter`). El controlador `AuthController` emite un desafío (`Challenge`) especificando el esquema y la URL de redirección final (`/`).
2. **Redirección al proveedor**: El middleware de ASP.NET Core redirige al usuario hacia el servidor de autorización del proveedor:
   - **Twitch**: `https://id.twitch.tv/oauth2/authorize` solicitando el scope de lectura básico.
   - **X (Twitter)**: `https://twitter.com/i/oauth2/authorize` bajo OAuth 2.0 con PKCE (Proof Key for Code Exchange) y scope `users.read`.
3. **Consentimiento del usuario**: El usuario concede permisos en la interfaz del proveedor.
4. **Callback y canje de código**: El proveedor redirige al navegador de vuelta a la aplicación local mediante la ruta registrada:
   - `/signin-twitch` para Twitch.
   - `/signin-twitter` para X.
   El middleware intercepta la petición, valida los parámetros de seguridad (incluido el estado y el verificador PKCE en X) e intercambia el código de autorización (`code`) por un token de acceso (`access_token`) mediante una comunicación directa back-channel.
5. **Consulta de perfil y sesión de usuario**:
   - Con el token obtenido, se consulta el endpoint de información del usuario (`https://api.twitch.tv/helix/users/` en Twitch y `https://api.twitter.com/2/users/me` en X).
   - Se extraen los claims del usuario (`NameIdentifier`, `Name`, `provider`).
   - Se emite una cookie segura de sesión (`CookieAuthenticationDefaults.AuthenticationScheme`) con `SameSite=Lax` y `HttpOnly=true`.
6. **Consulta y cierre de sesión**:
   - `GET /api/auth/me`: Retorna el estado `{ autenticado, proveedor, usuario, id }` para la interfaz.
   - `POST /api/auth/logout`: Cierra la sesión mediante `SignOutAsync` eliminando la cookie de autenticación.

---

## Análisis Técnico: Flujo OAuth en Meta (Facebook / Instagram)

### App Review y Niveles de Acceso (Standard Access vs. Advanced Access)

A diferencia de proveedores como Twitch o X —donde las aplicaciones pueden leer perfiles públicos básicos en modo desarrollo o producción sin auditorías previas—, el ecosistema de Meta (Meta for Developers) implementa un modelo de gobernanza estricto:

1. **Modo Desarrollo vs. Modo Producción**:
   - En **Modo Desarrollo (Development Mode)**, únicamente los usuarios agregados manualmente con roles en la app de Meta (Administradores, Desarrolladores o Evaluadores) pueden iniciar sesión con éxito. Cualquier usuario externo que intente iniciar sesión recibirá un mensaje de error indicando que la aplicación no está disponible.
   - Para abrir la aplicación al público general, debe cambiarse a **Modo Producción (Live Mode)**.

2. **Standard Access vs. Advanced Access**:
   - **Standard Access**: Es el nivel asignado por defecto a los permisos de una aplicación. Permite realizar llamadas al Graph API y solicitar permisos únicamente respecto a usuarios que tienen un rol explícito en la app. No permite autenticar usuarios finales del público general.
   - **Advanced Access**: Es indispensable para que cualquier usuario del público pueda autenticarse y conceder permisos a la aplicación en producción. Permisos esenciales como `public_profile`, `email` o accesos a páginas de negocio requieren solicitar Advanced Access.

3. **Proceso de App Review (Revisión de Aplicaciones)**:
   - Para obtener **Advanced Access**, la aplicación debe someterse a una revisión formal por parte del equipo de Meta.
   - Requiere proporcionar una justificación detallada de cada permiso solicitado, un video explicativo demostrando la experiencia de usuario y el uso de los datos, el cumplimiento estricto de las Políticas de la Plataforma de Meta y, en muchos casos, la **Verificación de Negocio (Business Verification)** aportando documentación legal y fiscal de la empresa.

4. **Comparativa**:
   - Mientras en Twitch y X basta con registrar el `ClientId`, `ClientSecret` y las URLs de retorno (`Callback URLs`) para admitir inicios de sesión globales inmediatos, en Meta la autenticación de usuarios externos depende del cumplimiento de la auditoría de App Review y la habilitación de Advanced Access.

---

---

## Entorno de Desarrollo con Docker Compose (Abraham Valeriano)

El repositorio incluye un entorno containerizado completo con **PostgreSQL 17**, el **backend .NET 10** y el **frontend servido con Nginx**:

- **`Dockerfile.AbrahamValeriano`**: Imagen multi-stage basada en `mcr.microsoft.com/dotnet/sdk:10.0` y `mcr.microsoft.com/dotnet/aspnet:10.0` que compila/publica `CatalogoApi` en modo `Release`, prepara `/app/keys` para Data Protection persistente con el usuario `app` y expone el puerto interno `8080`.
- **Migraciones automáticas**: Al iniciar, `CatalogoApi` ejecuta `db.Database.Migrate()` con reintentos contra PostgreSQL para aplicar automáticamente el esquema inicial (`InitialPostgres`) sin intervención manual.
- **Levantar el entorno completo**:
  1. Copia `.env.example` a `.env` en la raíz del proyecto y completa las credenciales OAuth que necesites probar.
  2. Ejecuta:
     ```bash
     docker compose -f docker-compose.dev.yml up --build
     ```
  3. Servicios disponibles por defecto:
     - **Frontend (Nginx)**: `http://localhost:3000`
     - **Backend API y Scalar**: `http://localhost:5174` / `http://localhost:5174/scalar/v1`
     - **PostgreSQL**: `localhost:5433` (volumen persistente `pgdata`, red interna `catalogo_net`)

---

Flujo de trabajo y ejecucion

Las reglas para hacer fork, nombrar ramas, escribir commits y abrir Pull Requests estan detalladas en CONTRIBUTING.md.

Las credenciales de OAuth, URLs de retorno y cadenas de conexion se configuran mediante variables de entorno locales. Nunca se suben secretos ni claves reales al repositorio.

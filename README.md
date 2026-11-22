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

Flujo de trabajo y ejecucion

Las reglas para hacer fork, nombrar ramas, escribir commits y abrir Pull Requests estan detalladas en CONTRIBUTING.md.

Las credenciales de OAuth, URLs de retorno y cadenas de conexion se configuran mediante variables de entorno locales. Nunca se suben secretos ni claves reales al repositorio.

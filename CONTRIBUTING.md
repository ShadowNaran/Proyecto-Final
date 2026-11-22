Guia de contribucion

1. Fork y clonado

Haz fork de este repositorio hacia tu cuenta de GitHub y clona tu copia en local:

git clone https://github.com/TU_USUARIO/Tarea-6.git
cd Tarea-6
git remote add upstream https://github.com/emanuelkXD/Tarea-6.git

2. Nomenclatura de ramas y commits

Crea una rama nueva por cada etapa desde la rama principal actualizada:
- feat/tablas-nombre
- feat/login-nombre
- feat/endpoints-nombre
- test/conflicto-post-nombre
- feat/put-softdelete-nombre

Usa mensajes de commit cortos y claros sobre lo que cambia en cada paso, por ejemplo "agregar modelos y migraciones de produccion" o "implementar soft delete en controladores".

3. Pruebas antes de subir

Configura tus variables de entorno locales sin guardar claves en archivos rastreados por git. Compila el proyecto con dotnet build, ejecuta las migraciones y verifica en local que los endpoints, el login y los filtros de registros activos funcionen bien.

4. Pull Requests y revision

Sube tu rama a tu fork con git push origin nombre-de-rama y abre una Pull Request apuntando a la rama principal del repositorio base. Indica en la descripcion que etapa entregas y pide revision.

Cuando el lider abra sus propias Pull Requests, otro integrante del equipo debe revisarlas y aprobarlas antes de que el lider haga la integracion.

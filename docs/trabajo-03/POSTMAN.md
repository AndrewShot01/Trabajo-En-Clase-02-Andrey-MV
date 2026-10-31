# Trabajo en clase 3 API del parqueo y Postman

La API es el programa que recibe solicitudes HTTP y devuelve datos JSON. Postman es el cliente desde el que envía esas solicitudes. No necesita programar dentro de Postman ni conectarlo directamente a PostgreSQL: Postman llama a la API, y la API utiliza los repositorios del trabajo 2.

## 1 Iniciar la API

Abra una terminal en la carpeta del repositorio y ejecute:

```powershell
dotnet restore TrabajoEnClase02.slnx
dotnet run --project Parqueo.Api
```

Espere `Now listening on: http://localhost:5080`. Mantenga esa terminal abierta mientras use Postman. Para detener la API, presione Ctrl+C. La aplicación de consola anterior continúa disponible con `dotnet run --project Parqueo`; ese comando no inicia un servidor HTTP.

Esta API está preparada para la práctica local en su equipo, sin autenticación. Escucha en localhost; no la publique directamente en Internet.

## 2 Primera solicitud desde Postman

Complete la bienvenida de Postman si aparece. Cree una solicitud HTTP con **New > HTTP Request** (el nombre de los botones puede variar según la versión).

1. Seleccione el método **GET**.
2. Escriba `http://localhost:5080/api/ingresos/1` en la barra de URL.
3. Abra **Headers** y agregue `X-Fuente-Datos` con valor `json`.
4. Pulse **Send**.
5. Debe ver **200 OK** y el registro con `automovil`, `parqueo`, `duracionMinutos`, `duracionHoras` y `montoTotal`.
6. Cambie la cabecera a `postgres` y pulse **Send** de nuevo: la misma ruta consultará su base real del esquema `user_07`.

El ingreso 1 devuelve 45 minutos, 0.75 horas y monto 675. El ingreso 14 tiene salida y cálculos nulos. Un GET puede abrirse también en el navegador, pero un navegador normal no permite cambiar fácilmente las cabeceras ni hacer POST, PUT o DELETE.

## 3 Importar la colección preparada

En Postman seleccione **Import** y el archivo `postman/Trabajo-03-Parqueo.postman_collection.json` del repositorio. Si no encuentra Import, pruebe Ctrl+O o búsquelo desde Collections.

La colección se llama **Trabajo en clase 3 — API Parqueo** y contiene las carpetas **JSON** y **POSTGRES**, cada una con 47 solicitudes. La variable `baseUrl` vale `http://localhost:5080`. Las cabeceras, parámetros, cuerpos JSON y pruebas ya están preparados. No contiene contraseñas ni cadenas de conexión.

Abra primero la carpeta JSON y una de sus solicitudes GET. Pulse Send y observe la respuesta y la pestaña de resultados de pruebas. Después pruebe POSTGRES. Si usa **Run collection / Runner**, mantenga el orden y ejecute una sola iteración para que las operaciones CRUD creen, modifiquen y retiren sus registros de prueba. El primer paso genera una clave temporal. Las precondiciones detienen la ejecución si esa clave ya está ocupada.

No ejecute solicitudes de modificación aisladas con claves de datos reales. Los ejemplos de la colección trabajan con registros temporales y los eliminan al terminar; no ejecutan DELETE sobre los registros iniciales. Si interrumpe el Runner después de crear registros, pueden quedar pendientes de retirar: conserve la variable `clavePrueba` y use las solicitudes de borrado en el orden ingreso, automóvil y parqueo.

## 4 Rutas GET

Todas las rutas empiezan con `http://localhost:5080/api/`. Las consultas por texto son subcadenas, sin distinguir mayúsculas. Los intervalos incluyen ambos límites. Use Params en Postman para escribir tildes y fechas sin codificar manualmente la URL.

| Entidad | Ruta | Parámetros o ejemplo |
| --- | --- | --- |
| Automóviles | `automoviles` | Todos |
| Automóviles | `automoviles/{id}` | `automoviles/1` |
| Automóviles | `automoviles/por-color` | `texto=roj` |
| Automóviles | `automoviles/por-anios` | `desde=2020`, `hasta=2022` |
| Automóviles | `automoviles/por-fabricante` | `texto=yot` |
| Automóviles | `automoviles/por-tipo` | `texto=4x` |
| Parqueos | `parqueos` | Todos |
| Parqueos | `parqueos/{id}` | `parqueos/1` |
| Parqueos | `parqueos/por-provincia` | `texto=José` |
| Parqueos | `parqueos/por-nombre` | `texto=flor` |
| Parqueos | `parqueos/por-precios` | `desde=900`, `hasta=1200` |
| Ingresos | `ingresos` | Todos |
| Ingresos | `ingresos/{id}` | `ingresos/1` o `ingresos/14` |
| Ingresos | `ingresos/por-tipo` | `texto=sed`, `desde=2026-10-01T00:00:00`, `hasta=2026-10-16T00:00:00` |
| Ingresos | `ingresos/por-provincia` | `texto=here`, mismas fechas |
| Ingresos | `ingresos/precio-por-hora/{idParqueo}` | `ingresos/precio-por-hora/1` |

Las respuestas de automóviles y parqueos incluyen su colección `ingresos`. Las respuestas de ingresos incluyen los objetos `automovil` y `parqueo`. Se llenan las propiedades de navegación y se proyectan a respuestas sin referencias circulares. Los campos calculados se exponen explícitamente, aunque estén excluidos de los archivos JSON persistidos del trabajo 2.

## 5 POST PUT y DELETE manualmente

Use preferiblemente los ejemplos de la colección. Para hacerlo manualmente, primero seleccione la fuente mediante `X-Fuente-Datos`. En POST y PUT seleccione **Body > raw > JSON**. Esto debe agregar `Content-Type: application/json`.

Cree un automóvil con **POST** a `/api/automoviles`:

```json
{"id":190000003,"color":"Verde","anio":2023,"fabricante":"Prueba API TC3","tipo":"moto"}
```

Cree un parqueo con **POST** a `/api/parqueos`:

```json
{"id":190000003,"provincia":"Cartago","nombre":"Prueba API TC3","precioPorHora":600}
```

Cree un ingreso con **POST** a `/api/ingresos`:

```json
{"consecutivo":190000003,"idParqueo":190000003,"idAutomovil":190000003,"entrada":"2026-10-20T08:00:00","salida":null}
```

Compruebe antes con GET que las claves del ejemplo están libres: debe obtener 404. Un POST correcto devuelve **201 Created** y una cabecera Location para consultar el recurso creado. Una clave ocupada devuelve 409 y no debe reutilizarse para estas pruebas.

Actualice el ingreso con **PUT** a `/api/ingresos/190000003`, usando el mismo cuerpo completo y `"salida":"2026-10-20T10:00:00"`. Debe recibir **204 No Content**. Consulte el ingreso con GET: a tarifa 600, el monto de dos horas será 1200. PUT de automóviles y parqueos sigue el mismo patrón, con el identificador en la URL y el cuerpo completo.

Borre con **DELETE**, sin Body, en este orden:

1. `/api/ingresos/190000003`.
2. `/api/automoviles/190000003`.
3. `/api/parqueos/190000003`.

Cada eliminación correcta devuelve 204. Un GET posterior devuelve 404. No se permite borrar un automóvil ni un parqueo si todavía tienen ingresos relacionados.

## 6 Fuentes y configuración

Por defecto se utiliza JSON. Puede elegir `json` o `postgres` por solicitud mediante `X-Fuente-Datos`; la API devuelve esa misma cabecera para comprobar la fuente efectiva. La ruta por defecto de los JSON de trabajo es `Parqueo.Api/.datos-api`, ignorada por Git. Se inicializa una sola vez desde los datos originales y no los modifica. Puede configurar `Parqueo__RutaJson` o `Parqueo__Fuente` mediante variables de entorno.

PostgreSQL usa el secreto `ConnectionStrings:Parqueo` ya configurado localmente en el trabajo 2, con conexión TLS a `advanced_programming` y esquema `user_07`. No ingrese credenciales en Postman ni en appsettings. La API no crea tablas ni ejecuta migraciones al iniciarse.

## 7 Códigos y problemas frecuentes

- **200**: consulta correcta; una lista vacía es válida.
- **201**: recurso creado.
- **204**: actualización o eliminación correcta; no hay cuerpo de respuesta.
- **400**: JSON, parámetros, fechas o datos inválidos; la clave del cuerpo debe coincidir con la URL.
- **404**: recurso inexistente.
- **409**: clave duplicada o eliminación incompatible con las relaciones.
- **503**: no se pudo acceder a la fuente de datos.

**ECONNREFUSED**: inicie `dotnet run --project Parqueo.Api` y conserve la terminal abierta. **404 en una ruta**: compruebe `/api/` y los nombres de la tabla de rutas. **503 en POSTGRES**: revise conexión, secreto y acceso a Azure. **415**: el cuerpo debe ser raw JSON con Content-Type application/json. Las fechas representan hora local y se envían sin sufijo Z.

## 8 Evidencia y pruebas reproducibles

`docs/trabajo-03/verificacion-http.txt` registra 94 solicitudes HTTP verificadas (47 por fuente) con sus códigos esperados; se comprobó que los datos originales permanecieron intactos. Esas pruebas se ejecutaron por HTTP desde un script, no se presentan como capturas de Postman.

La colección preparada incorpora pruebas de códigos, fuente, conteos, navegación y cálculos. La evidencia del Runner de Postman se registra por separado cuando se ejecuta dentro de la aplicación. Para el profesor, guarde capturas de GET de las tres entidades con sus relaciones, ingreso abierto, POST 201, PUT 204 y DELETE 204, o los resultados del Runner. No incluya secretos en las capturas.

Para repetir la prueba HTTP automatizada se necesita Python 3.12 o superior:

```powershell
python scripts/probar-api.py
```

El script usa la biblioteca estándar y no requiere paquetes adicionales. Mantenga la API encendida. Las pruebas comparan los datos antes y después del CRUD temporal.

Referencias: [API con controladores de ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/web-api/) y [Collection Runner de Postman](https://learning.postman.com/docs/tests-and-scripts/running-collections/intro-to-collection-runs).

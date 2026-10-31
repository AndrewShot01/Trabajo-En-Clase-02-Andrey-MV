# Trabajo en clase 04 Web MVC del parqueo

La solución conserva la consola del trabajo 2 y la API del trabajo 3. Este trabajo agrega **Parqueo.Mvc**, una web ASP.NET Core MVC en C# con vistas Razor, formularios de mantenimiento y una consulta maestro/detalle con JavaScript. La web consume la API por HTTP; no abre archivos de datos ni se conecta directamente a PostgreSQL.

## Ejecutar y abrir la web

Desde la raíz del repositorio, restaure y compile:

```powershell
dotnet restore TrabajoEnClase02.slnx
dotnet build TrabajoEnClase02.slnx
```

Abra dos terminales en esa misma carpeta. En la primera:

```powershell
dotnet run --project Parqueo.Api
```

En la segunda:

```powershell
dotnet run --project Parqueo.Mvc
```

Abra **http://localhost:5090** en el navegador. La API escucha en **http://localhost:5080**. Mantenga las dos terminales abiertas; para detener cada aplicación presione Ctrl+C. Si la API ya está encendida por el trabajo 3, puede usar esa instancia, siempre que se haya recompilado con los cambios del trabajo 4. Detenga las aplicaciones antes de compilar de nuevo en Windows, para evitar bloqueos de archivos DLL.

No necesita Postman para utilizar esta web. Postman sigue siendo útil para revisar directamente los endpoints de la API.

## Mantenimiento de automóviles

La página **Automóviles** muestra el filtro Desde el año / Hasta el año. Los límites son inclusivos. Con los datos iniciales, 2020 a 2022 devuelve Toyota 2020 y Suzuki 2022.

- **Nuevo automóvil**: complete identificador, año, fabricante, color y tipo. El identificador sugerido es el siguiente al máximo existente, pero se valida su unicidad al guardar. Puede modificarlo antes de crear el registro.
- **Detalles**: muestra la ficha y los ingresos relacionados del automóvil.
- **Editar**: permite cambiar los datos manteniendo el identificador original.
- **Eliminar**: abre una confirmación antes del borrado. Los automóviles con ingresos relacionados están protegidos: la web muestra el motivo y la API rechaza el borrado aunque se intente forzar el formulario.

Los mensajes de confirmación aparecen después de crear, editar y eliminar. Los errores se muestran sin perder los datos ingresados. Los formularios POST utilizan antifalsificación; se validan también las fechas, campos y claves en el servidor. El mantenimiento es de automóviles y la consulta de ingresos sigue el detalle concreto del enunciado. El CRUD de ingresos continúa disponible en la API del trabajo 3.

## Maestro y detalle con JavaScript

Entre en **Estadías por tipo**, elija Sedán, 4x4 o Moto y pulse Consultar vehículos. En el listado, seleccione **Ver estadías** para cargar el panel inferior sin recargar la página completa.

El panel incluye el identificador y nombre del parqueo, fecha/hora de entrada y salida, minutos, horas y monto en colones. Por ejemplo, Nissan #4 muestra 3 ingresos; uno tiene 180 minutos, 3 horas y ₡3.600, otro 405 minutos, 6,75 horas y ₡6.075, y el tercero permanece abierto. En ese caso se muestra En el parqueo, guiones para las duraciones y Pendiente de salida para el monto. Los valores reales de la API permanecen nulos, no se sustituyen por cero.

El JavaScript usa fetch contra el controlador MVC, que a su vez llama a la API. Maneja carga, ausencia de ingresos y errores; cancela solicitudes anteriores al seleccionar rápidamente otro automóvil. Los textos se insertan con textContent para no interpretar datos como HTML.

## Configuración de fuentes en appsettings

En `Parqueo.Mvc/appsettings.json`:

```json
"ParqueoApi": {
  "BaseUrl": "http://localhost:5080/",
  "FuenteMantenimiento": "json",
  "FuenteConsultas": "postgres"
}
```

Esta configuración permite que unas consultas utilicen JSON y otras PostgreSQL, tal como pide el profesor. La página indica la fuente efectiva. Para intercambiarlas, cambie los valores a `postgres` y `json`, respectivamente, y reinicie solo el MVC. Se recomienda cambiar fuentes cuando no haya formularios abiertos. No escriba contraseñas en este archivo.

El cliente agrega la cabecera X-Fuente-Datos en cada llamada. La API elige los repositorios existentes y utiliza la conexión local de Secret Manager del trabajo 2 para el esquema user_07. JSON trabaja en la copia persistente `.datos-api` del proyecto API. No se modificó el modelo de tablas ni se ejecutaron scripts de creación en esta tarea.

## Rutas y arquitectura

| Página o endpoint | Función |
| --- | --- |
| `GET /Automoviles` o `/` | Filtrar por años; parámetros Desde y Hasta |
| `GET/POST /Automoviles/Crear` | Formulario e inserción por API |
| `GET /Automoviles/Detalles/{id}` | Ficha e ingresos relacionados |
| `GET/POST /Automoviles/Editar/{id}` | Formulario y actualización por API |
| `GET/POST /Automoviles/Eliminar/{id}` | Confirmación y borrado por API |
| `GET /Consultas?tipo=sedán` | Maestro de automóviles por tipo |
| `GET /Consultas/Ingresos/{id}` | JSON para el panel JavaScript |
| API: `GET /api/ingresos/por-automovil/{id}` | Consulta nueva, filtrada en el repositorio |

`Parqueo.Mvc/Services/ParqueoApi.cs` centraliza las llamadas HTTP y la fuente configurada. Los controladores MVC manejan los formularios, las vistas y los mensajes. `wwwroot/js/maestro-detalle.js` actualiza el panel. La cadena de acceso es **vista → controlador MVC → cliente HTTP → controlador API → repositorio → JSON/EF Core**.

## Verificación ejecutada

- **74 comprobaciones MVC por HTTP**, con CRUD de automóviles, filtros, modelos inválidos, claves duplicadas, antifalsificación, protección de relaciones y ambas fuentes.
- **2 comprobaciones de indisponibilidad**, deteniendo temporalmente la API: mensaje comprensible en la lista y HTTP 503 para el panel.
- **94 solicitudes de regresión de la API** del trabajo 3, repetidas sobre JSON y PostgreSQL.
- **37 comprobaciones locales de regresión** del trabajo 2.
- Revisión en Microsoft Edge del filtro por años, formulario de creación y carga del detalle con JavaScript. Capturas reales: automoviles.png, formulario.png y maestro-detalle.png.

Se comprobó que los datos originales de ambas fuentes permanecieron intactos después de retirar los vehículos temporales. La prueba MVC utiliza una segunda instancia solo para intercambiar las fuentes sin alterar la configuración de entrega.

Para repetir las pruebas automatizadas, inicie API y MVC como se indicó. En una tercera terminal inicie el MVC de prueba:

```powershell
dotnet run --project Parqueo.Mvc --no-build --no-launch-profile -- --urls http://localhost:5091 --ParqueoApi:FuenteMantenimiento=postgres --ParqueoApi:FuenteConsultas=json
```

En una cuarta terminal, con Python 3 y lxml instalados:

```powershell
python scripts/probar-mvc.py
```

El script verifica los puertos 5090 y 5091, utiliza claves temporales y retira únicamente los vehículos que creó. Las pruebas se diseñaron para los datos iniciales de la práctica; no son pruebas para una base de producción.

## Alcance

Aplicación académica local, sin autenticación ni despliegue público. Se mantiene el acceso limitado a localhost. No se cambió el repositorio remoto ni se incluyeron secretos. Los commits nuevos se identifican como Trabajo en clase 04.

Referencias: [MVC de ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview) y [antifalsificación en formularios MVC](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0).

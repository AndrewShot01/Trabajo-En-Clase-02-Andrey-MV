# Sistema de parqueo

Trabajo en Clase 02 de BSI-19 Programación Avanzada. Consola C# .NET 10, EF Core 10.0.13 y proveedor Npgsql 10.0.3. El motor confirmado por el estudiante es PostgreSQL en Azure, base `advanced_programming`, esquema `user_07`. El repositorio original contenía solamente `.gitignore`.

## Ejecutar

Desde la raíz, con SDK .NET 10:

```powershell
dotnet restore TrabajoEnClase02.slnx
dotnet build TrabajoEnClase02.slnx --no-restore
dotnet run --project Parqueo --no-build
dotnet run --project Parqueo --no-build -- --postgres
```

Sin opciones usa los JSON copiados junto al ejecutable; `--postgres` usa la conexión real de Secret Manager. Ambos modos listan ingresos de provincias que contienen `San`, con entrada, salida, minutos, horas y monto. `--sql` se conserva como alias de `--postgres`.

Para persistir modificaciones directamente en los datos fuente, use `--datos ./Parqueo/Datos`. Los archivos del directorio de compilación pueden reemplazarse al compilar.

## Estructura

- `diseño-bd.sql`: DDL PostgreSQL, claves, restricciones e índices.
- `insertar-registros.sql`: 5 automóviles, 2 parqueos y 15 ingresos, de los cuales 2 no tienen salida.
- `Parqueo/Datos`: los mismos datos en tres JSON.
- `Parqueo/Modelos/Generado`: entidades obtenidas mediante scaffold y adaptadas a nombres C# legibles.
- `Parqueo/Modelos/Entidades.cs`: interfaz y propiedades de clave en parciales.
- `Parqueo/Modelos/Ingreso.Calculos.cs`: propiedades calculadas que no se sobrescriben al regenerar.
- `Parqueo/ParqueoContext.cs`: contexto obtenido del esquema real `user_07`.
- `Parqueo/FabricaContexto.cs`: conexión Npgsql desde secretos locales.
- `Parqueo/Repositorios`: interfaces, consultas comunes y almacenes EF/JSON con validaciones.
- `Parqueo/OperacionesBd.cs`: inspección, inicialización transaccional y verificación SQL.
- `Parqueo/Pruebas.cs` y `PruebasPostgres.cs`: pruebas locales y contra PostgreSQL.
- `scripts`: scaffold, integración y comparación de datos.
- `sql/verificar.sql`: verificación de restricciones con rollback.
- `docs`: resultados y estado de la entrega académica.

## Modelo y reglas

Las tablas son `user_07."PRQAutomoviles"`, `user_07."PRQParqueos"` y `user_07."PRQIngresosAutomoviles"`. Los nombres entre comillas conservan mayúsculas en PostgreSQL. Cada ingreso referencia automóvil y parqueo. Las claves son enteros positivos asignados por el llamador, sin identidad automática. Se impide eliminar padres con ingresos relacionados. Los tipos válidos son `sedán`, `4x4` y `moto`.

Los intervalos de años, precios y fechas incluyen ambos límites. Se filtra por la fecha de entrada completa: el llamador debe incluir la hora final deseada. Las fechas usan `timestamp without time zone`, como hora local del parqueo. Las búsquedas textuales usan subcadenas sin distinguir mayúsculas; las pruebas verificaron también provincia con tilde. No hay búsqueda por distancia de edición.

La duración utiliza ticks y aritmética decimal. Horas = minutos / 60; monto = horas × tarifa actual del parqueo. Se redondea solo el monto final a dos decimales, con `MidpointRounding.AwayFromZero`. No se cobra por hora iniciada ni se guarda una tarifa histórica. Sin salida, minutos, horas y monto devuelven `null`. Los cálculos son propiedades no persistidas.

## Conexión y secretos

La conexión del estudiante ya se configuró localmente fuera del repositorio. En otro equipo configure el secreto con su cadena real:

```powershell
dotnet user-secrets set "ConnectionStrings:Parqueo" "<CADENA_REAL>" --project Parqueo
```

Formato de referencia, sin contraseña real:

```text
Host=<SERVIDOR>;Port=5432;Database=advanced_programming;Username=user_07;Password=<CONTRASEÑA>;SSL Mode=VerifyFull;Search Path=user_07
```

Se usa TLS con verificación del servidor. Secret Manager es almacenamiento local de desarrollo fuera de Git, no almacenamiento cifrado de producción. Nunca incluya credenciales en archivos versionados ni registre la cadena. Los comandos no imprimen el secreto.

## Scripts de la base

**Las tres tablas y los datos iniciales ya fueron creados en la base real con autorización del estudiante. No vuelva a ejecutar la creación para reiniciar datos.**

Puede abrir los SQL con la extensión PostgreSQL y confirmar que la conexión activa corresponde a `advanced_programming`, esquema `user_07`. En una instalación inicial sin estas tablas, ejecute primero `diseño-bd.sql` y luego `insertar-registros.sql`. Los scripts no crean ni modifican otros esquemas y no son migraciones idempotentes.

Alternativamente, desde la raíz:

```powershell
dotnet run --project Parqueo -- --inspeccionar-bd
dotnet run --project Parqueo -- --inicializar-bd
```

La inicialización ejecuta ambos scripts en una sola transacción y rechaza la operación si alguna tabla PRQ ya existe. No elimina tablas ni datos existentes.

## Scaffold e integración

El scaffold se ejecutó contra las tres tablas reales de `user_07`. Para repetirlo después de un cambio autorizado en el esquema:

```powershell
dotnet tool restore
powershell -NoProfile -ExecutionPolicy Bypass -File ./scripts/scaffold.ps1
# Revisar los cuatro archivos generados en .artifacts/scaffold antes de integrarlos.
powershell -NoProfile -ExecutionPolicy Bypass -File ./scripts/integrar-scaffold.ps1
```

La política se aplica únicamente al proceso invocado. El script lee Secret Manager en Windows y utiliza el proveedor `Npgsql.EntityFrameworkCore.PostgreSQL`. Enumera explícitamente las tres tablas `user_07.PRQAutomoviles`, `user_07.PRQParqueos` y `user_07.PRQIngresosAutomoviles`. No usa `--schema`, comodines ni `--force`. Si ya hay una salida previa en `.artifacts/scaffold`, consérvela o muévala a un respaldo antes de repetir; EF no la sobrescribe.

La integración renombra las entidades a `Automovil`, `Estacionamiento` e `Ingreso`, conserva los nombres SQL entre comillas, excluye de la serialización las navegaciones y elimina metadatos de enums ajenos que Npgsql descubre globalmente. Solo integra las tres entidades y su contexto. Mantiene intactos los parciales de claves y cálculos, la fábrica y los repositorios. El script de integración reemplaza los archivos generados: revisar el diff antes de confirmar el commit.

## Repositorios, consultas y CRUD

`IAutomoviles`, `IParqueos` e `IIngresos` usan los mismos contratos en ambas fuentes:

```csharp
using Parqueo;
using Parqueo.Modelos;
using Parqueo.Repositorios;

using IAlmacen datos = new AlmacenJson("./Parqueo/Datos");
// Alternativa: new AlmacenEf(new FabricaContexto().CreateDbContext([]));
IAutomoviles autos = new Automoviles(datos);
IParqueos parqueos = new Parqueos(datos);
IIngresos ingresos = new Ingresos(datos);
var rojos = autos.PorColor("roj");
var recientes = autos.PorAnios(2020, 2024);
var toyota = autos.PorFabricante("yota");
var motos = autos.PorTipo("mot");
var jose = parqueos.PorProvincia("José");
var central = parqueos.PorNombre("centr");
var economicos = parqueos.PorPrecios(500, 1500);
var tarifa = ingresos.ObtenerPrecioPorHoraPorParqueo(1);
var inicio = new DateTime(2026, 10, 1);
var fin = new DateTime(2026, 10, 31, 23, 59, 59);
var porTipo = ingresos.PorTipo("sed", inicio, fin);
var porProvincia = ingresos.PorProvincia("here", inicio, fin);

autos.Insertar(new Automovil { Id=100, Color="Verde", Anio=2023, Fabricante="Honda", Tipo="moto" });
var auto = autos.PorId(100)!;
auto.Color = "Gris";
autos.Actualizar(auto);
autos.Borrar(100);
```

Los tres repositorios ofrecen `PorId`, `Insertar`, `Actualizar` y `Borrar`. Actualizar recibe el registro completo con su clave original. Para ingresos deben existir sus padres. Identificadores ausentes devuelven `null` en `PorId`; listas vacías son válidas. Duplicados, parámetros inválidos, relaciones inexistentes y modificaciones de registros ausentes producen errores explícitos.

Los dos listados llaman a `ObtenerPrecioPorHoraPorParqueo` una vez por parqueo distinto y exponen `MontoACancelar`. EF filtra y combina las tablas en PostgreSQL antes de materializarlas. JSON evalúa las mismas expresiones localmente.

Cada operación CRUD EF valida y guarda en una transacción serializable. JSON bloquea el directorio durante la sesión, valida relaciones, escribe y vacía un temporal en el mismo directorio y reemplaza el archivo original. Las pruebas comprueban que un fallo de escritura conserva el archivo anterior. No hay transacciones de varios comandos ni de varios JSON. Use cada almacén secuencialmente y libérelo con `Dispose`. Los errores se propagan y la consola devuelve código 1.

## Pruebas y evidencia

```powershell
dotnet run --project Parqueo -- --pruebas
powershell -NoProfile -ExecutionPolicy Bypass -File ./scripts/verificar-datos.ps1
dotnet run --project Parqueo -- --pruebas-bd
dotnet run --project Parqueo -- --verificar-sql
```

Resultados ejecutados: 37 comprobaciones locales, 28 comprobaciones PostgreSQL, igualdad de los 22 registros SQL/JSON y validación SQL de cantidades, montos, salida nula y restricciones. Las pruebas locales usan copias temporales. Las pruebas remotas insertan claves temporales, comprueban CRUD, las eliminan en `finally` y comparan que los datos iniciales permanecen intactos. Ejecútelas en esta base de clase con los datos de prueba previstos. `sql/verificar.sql` usa rollback y no conserva modificaciones.

La entrega académica aún requiere adjuntar la conversación completa y la reflexión personal del estudiante. El estudiante autorizó publicar los commits en GitHub después de revisar la ejecución con PostgreSQL.

Referencias: [Npgsql EF Core](https://www.npgsql.org/efcore/), [TLS en Azure PostgreSQL](https://learn.microsoft.com/en-us/azure/postgresql/security/security-tls-how-to-connect) y [Secret Manager](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0).

# Sistema de parqueo

Trabajo en Clase 02 de Programación Avanzada. Aplicación de consola C# sobre .NET 10, EF Core 10.0.13 y SQL Server. Permite ejecutar consultas y CRUD mediante los mismos repositorios sobre SQL Server o tres archivos JSON. El repositorio original contenía solamente `.gitignore`.

## Ejecutar y comprobar

Desde la raíz, con el SDK .NET 10:

```powershell
dotnet restore TrabajoEnClase02.slnx
dotnet build TrabajoEnClase02.slnx --no-restore
dotnet run --project Parqueo --no-build
dotnet run --project Parqueo --no-build -- --pruebas
dotnet run --project Parqueo -- --datos ./Parqueo/Datos
powershell -NoProfile -ExecutionPolicy Bypass -File ./scripts/verificar-datos.ps1
```

La demostración lista ingresos en provincias que contienen `San`, con entrada, salida, minutos, horas y monto. Las pruebas usan copias temporales y no modifican los JSON originales. Su comando devuelve código distinto de cero ante fallos. Para conservar cambios reales, use `--datos ./Parqueo/Datos`: los archivos copiados al directorio de compilación pueden reemplazarse en compilaciones futuras.

## Estructura

- `diseño-bd.sql`: tablas, claves, restricciones e índices.
- `insertar-registros.sql`: 5 automóviles, 2 parqueos y 15 ingresos (2 abiertos).
- `Parqueo/Datos`: los mismos registros en JSON, sin propiedades calculadas persistidas.
- `Parqueo/Modelos`: entidades y clase parcial de cálculos.
- `Parqueo/ParqueoContext.cs`: mapeo manual equivalente al DDL y fábrica de conexión por secretos.
- `Parqueo/Repositorios`: interfaces, consultas comunes, validación y almacenes EF/JSON.
- `Parqueo/Pruebas.cs`: comprobaciones locales ejecutables sin servidor.
- `scripts/scaffold.ps1`: ingeniería inversa de las tres tablas explícitas.
- `sql/verificar.sql`: verificaciones preparadas para SQL Server, con rollback.
- `docs/entrega.md`: estado académico, evidencia y pendientes.

## Modelo y reglas

`PRQAutomoviles(Id, Color, Anio, Fabricante, Tipo)` y `PRQParqueos(Id, Provincia, Nombre, PrecioPorHora)` son los padres de `PRQIngresosAutomoviles(Consecutivo, IdParqueo, IdAutomovil, Entrada, Salida)`. Las claves son enteros positivos asignados por el llamador, sin IDENTITY. No se elimina un padre con ingresos relacionados. Los tipos válidos son `sedán`, `4x4` y `moto`.

Los intervalos de años, precios y fechas incluyen ambos límites. Se filtra por la fecha de entrada completa; para cubrir un día completo el llamador debe proporcionar también su hora final. Las fechas representan hora local del parqueo, sin conversión de zona. Las búsquedas textuales usan subcadenas sin distinguir mayúsculas; en SQL el tratamiento de acentos depende de la intercalación de la base. No hay búsqueda difusa por distancia de edición.

La estadía utiliza ticks y aritmética decimal: minutos / 60 × precio por hora. No se cobra por hora iniciada. Se redondea únicamente el monto final a dos decimales con `MidpointRounding.AwayFromZero`. Si no hay salida, minutos, horas y monto son nulos. El precio se obtiene del parqueo actual; no se guarda una tarifa histórica. `Ingreso.Calculos.cs` mantiene los cálculos separados del modelo persistido.

## Scripts SQL

Con autorización y una base SQL Server seleccionada, ejecute en SSMS, en este orden:

1. `diseño-bd.sql`.
2. `insertar-registros.sql`.
3. Opcionalmente `sql/verificar.sql` en una base de pruebas.

No se crea ni se elige una base automáticamente. Los dos primeros scripts se ejecutan una sola vez en una base sin estas tablas/datos: no son scripts de migración ni de reinicio. Usan transacciones y `XACT_ABORT`. No se ejecutaron contra el servidor del profesor.

## Secretos y conexión

Obtenga del profesor servidor, nombre de base, usuario, contraseña y requisitos de conexión. Configure localmente la cadena real, sustituyendo el marcador completo:

```powershell
dotnet user-secrets set "ConnectionStrings:Parqueo" "<CADENA_REAL_DEL_PROFESOR>" --project Parqueo
dotnet run --project Parqueo -- --sql
```

No coloque la cadena en archivos del repositorio. `UserSecretsId` ya está configurado. Secret Manager es almacenamiento local de desarrollo fuera del repositorio, no un almacén cifrado de producción. La fábrica lee `ConnectionStrings:Parqueo`; sin secreto informa el requisito y termina.

## Scaffold pendiente

La herramienta local `dotnet-ef` está fijada a 10.0.13. Una vez creadas las tablas reales y confirmada la conexión autorizada:

```powershell
dotnet tool restore
powershell -NoProfile -ExecutionPolicy Bypass -File ./scripts/scaffold.ps1
```

El script lee el secreto local y pasa la conexión en memoria al proceso de EF. No imprima ni registre sus argumentos. Incluye `--table dbo.PRQAutomoviles --table dbo.PRQParqueos --table dbo.PRQIngresosAutomoviles` y `--no-onconfiguring`. No utiliza `--force`. EF no interpreta `PRQ*` como un filtro de prefijo: se enumeran las tres tablas permitidas. El script de PowerShell está preparado para Windows.

El resultado se genera separado en `Parqueo/Generado`, con `ParqueoScaffoldContext`. El contexto y entidades actuales fueron escritos a partir del DDL; **no son resultado de un scaffold ejecutado**. Antes de sustituirlos, comparar propiedades, conservar los nombres `Automovil`, `Estacionamiento` e `Ingreso`, adaptar la fábrica y mantener `IRegistro`, `Clave` y propiedades calculadas en parciales separadas. Las entidades generadas usan nombres diferentes y no sustituyen automáticamente las actuales. La integración del scaffold real queda pendiente de acceso al servidor.

## Usar repositorios

Los contratos `IAutomoviles`, `IParqueos` e `IIngresos` no dependen de la fuente. Cambie únicamente el almacén:

```csharp
using Parqueo.Modelos;
using Parqueo.Repositorios;

using IAlmacen datos = new AlmacenJson("./Parqueo/Datos");
// Alternativa SQL: new AlmacenEf(new FabricaContexto().CreateDbContext([]));
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
```

Ambos listados llaman a `ObtenerPrecioPorHoraPorParqueo` una vez por parqueo distinto y exponen `MontoACancelar`. EF ejecuta filtros y joins en SQL; JSON evalúa las mismas expresiones localmente. No se utiliza `ToList` antes de filtrar las tablas SQL.

## CRUD

Los tres repositorios ofrecen `PorId`, `Insertar`, `Actualizar` y `Borrar`:

```csharp
autos.Insertar(new Automovil { Id=100, Color="Verde", Anio=2023, Fabricante="Honda", Tipo="moto" });
var auto = autos.PorId(100)!;
auto.Color = "Gris";
autos.Actualizar(auto);
autos.Borrar(100);
```

Para parqueos e ingresos se usa el mismo contrato con `Estacionamiento` e `Ingreso`. Al insertar un ingreso, el automóvil y el parqueo deben existir. Actualizar recibe el registro completo y no permite cambiar su clave. Un `PorId` inexistente devuelve `null`; una búsqueda vacía devuelve una lista vacía. Duplicados, fechas inválidas y parámetros incorrectos lanzan excepciones; actualizar o borrar un registro ausente también falla expresamente.

Cada operación es una unidad de persistencia. EF valida y guarda dentro de una transacción serializable. JSON bloquea el directorio mientras viva el almacén, valida relaciones, escribe y vacía un temporal en el mismo directorio y luego reemplaza el archivo; si falla antes del reemplazo, conserva el original. No existe una transacción de varios comandos del usuario ni de varios archivos. Los almacenes se usan secuencialmente, no desde hilos concurrentes. Dispose libera el bloqueo; un segundo proceso recibe error de acceso. Errores de lectura, JSON inválido o conexión se propagan al llamador y la consola informa el fallo con código de salida 1.

## Validación y pendientes

La ejecución local completa 37 comprobaciones y el verificador de datos confirma igualdad de los 22 registros SQL/JSON campo por campo. La ejecución local verifica consultas, CRUD de las tres entidades, relaciones, redondeo, límites, salida nula y reapertura. Se verifica además traducción de una consulta a SQL mediante `ToQueryString`, sin conexión: esto no prueba ejecución en SQL Server.

Pendientes: conexión real, ejecución de DDL e inserciones, scaffold e integración de sus resultados, consultas/CRUD sobre SQL Server y evidencias de esa ejecución. El push requiere revisión y autorización expresa.

Referencias consultadas: [Secret Manager de Microsoft](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0) e [ingeniería inversa de EF Core](https://learn.microsoft.com/en-us/ef/core/managing-schemas/scaffolding/).

# Evidencia del trabajo en clase

Asignatura: BSI-19 Programación Avanzada. Profesor: MSc. Héctor Egérico Fernández Méndez.
Estudiante: Andrey (completar nombre académico completo antes de entregar).
Repositorio: https://github.com/AndrewShot01/Trabajo-En-Clase-02-Andrey-MV

## Desarrollo y resultados

El repositorio inicial contenía únicamente .gitignore. Se creó una solución C# con SQL, JSON, modelos, cálculos, interfaces, repositorios, consultas y CRUD. La solicitud inicial indicaba SQL Server; posteriormente el estudiante confirmó que el servidor del profesor utiliza PostgreSQL y autorizó adaptar el proyecto y crear los objetos en su esquema user_07.

Se verificó la conexión TLS a advanced_programming con user_07. La inspección inicial encontró cero tablas en user_07. Se crearon transaccionalmente PRQAutomoviles, PRQParqueos y PRQIngresosAutomoviles y se insertaron 5 automóviles, 2 parqueos y 15 ingresos. No se modificaron otros esquemas.

Se ejecutó el scaffold real con Npgsql, limitado a las tres tablas de user_07. Sus entidades y contexto quedaron integrados; los cálculos y las claves permanecen en clases parciales independientes. Se excluyeron metadatos de enums ajenos que el proveedor descubre globalmente. No hay credenciales en los archivos versionados.

## Cumplimiento y pruebas

Implementado y ejecutado: DDL, inserciones, JSON equivalentes, configuración de secreto local, conexión PostgreSQL, scaffold integrado, consultas de ambas fuentes, función ObtenerPrecioPorHoraPorParqueo, cálculos nulos para ingresos abiertos y CRUD de las tres entidades.

Pasaron 37 comprobaciones locales, 28 comprobaciones contra PostgreSQL y la comparación campo por campo de los 22 registros SQL/JSON. También se ejecutó sql/verificar.sql con comprobaciones de cantidades, monto, ingreso abierto e integridad a nivel de base. Los registros temporales del CRUD se eliminaron y se verificó que los datos iniciales permanecieran intactos. Consulte docs/verificacion.txt y docs/verificacion-postgres.txt.

## Dificultades y decisiones

Se clonó el remoto porque la carpeta inicial estaba vacía. La identidad Git se configuró localmente con los datos proporcionados por el estudiante. Se sustituyó el proveedor SQL Server por Npgsql tras confirmar el motor real. Se conservaron las fechas locales en timestamp without time zone y los nombres SQL con mayúsculas mediante comillas. El scaffold renombra sus tipos al integrarse para conservar los contratos de los repositorios.

## Evidencia Git

Se trabajó en main con commits locales por etapas, sin pull requests ni fusiones. La lista real está disponible con git log --oneline; git status --short --branch y git remote -v muestran el estado y el remoto. El estudiante autorizó publicar los commits en GitHub después de revisar la ejecución con PostgreSQL.

## Conversación y reflexión pendientes del estudiante

Resumen, no transcripción literal: se solicitó inspección y plan; se indicó y clonó el repositorio oficial; se autorizó implementar el trabajo; se proporcionó identidad Git; se confirmó posteriormente PostgreSQL y el esquema user_07 y se autorizó crear tablas y datos. El agente implementó y comprobó la solución sobre ambas fuentes.

El estudiante debe adjuntar la conversación completa desde la aplicación y completar su reflexión personal. No se inventan capturas ni experiencias del estudiante. El README explica cómo ejecutar, consultar, realizar CRUD y regenerar el modelo.

Como base para esa reflexión: separar repositorios y almacenamiento permitió probar localmente y contrastar resultados con la base real; los JSON también requieren integridad referencial y escritura consistente; el scaffold debe regenerarse sin perder personalizaciones.

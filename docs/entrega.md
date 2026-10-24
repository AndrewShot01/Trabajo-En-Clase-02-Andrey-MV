# Evidencia del trabajo en clase

Asignatura: BSI-19 Programación Avanzada. Profesor: MSc. Héctor Egérico Fernández Méndez.
Estudiante: Andrey (completar nombre académico completo antes de entregar).
Repositorio: https://github.com/AndrewShot01/Trabajo-En-Clase-02-Andrey-MV

## Desarrollo y resultados

El repositorio inicial tenía únicamente un .gitignore. Se construyó una solución de consola C# con tres tablas relacionadas, datos equivalentes SQL/JSON, campos calculados, interfaces y repositorios comunes. El README explica la estructura, comandos y decisiones.

La ejecución local inicial completó 33 comprobaciones: filtros, CRUD, persistencia, integridad, montos y traducción SQL sin conexión. La ejecución final completó 37 comprobaciones, incluida protección ante escritura fallida y JSON inválido. El verificador de datos confirmó los 22 registros SQL/JSON campo por campo. La compilación final tuvo cero errores y cero advertencias; la auditoría NuGet no informó paquetes vulnerables. La ejecución final y sus resultados se registran en docs/verificacion.txt. Los artefactos de prueba se escriben en directorios temporales, sin alterar los datos de entrega.

## Cumplimiento

Implementado y probado localmente: datos JSON, repositorios, consultas, CRUD, cálculos y manejo de casos inválidos. Preparado: DDL SQL Server, inserciones, mapeo EF, lectura de secretos, procedimiento de scaffold y verificación SQL. Pendiente de verificar contra el servidor: scripts, conexión, ingeniería inversa e integración del modelo generado, consultas y CRUD EF. No se afirma que estas pruebas remotas hayan pasado.

## Dificultades

La carpeta abierta no era un repositorio; se clonó el remoto indicado. El remoto no contenía código. No había identidad Git; el estudiante proporcionó nombre y correo y se configuraron solamente para este repositorio. La restauración inicial informó una dependencia transitiva vulnerable; se actualizó el paquete criptográfico y se repitió la verificación. No se recibieron credenciales del servidor, por lo que se priorizó una implementación local comprobable.

## Evidencia Git

Se trabajó en main con commits locales por etapas, sin pull requests ni fusiones. Para obtener la lista real completa:

```powershell
git log --oneline
git status --short --branch
git remote -v
```

No se realizó push. La lista de commits locales se puede usar como evidencia; no se inventaron capturas ni resultados del servidor.

## Conversación con el agente

Este apartado es un resumen, no una transcripción literal. El estudiante solicitó inicialmente solo inspección y un plan. La inspección detectó una carpeta vacía. El estudiante indicó el repositorio oficial, que se clonó y contenía únicamente .gitignore. Posteriormente solicitó realizar el trabajo y suministró la identidad para los commits. El agente implementó los componentes locales y reportó la dependencia pendiente del servidor.

Antes de entregar, el estudiante debe adjuntar la conversación completa desde la aplicación, como solicita el documento del profesor. No se genera una transcripción que omita mensajes aparentando estar completa.

## Conclusiones para completar por el estudiante

La separación entre repositorios y almacenamiento permite reutilizar consultas y probar sin servidor. La validación de relaciones debe conservarse también al persistir JSON. El scaffold real requiere verificar el esquema existente, proteger credenciales y conservar personalizaciones en parciales. El estudiante debe añadir su reflexión personal sobre el aprendizaje y completar las evidencias SQL cuando disponga de conexión.

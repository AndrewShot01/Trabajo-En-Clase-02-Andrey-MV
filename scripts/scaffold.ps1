# Ejecutar únicamente después de confirmar servidor, permisos y tablas existentes.
$ErrorActionPreference = 'Stop'
$secreto = Join-Path $env:APPDATA 'Microsoft/UserSecrets/trabajo-en-clase-02-parqueo/secrets.json'
if (-not (Test-Path -LiteralPath $secreto)) { throw 'Configure primero ConnectionStrings:Parqueo con dotnet user-secrets.' }
$configuracion = Get-Content -LiteralPath $secreto -Raw | ConvertFrom-Json
$conexion = $configuracion.'ConnectionStrings:Parqueo'
if (-not $conexion -and $configuracion.ConnectionStrings) { $conexion = $configuracion.ConnectionStrings.Parqueo }
if (-not $conexion) { throw 'Falta el secreto ConnectionStrings:Parqueo.' }
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
 dotnet tool restore
 if ($LASTEXITCODE -ne 0) { throw 'No se pudo restaurar dotnet-ef.' }
 # Lista explícita: --table no admite comodines PRQ*. Sin --force ni credenciales en archivos.
 dotnet ef dbcontext scaffold $conexion Npgsql.EntityFrameworkCore.PostgreSQL --project Parqueo --table user_07.PRQAutomoviles --table user_07.PRQParqueos --table user_07.PRQIngresosAutomoviles --context ParqueoScaffoldContext --context-dir ../.artifacts/scaffold --output-dir ../.artifacts/scaffold/Entidades --namespace Parqueo.Generado --no-onconfiguring --use-database-names --no-pluralize
 if ($LASTEXITCODE -ne 0) { throw 'Falló el scaffold; revise conexión y permisos.' }
} finally { $conexion = $null; Pop-Location }

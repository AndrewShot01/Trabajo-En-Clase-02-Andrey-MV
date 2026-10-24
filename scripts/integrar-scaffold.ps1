param([string]$Ruta = '.artifacts/scaffold')
$ErrorActionPreference='Stop'
$raiz=Split-Path $PSScriptRoot -Parent
$origen=Join-Path $raiz $Ruta
$mapa=@{PRQAutomoviles='Automovil';PRQParqueos='Estacionamiento';PRQIngresosAutomoviles='Ingreso'}
$destino=Join-Path $raiz 'Parqueo/Modelos/Generado'
New-Item -ItemType Directory -Force $destino | Out-Null
function Convertir-Nombres([string]$texto) {
 foreach($nombre in $mapa.Keys){
  $texto=[regex]::Replace($texto,"\b$nombre\b",$mapa[$nombre])
  $texto=$texto.Replace('"'+$mapa[$nombre]+'"','"'+$nombre+'"')
 }
 return $texto
}
foreach($nombre in $mapa.Keys){
 $texto=Get-Content -Encoding UTF8 -LiteralPath (Join-Path $origen "Entidades/$nombre.cs") -Raw
 $texto=(Convertir-Nombres $texto).Replace('namespace Parqueo.Generado;','namespace Parqueo.Modelos;')
 $texto=$texto.Replace('    public virtual','    [System.Text.Json.Serialization.JsonIgnore]'+[Environment]::NewLine+'    public virtual')
 Set-Content -Encoding UTF8 -LiteralPath (Join-Path $destino ($mapa[$nombre]+'.generated.cs')) -Value $texto.TrimEnd()
}
$contexto=Get-Content -Encoding UTF8 -LiteralPath (Join-Path $origen 'ParqueoScaffoldContext.cs') -Raw
$contexto=(Convertir-Nombres $contexto).Replace('namespace Parqueo.Generado;','namespace Parqueo;').Replace('ParqueoScaffoldContext','ParqueoContext')
$contexto=$contexto.Replace('DbSet<Automovil> Automovil {','DbSet<Automovil> Automoviles {').Replace('DbSet<Ingreso> Ingreso {','DbSet<Ingreso> Ingresos {').Replace('DbSet<Estacionamiento> Estacionamiento {','DbSet<Estacionamiento> Parqueos {')
$contexto='using Parqueo.Modelos;'+[Environment]::NewLine+$contexto
# Npgsql inspecciona enums globales incluso al filtrar tablas. Excluir metadatos de otros esquemas.
$contexto=[regex]::Replace($contexto,'(?s)        modelBuilder\s*\.HasPostgresEnum.*?;\s*','')
Set-Content -Encoding UTF8 -LiteralPath (Join-Path $raiz 'Parqueo/ParqueoContext.cs') -Value $contexto.TrimEnd()
Write-Output 'Scaffold integrado: tres entidades, contexto de user_07 y cálculos conservados en parciales.'

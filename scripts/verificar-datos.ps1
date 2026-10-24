$ErrorActionPreference='Stop'
$raiz=Split-Path $PSScriptRoot -Parent
$sql=Get-Content -Encoding UTF8 -LiteralPath (Join-Path $raiz 'insertar-registros.sql')
$tablas=@{PRQAutomoviles='automoviles';PRQParqueos='parqueos';PRQIngresosAutomoviles='ingresos'}
$total=0
foreach($tabla in $tablas.Keys){
 $json=Get-Content -Encoding UTF8 -LiteralPath (Join-Path $raiz "Parqueo/Datos/$($tablas[$tabla]).json") -Raw | ConvertFrom-Json
 $filas=@($sql | Where-Object {$_ -match ('^INSERT INTO user_07\."'+$tabla+'" ')})
 if($filas.Count -ne $json.Count){throw "Cantidad diferente: $tabla"}
 foreach($fila in $filas){
  if($fila -notmatch '\(([^)]+)\) VALUES \((.+)\);'){throw 'INSERT no reconocido'}
  $columnas=@($Matches[1].Split(',') | ForEach-Object {$_.Trim('"')})
  $valores=@([regex]::Matches($Matches[2],"N?'(?:[^']|'')*'|NULL|[0-9.]+") | ForEach-Object {$_.Value})
  $clave=[int]$valores[0]
  $registro=@($json | Where-Object { $_.($columnas[0]) -eq $clave })
  if($registro.Count -ne 1){throw "Clave incorrecta: $tabla $clave"}
  for($n=0;$n -lt $columnas.Count;$n++){
   $valor=$valores[$n]
   $actual=$registro[0].($columnas[$n])
   if($valor -eq 'NULL'){if($null -ne $actual){throw 'Nulo diferente'}}
   elseif($valor -match "^N?'(.*)'$"){if($actual -cne $Matches[1]){throw "Texto diferente: $tabla $clave $($columnas[$n])"}}
   elseif([decimal]$actual -ne [decimal]$valor){throw 'Número diferente'}
  }
  $total++
 }
}
Write-Output "OK: $total registros SQL y JSON coinciden en todos sus campos."

-- Ejecutar en la base de pruebas autorizada, después del DDL y las inserciones.
-- No ejecutado aún contra SQL Server. Los cambios de esta prueba se revierten.
SET XACT_ABORT ON;
BEGIN TRY
 BEGIN TRANSACTION;
 IF (SELECT COUNT(*) FROM dbo.PRQAutomoviles) < 5 THROW 51000, 'Faltan automóviles', 1;
 IF (SELECT COUNT(*) FROM dbo.PRQParqueos) < 2 THROW 51000, 'Faltan parqueos', 1;
 IF (SELECT COUNT(*) FROM dbo.PRQIngresosAutomoviles) < 15 THROW 51000, 'Faltan ingresos', 1;
 SELECT i.Consecutivo, a.Tipo, p.Provincia, i.Entrada, i.Salida,
  CASE WHEN i.Salida IS NULL THEN NULL ELSE
   ROUND(DATEDIFF_BIG(SECOND,i.Entrada,i.Salida)/3600.0*p.PrecioPorHora,2) END AS Monto
 FROM dbo.PRQIngresosAutomoviles i
 JOIN dbo.PRQAutomoviles a ON a.Id=i.IdAutomovil
 JOIN dbo.PRQParqueos p ON p.Id=i.IdParqueo;
 IF EXISTS(SELECT 1 FROM dbo.PRQAutomoviles WHERE Id=900001)
  THROW 51000, 'Clave de prueba ocupada', 1;
 INSERT dbo.PRQAutomoviles VALUES(900001,N'Verde',2026,N'Prueba',N'moto');
 UPDATE dbo.PRQAutomoviles SET Color=N'Gris' WHERE Id=900001;
 IF NOT EXISTS(SELECT 1 FROM dbo.PRQAutomoviles WHERE Id=900001 AND Color=N'Gris') THROW 51000,'Actualización fallida',1;
 IF EXISTS(SELECT 1 FROM dbo.PRQParqueos WHERE Id=900001) OR EXISTS(SELECT 1 FROM dbo.PRQIngresosAutomoviles WHERE Consecutivo=900001)
  THROW 51000, 'Otra clave de prueba ocupada', 1;
 INSERT dbo.PRQParqueos VALUES(900001,N'Cartago',N'Prueba',600);
 UPDATE dbo.PRQParqueos SET PrecioPorHora=900 WHERE Id=900001;
 IF NOT EXISTS(SELECT 1 FROM dbo.PRQParqueos WHERE Id=900001 AND PrecioPorHora=900) THROW 51000,'Actualización de parqueo fallida',1;
 INSERT dbo.PRQIngresosAutomoviles VALUES(900001,900001,900001,'2026-10-20T08:00:00',NULL);
 IF NOT EXISTS(SELECT 1 FROM dbo.PRQIngresosAutomoviles WHERE Consecutivo=900001 AND Salida IS NULL) THROW 51000,'Ingreso abierto fallido',1;
 UPDATE dbo.PRQIngresosAutomoviles SET Salida='2026-10-20T09:00:00' WHERE Consecutivo=900001;
 IF NOT EXISTS(SELECT 1 FROM dbo.PRQIngresosAutomoviles WHERE Consecutivo=900001 AND DATEDIFF(MINUTE,Entrada,Salida)=60) THROW 51000,'Actualización de ingreso fallida',1;
 DELETE dbo.PRQIngresosAutomoviles WHERE Consecutivo=900001;
 DELETE dbo.PRQAutomoviles WHERE Id=900001;
 DELETE dbo.PRQParqueos WHERE Id=900001;
 IF EXISTS(SELECT 1 FROM dbo.PRQIngresosAutomoviles WHERE Consecutivo=900001) OR EXISTS(SELECT 1 FROM dbo.PRQAutomoviles WHERE Id=900001) OR EXISTS(SELECT 1 FROM dbo.PRQParqueos WHERE Id=900001) THROW 51000,'Borrado fallido',1;
 ROLLBACK;
 PRINT 'Comprobaciones SQL completadas y cambios revertidos';
END TRY
BEGIN CATCH
 IF @@TRANCOUNT > 0 ROLLBACK;
 THROW;
END CATCH;

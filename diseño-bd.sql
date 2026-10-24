SET XACT_ABORT ON;
BEGIN TRANSACTION;
CREATE TABLE dbo.PRQAutomoviles (
 Id int NOT NULL PRIMARY KEY CHECK(Id > 0), Color nvarchar(50) NOT NULL CHECK(LEN(LTRIM(RTRIM(Color))) > 0),
 Anio int NOT NULL CHECK(Anio BETWEEN 1886 AND 2100), Fabricante nvarchar(80) NOT NULL CHECK(LEN(LTRIM(RTRIM(Fabricante))) > 0),
 Tipo nvarchar(10) NOT NULL CHECK(Tipo IN (N'sedán',N'4x4',N'moto')));
CREATE TABLE dbo.PRQParqueos (
 Id int NOT NULL PRIMARY KEY CHECK(Id > 0), Provincia nvarchar(50) NOT NULL CHECK(LEN(LTRIM(RTRIM(Provincia))) > 0),
 Nombre nvarchar(100) NOT NULL CHECK(LEN(LTRIM(RTRIM(Nombre))) > 0), PrecioPorHora decimal(12,2) NOT NULL CHECK(PrecioPorHora > 0));
CREATE TABLE dbo.PRQIngresosAutomoviles (
 Consecutivo int NOT NULL PRIMARY KEY CHECK(Consecutivo > 0), IdParqueo int NOT NULL REFERENCES dbo.PRQParqueos(Id),
 IdAutomovil int NOT NULL REFERENCES dbo.PRQAutomoviles(Id), Entrada datetime2 NOT NULL, Salida datetime2 NULL,
 CONSTRAINT CK_PRQ_Fechas CHECK(Salida IS NULL OR Salida >= Entrada));
CREATE INDEX IX_PRQIngresos_Entrada ON dbo.PRQIngresosAutomoviles(Entrada) INCLUDE(IdAutomovil,IdParqueo,Salida);
CREATE INDEX IX_PRQIngresos_Automovil ON dbo.PRQIngresosAutomoviles(IdAutomovil);
CREATE INDEX IX_PRQIngresos_Parqueo ON dbo.PRQIngresosAutomoviles(IdParqueo);
COMMIT;

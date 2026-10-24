-- PostgreSQL: ejecutar únicamente en el esquema user_07.
BEGIN TRANSACTION;
CREATE TABLE user_07."PRQAutomoviles" (
 "Id" int NOT NULL PRIMARY KEY CHECK("Id" > 0), "Color" varchar(50) NOT NULL CHECK(length(ltrim(rtrim("Color"))) > 0),
 "Anio" int NOT NULL CHECK("Anio" BETWEEN 1886 AND 2100), "Fabricante" varchar(80) NOT NULL CHECK(length(ltrim(rtrim("Fabricante"))) > 0),
 "Tipo" varchar(10) NOT NULL CHECK("Tipo" IN ('sedán','4x4','moto')));
CREATE TABLE user_07."PRQParqueos" (
 "Id" int NOT NULL PRIMARY KEY CHECK("Id" > 0), "Provincia" varchar(50) NOT NULL CHECK(length(ltrim(rtrim("Provincia"))) > 0),
 "Nombre" varchar(100) NOT NULL CHECK(length(ltrim(rtrim("Nombre"))) > 0), "PrecioPorHora" decimal(12,2) NOT NULL CHECK("PrecioPorHora" > 0));
CREATE TABLE user_07."PRQIngresosAutomoviles" (
 "Consecutivo" int NOT NULL PRIMARY KEY CHECK("Consecutivo" > 0), "IdParqueo" int NOT NULL REFERENCES user_07."PRQParqueos"("Id"),
 "IdAutomovil" int NOT NULL REFERENCES user_07."PRQAutomoviles"("Id"), "Entrada" timestamp without time zone NOT NULL, "Salida" timestamp without time zone NULL,
 CONSTRAINT CK_PRQ_Fechas CHECK("Salida" IS NULL OR "Salida" >= "Entrada"));
CREATE INDEX IX_PRQIngresos_Entrada ON user_07."PRQIngresosAutomoviles"("Entrada") INCLUDE("IdAutomovil","IdParqueo","Salida");
CREATE INDEX IX_PRQIngresos_Automovil ON user_07."PRQIngresosAutomoviles"("IdAutomovil");
CREATE INDEX IX_PRQIngresos_Parqueo ON user_07."PRQIngresosAutomoviles"("IdParqueo");
COMMIT;

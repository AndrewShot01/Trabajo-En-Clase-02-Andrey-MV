-- PostgreSQL. Ejecutar después de cargar los datos iniciales. No modifica datos al terminar.
BEGIN;
DO $$
DECLARE precio numeric;
BEGIN
 IF (SELECT count(*) FROM user_07."PRQAutomoviles") <> 5 THEN RAISE EXCEPTION 'Cantidad incorrecta de automóviles'; END IF;
 IF (SELECT count(*) FROM user_07."PRQParqueos") <> 2 THEN RAISE EXCEPTION 'Cantidad incorrecta de parqueos'; END IF;
 IF (SELECT count(*) FROM user_07."PRQIngresosAutomoviles") <> 15 THEN RAISE EXCEPTION 'Cantidad incorrecta de ingresos'; END IF;
 SELECT round(extract(epoch FROM (i."Salida"-i."Entrada"))/3600*p."PrecioPorHora",2)
 INTO precio FROM user_07."PRQIngresosAutomoviles" i JOIN user_07."PRQParqueos" p ON p."Id"=i."IdParqueo" WHERE i."Consecutivo"=1;
 IF precio IS DISTINCT FROM 675.00 THEN RAISE EXCEPTION 'Monto incorrecto'; END IF;
 IF NOT EXISTS(SELECT 1 FROM user_07."PRQIngresosAutomoviles" WHERE "Consecutivo"=14 AND "Salida" IS NULL) THEN RAISE EXCEPTION 'Falta ingreso abierto'; END IF;
 BEGIN
  DELETE FROM user_07."PRQAutomoviles" WHERE "Id"=1;
  RAISE EXCEPTION 'La base permitió borrar un padre relacionado';
 EXCEPTION WHEN foreign_key_violation THEN NULL;
 END;
 BEGIN
  UPDATE user_07."PRQIngresosAutomoviles" SET "Salida"="Entrada"-interval '1 minute' WHERE "Consecutivo"=1;
  RAISE EXCEPTION 'La base permitió una salida anterior';
 EXCEPTION WHEN check_violation THEN NULL;
 END;
 BEGIN
  UPDATE user_07."PRQIngresosAutomoviles" SET "IdAutomovil"=2147483647 WHERE "Consecutivo"=1;
  RAISE EXCEPTION 'La base permitió una referencia inexistente';
 EXCEPTION WHEN foreign_key_violation THEN NULL;
 END;
END $$;
ROLLBACK;

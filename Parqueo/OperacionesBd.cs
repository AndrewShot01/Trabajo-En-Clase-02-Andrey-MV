using Microsoft.Extensions.Configuration;
using Npgsql;
namespace Parqueo;

public static class OperacionesBd
{
    public static NpgsqlConnection Conexion()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<FabricaContexto>().Build();
        return new NpgsqlConnection(config.GetConnectionString("Parqueo") ?? throw new InvalidOperationException("Falta el secreto de conexión."));
    }
    public static void VerificarSql()
    {
        using var conexion = Conexion(); conexion.Open();
        using var comando = new NpgsqlCommand(File.ReadAllText("sql/verificar.sql"), conexion);
        comando.ExecuteNonQuery();
        Console.WriteLine("OK PostgreSQL: verificación SQL de cantidades, monto, salida nula y restricciones; rollback completado.");
    }
    public static void Inicializar()
    {
        using var conexion = Conexion(); conexion.Open();
        using var transaccion = conexion.BeginTransaction();
        using (var verificar = new NpgsqlCommand("SELECT count(*) FROM pg_catalog.pg_tables WHERE schemaname='user_07' AND tablename IN ('PRQAutomoviles','PRQParqueos','PRQIngresosAutomoviles')", conexion, transaccion))
            if ((long)verificar.ExecuteScalar()! != 0) throw new InvalidOperationException("Ya existen tablas PRQ. No se sobrescribirá su contenido.");
        foreach (var archivo in new[] { "diseño-bd.sql", "insertar-registros.sql" })
        {
            var sql = System.Text.RegularExpressions.Regex.Replace(File.ReadAllText(archivo), @"(?im)^\s*(BEGIN TRANSACTION|COMMIT);\s*$", "");
            using var cmd = new NpgsqlCommand(sql, conexion, transaccion); cmd.ExecuteNonQuery();
        }
        transaccion.Commit();
        Console.WriteLine("Tablas PRQ y 22 registros creados en user_07 dentro de una transacción.");
    }
    public static void Inspeccionar()
    {
        using var conexion = Conexion(); conexion.Open();
        using var cmd = new NpgsqlCommand("SELECT current_database(), current_user, current_schema(); SELECT tablename FROM pg_catalog.pg_tables WHERE schemaname='user_07' ORDER BY tablename", conexion);
        using var lector = cmd.ExecuteReader();
        lector.Read(); Console.WriteLine($"Base: {lector.GetString(0)}; usuario: {lector.GetString(1)}; esquema: {lector.GetString(2)}");
        lector.NextResult(); int n = 0;
        while (lector.Read()) { Console.WriteLine("Tabla existente: " + lector.GetString(0)); n++; }
        Console.WriteLine($"{n} tablas en user_07. Inspección sin cambios.");
    }
}

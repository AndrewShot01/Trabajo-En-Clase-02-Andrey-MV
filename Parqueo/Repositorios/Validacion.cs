using Parqueo.Modelos;
namespace Parqueo.Repositorios;

public static class Validacion
{
    public static string Texto(string? texto, int max = 100)
    {
        if (string.IsNullOrWhiteSpace(texto) || texto.Length > max) throw new ArgumentException($"Texto requerido, máximo {max} caracteres.");
        return texto.Trim();
    }
    public static void Rango<T>(T desde, T hasta) where T : IComparable<T>
    { if (desde.CompareTo(hasta) > 0) throw new ArgumentException("Intervalo invertido."); }
    public static void Registro<T>(IAlmacen almacen, T r, string operacion) where T : class, IRegistro
    {
        if (r.Clave <= 0) throw new ArgumentException("La clave debe ser positiva.");
        var existe = almacen.Consultar<T>().Any(Claves.Es<T>(r.Clave));
        if (operacion == "insertar" && existe) throw new InvalidOperationException("Clave duplicada.");
        if (operacion != "insertar" && !existe) throw new KeyNotFoundException("Registro inexistente.");
        if (operacion == "borrar")
        {
            if (r is Automovil && almacen.Consultar<Ingreso>().Any(x => x.IdAutomovil == r.Clave) ||
                r is Estacionamiento && almacen.Consultar<Ingreso>().Any(x => x.IdParqueo == r.Clave))
                throw new InvalidOperationException("El registro tiene ingresos relacionados.");
            return;
        }
        switch (r)
        {
            case Automovil a:
                Texto(a.Color, 50); Texto(a.Fabricante, 80);
                if (a.Anio < 1886 || a.Anio > 2100 || a.Tipo is not ("sedán" or "4x4" or "moto")) throw new ArgumentException("Año o tipo inválido.");
                break;
            case Estacionamiento p:
                Texto(p.Provincia, 50); Texto(p.Nombre);
                if (p.PrecioPorHora <= 0 || p.PrecioPorHora > 9999999999.99m || decimal.Round(p.PrecioPorHora, 2) != p.PrecioPorHora) throw new ArgumentException("Precio inválido (máximo dos decimales).");
                break;
            case Ingreso i:
                if (i.Entrada == default || i.Salida < i.Entrada) throw new ArgumentException("Fechas inválidas.");
                if (!almacen.Consultar<Automovil>().Any(x => x.Id == i.IdAutomovil) || !almacen.Consultar<Estacionamiento>().Any(x => x.Id == i.IdParqueo)) throw new ArgumentException("Automóvil o parqueo inexistente.");
                break;
        }
    }
}

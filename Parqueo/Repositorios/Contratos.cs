using Parqueo.Modelos;
namespace Parqueo.Repositorios;

public interface IAlmacen : IDisposable
{
    IQueryable<T> Consultar<T>() where T : class, IRegistro;
    void Cambiar<T>(T registro, string operacion) where T : class, IRegistro;
}
public interface IRepositorio<T> where T : class, IRegistro
{
    T? PorId(int id);
    void Insertar(T registro);
    void Actualizar(T registro);
    void Borrar(int id);
}
public interface IAutomoviles : IRepositorio<Automovil>
{
    IReadOnlyList<Automovil> PorColor(string texto);
    IReadOnlyList<Automovil> PorAnios(int desde, int hasta);
    IReadOnlyList<Automovil> PorFabricante(string texto);
    IReadOnlyList<Automovil> PorTipo(string texto);
}
public interface IParqueos : IRepositorio<Estacionamiento>
{
    IReadOnlyList<Estacionamiento> PorProvincia(string texto);
    IReadOnlyList<Estacionamiento> PorNombre(string texto);
    IReadOnlyList<Estacionamiento> PorPrecios(decimal desde, decimal hasta);
}
public interface IIngresos : IRepositorio<Ingreso>
{
    decimal ObtenerPrecioPorHoraPorParqueo(int id);
    IReadOnlyList<DetalleIngreso> PorTipo(string tipo, DateTime desde, DateTime hasta);
    IReadOnlyList<DetalleIngreso> PorProvincia(string provincia, DateTime desde, DateTime hasta);
}
public record DetalleIngreso(Ingreso Ingreso, Automovil Automovil, Estacionamiento Parqueo)
{
    public decimal? DuracionMinutos => Ingreso.DuracionMinutos;
    public decimal? DuracionHoras => Ingreso.DuracionHoras;
    public decimal? MontoACancelar => Ingreso.MontoTotal;
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
namespace Parqueo;

public class FabricaContexto : IDesignTimeDbContextFactory<ParqueoContext>
{
    public ParqueoContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder().AddUserSecrets<FabricaContexto>().Build();
        var cadena = config.GetConnectionString("Parqueo") ?? throw new InvalidOperationException("Configure el secreto ConnectionStrings:Parqueo.");
        return new(new DbContextOptionsBuilder<ParqueoContext>().UseNpgsql(cadena).Options);
    }
}

using Parqueo.Api;
using Parqueo.Repositorios;
using System.Text.Encodings.Web;
using System.Text.Unicode;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All));
builder.Services.AddHttpContextAccessor();
var ruta = builder.Configuration["Parqueo:RutaJson"] ?? Path.Combine(builder.Environment.ContentRootPath, ".datos-api");
ruta = Path.GetFullPath(ruta, builder.Environment.ContentRootPath);
builder.Configuration["Parqueo:RutaJson"] = ruta;
Directory.CreateDirectory(ruta);
foreach (var archivo in Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "DatosIniciales"), "*.json"))
{
    var destino = Path.Combine(ruta, Path.GetFileName(archivo)); if (!File.Exists(destino)) File.Copy(archivo, destino);
}
builder.Services.AddSingleton<AccesoDatos>();
builder.Services.AddScoped<IAlmacen>(sp => sp.GetRequiredService<AccesoDatos>().Crear());
builder.Services.AddScoped<IAutomoviles, Automoviles>();
builder.Services.AddScoped<IParqueos, Parqueos>();
builder.Services.AddScoped<IIngresos, Ingresos>();
builder.Services.AddScoped<Navegaciones>();
var app = builder.Build();
app.Use(async (ctx, next) => { try { await next(); } catch (Exception e) when (!ctx.Response.HasStarted) { await Errores.Responder(ctx, e); } });
app.MapGet("/", () => Results.Ok(new { trabajo = "Trabajo en clase 3 — API del parqueo", ayuda = "GET /api/automoviles, /api/parqueos, /api/ingresos. Cabecera X-Fuente-Datos: json o postgres." }));
app.MapControllers();
app.Run();

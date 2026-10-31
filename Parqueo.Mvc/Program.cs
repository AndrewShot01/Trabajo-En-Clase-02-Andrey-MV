using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Parqueo.Mvc.Models;
using Parqueo.Mvc.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddOptions<ParqueoOpciones>().Bind(builder.Configuration.GetSection("ParqueoApi"))
 .Validate(x => Uri.TryCreate(x.BaseUrl, UriKind.Absolute, out var u) && u.Scheme is "http" or "https" && x.BaseUrl.EndsWith('/'), "BaseUrl debe ser una URL HTTP absoluta terminada en /.")
 .Validate(x => new[] { x.FuenteMantenimiento, x.FuenteConsultas }.All(f => f is "json" or "postgres"), "Las fuentes deben ser json o postgres.").ValidateOnStart();
builder.Services.AddHttpClient<ParqueoApi>((sp, client) => { client.BaseAddress = new Uri(sp.GetRequiredService<IOptionsMonitor<ParqueoOpciones>>().CurrentValue.BaseUrl); client.Timeout = TimeSpan.FromSeconds(20); });
var app = builder.Build();
app.UseExceptionHandler("/Home/Error");
app.UseRouting();
app.MapStaticAssets();
app.MapControllerRoute("default", "{controller=Automoviles}/{action=Index}/{id?}").WithStaticAssets();
app.Run();

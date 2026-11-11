using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SistemaVisitas.Client;
using SistemaVisitas.Client.Auth;
using SistemaVisitas.Client.Services;
using System.Net.Http; // <--- Necesario para IHttpClientFactory

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 1. Registramos el Interceptor como Transient
builder.Services.AddTransient<CustomAuthorizationMessageHandler>();

// 2. Configuramos un HttpClient base con el Interceptor (Puerto 7032 según tu configuración)
builder.Services.AddHttpClient("ApiBackend", client =>
    client.BaseAddress = new Uri("https://localhost:7032/"))
    .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

// 3. Modificamos la inyección para que use ese HttpClient específico
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiBackend"));

// 4. Inyectar Servicios de negocio
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TiposDispositivosService>();
builder.Services.AddScoped<DispositivosService>();
builder.Services.AddScoped<EmpresasService>();
builder.Services.AddScoped<PlantillasVisitaService>();
builder.Services.AddScoped<HojasVisitaService>();
builder.Services.AddScoped<UsuariosService>();
builder.Services.AddScoped<DashboardService>();

// 5. Inyectar dependencias de Seguridad
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

await builder.Build().RunAsync();
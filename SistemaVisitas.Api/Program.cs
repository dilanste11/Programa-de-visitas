using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using Scalar.AspNetCore;
using SistemaVisitas.Api.Models; // Para que reconozca ApplicationDbContext
using SistemaVisitas.Api.Services;
using System.Text;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);
// Leer configuración de Cloudinary
var cloudName = builder.Configuration.GetValue<string>("CloudinarySettings:CloudName");
var apiKey = builder.Configuration.GetValue<string>("CloudinarySettings:ApiKey");
var apiSecret = builder.Configuration.GetValue<string>("CloudinarySettings:ApiSecret");

Account account = new Account(cloudName, apiKey, apiSecret);
Cloudinary cloudinary = new Cloudinary(account);



// 1. Configuración de Base de Datos
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContext")
    ?? throw new InvalidOperationException("Connection string 'ApplicationDbContext' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Configuración de Controladores y OpenAPI (Scalar)
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton(cloudinary);

builder.Services.AddScoped<GeneradorPdfService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirBlazor", builder =>
    {
        builder.AllowAnyOrigin()  // Permite peticiones de cualquier puerto (ideal para desarrollo local)
               .AllowAnyMethod()  // Permite POST, GET, PUT, DELETE, etc.
               .AllowAnyHeader(); // Permite enviar tokens y encabezados personalizados
    });
});


// 3. Configuración del Middleware de Autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// 4. Configuración del Pipeline HTTP (Middlewares)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors("PermitirBlazor");

// MUY IMPORTANTE: Authentication (Saber quién eres) siempre debe ir antes de Authorization (Saber qué puedes hacer)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
using Microsoft.AspNetCore.Builder; // <--- VITAL PARA WEBAPPLICATION
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.OpenApi;
using Scalar.AspNetCore;
using HOPAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVICIOS (Dependency Injection) ---
builder.Services.AddControllers();

// Configuración de OpenAPI (necesario para Scalar)
builder.Services.AddOpenApi();

// Registrar tu clase de acceso a datos para SQL Server
builder.Services.AddScoped<SqlDataAccess>();

// Configurar CORS para tu Frontend en React
builder.Services.AddCors(options => {
    options.AddPolicy("AllowReact", policy => 
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// --- 2. MIDDLEWARE (Pipeline de ejecución) ---
if (app.Environment.IsDevelopment())
{
    // Habilitar el endpoint de OpenAPI (v1.json)
    app.MapOpenApi();
    
    // Habilitar la interfaz de Scalar
    app.MapScalarApiReference(options => {
        options.WithTitle("HOP API - Sistema de Trabajos")
               .WithTheme(ScalarTheme.Moon)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseCors("AllowReact");
app.UseHttpsRedirection();
app.UseAuthorization();

// Mapear los controladores (Usuarios, Servicios, etc.)
app.MapControllers();

app.Run();
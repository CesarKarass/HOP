using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.OpenApi;
using Scalar.AspNetCore;
using HOPAPI.Data;
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVICIOS (Dependency Injection) ---
builder.Services.AddControllers();

// Configuración de OpenAPI
builder.Services.AddOpenApi();

// Registrar tu clase de acceso a datos para SQL Server
builder.Services.AddScoped<SqlDataAccess>();

// Configurar CORS para tu Frontend (Node.js en puerto 3000)
builder.Services.AddCors(options => {
    options.AddPolicy("AllowFrontend", policy => 
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

var app = builder.Build();

// --- 2. MIDDLEWARE ---

// Servir archivos estáticos desde wwwroot
app.UseStaticFiles(); // Esto sirve archivos desde wwwroot

// También servir archivos desde la carpeta static (para compatibilidad)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "static")),
    RequestPath = "/static"
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => {
        options.WithTitle("HOP API - Sistema de Trabajos")
               .WithTheme(ScalarTheme.Moon)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
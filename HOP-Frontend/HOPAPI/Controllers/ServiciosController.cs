using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using HOPAPI.Data;
using HOPAPI.Models;
using System.Data;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace HOPAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiciosController : ControllerBase
{
    private readonly SqlDataAccess _db;
    private readonly IWebHostEnvironment _env;

    public ServiciosController(SqlDataAccess db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpPost("buscar")]
    public async Task<IActionResult> Listar([FromBody] FiltroServicioDto filtros)
    {
        try
        {
            Console.WriteLine($"=== BUSCANDO SERVICIOS ===");
            Console.WriteLine($"Busqueda: '{filtros.Busqueda}'");
            Console.WriteLine($"CategoriaId: {filtros.CategoriaId}");
            
            SqlParameter[] parameters = {
                new SqlParameter("@Busqueda", string.IsNullOrEmpty(filtros.Busqueda) ? DBNull.Value : (object)filtros.Busqueda),
                new SqlParameter("@CategoriaID", filtros.CategoriaId == 0 ? DBNull.Value : (object)filtros.CategoriaId)
            };

            DataTable dt = await _db.ExecuteQueryAsync("ObtenerServicios", parameters);
            
            Console.WriteLine($"Filas devueltas: {dt.Rows.Count}");
            
            var lista = dt.AsEnumerable().Select(row => new ServicioAlertaDto
            {
                Id = Convert.ToInt32(row["Id"]),
                Titulo = row["Titulo"]?.ToString() ?? "",
                Ubicacion = row["Ubicacion"]?.ToString() ?? "",
                Categoria = row["Categoria"]?.ToString() ?? "",
                Autor = row["Autor"]?.ToString() ?? "",
                FechaRegistro = Convert.ToDateTime(row["FechaRegistro"]),
                Descripcion = row["Descripcion"]?.ToString() ?? "",
                ImagenURL = row["ImagenURL"]?.ToString() ?? "/static/images/vacante.jpg",
                AutorId = Convert.ToInt32(row["AutorId"])
            }).ToList();

            Console.WriteLine($"Servicios encontrados: {lista.Count}");
            
            return Ok(lista);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine($"STACK: {ex.StackTrace}");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromForm] string Titulo,
        [FromForm] int UsuarioID,
        [FromForm] string Ubicacion,
        [FromForm] int CategoriaID,
        [FromForm] string Descripcion,
        IFormFile imagen = null)
    {
        try
        {
            string imagenUrl = "/static/images/vacante.jpg";
            
            if (imagen != null && imagen.Length > 0)
            {
                var extension = Path.GetExtension(imagen.FileName).ToLower();
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }
                
                imagenUrl = "/uploads/" + uniqueFileName;
            }
            
            SqlParameter[] p = {
                new SqlParameter("@Titulo", Titulo),
                new SqlParameter("@UsuarioID", UsuarioID),
                new SqlParameter("@Ubicacion", Ubicacion),
                new SqlParameter("@CategoriaID", CategoriaID),
                new SqlParameter("@Descripcion", Descripcion ?? ""),
                new SqlParameter("@ImagenURL", imagenUrl)
            };
            await _db.ExecuteNonQueryAsync("CrearServicio", p);
            return Ok(new { mensaje = "Servicio publicado exitosamente", imagenUrl = imagenUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("actualizar")]
    public async Task<IActionResult> Actualizar(
        [FromForm] int id,
        [FromForm] string Titulo,
        [FromForm] string Descripcion,
        [FromForm] string Ubicacion,
        [FromForm] int CategoriaId,
        IFormFile imagen = null)
    {
        try
        {
            string imagenUrl = null;
            
            if (imagen != null && imagen.Length > 0)
            {
                var extension = Path.GetExtension(imagen.FileName).ToLower();
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }
                
                imagenUrl = "/uploads/" + uniqueFileName;
            }
            
            SqlParameter[] p = {
                new SqlParameter("@Id", id),
                new SqlParameter("@Titulo", Titulo),
                new SqlParameter("@Descripcion", Descripcion ?? ""),
                new SqlParameter("@Ubicacion", Ubicacion),
                new SqlParameter("@CategoriaID", CategoriaId),
                new SqlParameter("@ImagenURL", imagenUrl ?? (object)DBNull.Value)
            };
            await _db.ExecuteNonQueryAsync("ActualizarServicio", p);
            return Ok(new { mensaje = "Servicio actualizado exitosamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            SqlParameter[] p = { new SqlParameter("@Id", id) };
            await _db.ExecuteNonQueryAsync("EliminarServicio", p);
            return Ok(new { mensaje = "Servicio eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("postular")]
    public async Task<IActionResult> Postular([FromForm] int servicioId, [FromForm] int prestadorId)
    {
        try 
        {
            SqlParameter[] p = { 
                new SqlParameter("@ServicioID", servicioId), 
                new SqlParameter("@PrestadorID", prestadorId) 
            };
            await _db.ExecuteNonQueryAsync("CrearPostulacion", p);
            return Ok(new { mensaje = "Postulación enviada" });
        } 
        catch (Exception ex) 
        { 
            return BadRequest(new { error = ex.Message }); 
        }
    }
}
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
using System.Collections.Generic;

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
            //   AutorId = Convert.ToInt32(row["AutorId"]),
                Reclutando = row["Reclutando"] != DBNull.Value && Convert.ToBoolean(row["Reclutando"])
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
        [FromForm] bool Reclutando = true,
        IFormFile imagen = null)
    {
        try
        {
            string imagenUrl = "/static/images/vacante.jpg";
            
            if (imagen != null && imagen.Length > 0)
            {
                var extension = Path.GetExtension(imagen.FileName).ToLower();
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
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
                new SqlParameter("@ImagenURL", imagenUrl),
                new SqlParameter("@Reclutando", Reclutando)
            };
            await _db.ExecuteNonQueryAsync("CrearServicio", p);
            return Ok(new { mensaje = "Servicio publicado exitosamente", imagenUrl = imagenUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("subir-imagenes/{servicioId}")]
    public async Task<IActionResult> SubirImagenes(int servicioId, List<IFormFile> imagenes)
    {
        try
        {
            var imagenesUrls = new List<string>();
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "servicios", servicioId.ToString());
            
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);
            
            int orden = 0;
            foreach (var imagen in imagenes)
            {
                if (imagen.Length > 0)
                {
                    var fileName = $"{DateTime.Now.Ticks}_{orden}_{imagen.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagen.CopyToAsync(stream);
                    }
                    
                    var imagenUrl = $"/uploads/servicios/{servicioId}/{fileName}";
                    imagenesUrls.Add(imagenUrl);
                    
                    SqlParameter[] p = {
                        new SqlParameter("@ServicioId", servicioId),
                        new SqlParameter("@ImagenUrl", imagenUrl),
                        new SqlParameter("@Orden", orden)
                    };
                    await _db.ExecuteNonQueryAsync("InsertarImagenServicio", p);
                    orden++;
                }
            }
            
            return Ok(new { mensaje = "Imágenes subidas correctamente", imagenes = imagenesUrls });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("imagenes/{servicioId}")]
    public async Task<IActionResult> GetImagenesServicio(int servicioId)
    {
        try
        {
            SqlParameter[] p = { new SqlParameter("@ServicioId", servicioId) };
            DataTable dt = await _db.ExecuteQueryAsync("ObtenerImagenesServicio", p);
            
            var imagenes = dt.AsEnumerable().Select(row => new
            {
                Id = row.Field<int>("Id"),
                ImagenUrl = row.Field<string>("ImagenUrl"),
                Orden = row.Field<int>("Orden")
            }).ToList();
            
            return Ok(imagenes);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("imagenes/{imagenId}")]
    public async Task<IActionResult> EliminarImagen(int imagenId)
    {
        try
        {
            SqlParameter[] pGet = { new SqlParameter("@ImagenId", imagenId) };
            DataTable dt = await _db.ExecuteQueryAsync("ObtenerImagenPorId", pGet);
            
            if (dt.Rows.Count > 0)
            {
                string imagenUrl = dt.Rows[0]["ImagenUrl"].ToString();
                var filePath = Path.Combine(_env.WebRootPath, imagenUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }
            
            SqlParameter[] p = { new SqlParameter("@ImagenId", imagenId) };
            await _db.ExecuteNonQueryAsync("EliminarImagenServicio", p);
            
            return Ok(new { mensaje = "Imagen eliminada correctamente" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("actualizar")]
    public async Task<IActionResult> Actualizar(
        [FromForm] int id,
        [FromForm] string Titulo,
        [FromForm] string Descripcion,
        [FromForm] string Ubicacion,
        [FromForm] int CategoriaId,
        [FromForm] bool Reclutando,
        IFormFile imagen = null)
    {
        try
        {
            string imagenUrl = null;
            
            if (imagen != null && imagen.Length > 0)
            {
                var extension = Path.GetExtension(imagen.FileName).ToLower();
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
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
                new SqlParameter("@ImagenURL", imagenUrl ?? (object)DBNull.Value),
                new SqlParameter("@Reclutando", Reclutando)
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
        // Eliminar imágenes de la carpeta primero
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "servicios", id.ToString());
        if (Directory.Exists(uploadsFolder))
            Directory.Delete(uploadsFolder, true);
        
        SqlParameter[] p = { new SqlParameter("@Id", id) };
        await _db.ExecuteNonQueryAsync("EliminarServicio", p);
        return Ok(new { mensaje = "Servicio eliminado exitosamente" });
    }
    catch (SqlException ex)
    {
        // Capturar error específico de foreign key
        if (ex.Message.Contains("REFERENCE constraint"))
        {
            return BadRequest(new { error = "No se puede eliminar el servicio porque tiene calificaciones asociadas. Por favor, elimina las calificaciones primero." });
        }
        return StatusCode(500, new { error = ex.Message });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = ex.Message });
    }
}

    [HttpPost("postular")]
    public async Task<IActionResult> Postular([FromForm] int servicioId, [FromForm] int prestadorId, [FromForm] string mensaje = "", [FromForm] int cvId = 0)
    {
        try 
        {
            SqlParameter[] p = { 
                new SqlParameter("@ServicioID", servicioId), 
                new SqlParameter("@PrestadorID", prestadorId),
                new SqlParameter("@Mensaje", string.IsNullOrEmpty(mensaje) ? DBNull.Value : (object)mensaje),
                new SqlParameter("@CvId", cvId == 0 ? DBNull.Value : (object)cvId)
            };
            await _db.ExecuteNonQueryAsync("CrearPostulacion", p);
            return Ok(new { mensaje = "Postulación enviada exitosamente" });
        } 
        catch (SqlException ex) when (ex.Number == 50000)
        { 
            return BadRequest(new { error = ex.Message }); 
        }
        catch (Exception ex) 
        { 
            return BadRequest(new { error = ex.Message }); 
        }
    }

    [HttpGet("mis-servicios/{usuarioId}")]
    public async Task<IActionResult> GetMisServicios(int usuarioId)
    {
        try
        {
            SqlParameter[] p = { new SqlParameter("@UsuarioID", usuarioId) };
            DataTable dt = await _db.ExecuteQueryAsync("ObtenerServiciosPorUsuario", p);

            var lista = dt.AsEnumerable().Select(row => new ServicioAlertaDto
            {
                Id = row.Field<int>("Id"),
                Titulo = row.Field<string>("Titulo") ?? "",
                Ubicacion = row.Field<string>("Ubicacion") ?? "",
                Categoria = row.Field<string>("Categoria") ?? "",
                Autor = row.Field<string>("Autor") ?? "",
                FechaRegistro = row.Field<DateTime>("FechaRegistro"),
                Descripcion = row.Field<string>("Descripcion") ?? "",
                UsuarioId = row.Field<int>("UsuarioID"),
                ImagenURL = row.Field<string>("ImagenURL") ?? "/static/images/vacante.jpg",
                Reclutando = row["Reclutando"] != DBNull.Value && Convert.ToBoolean(row["Reclutando"])
            }).ToList();

            return Ok(lista);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using HOPAPI.Data;
using HOPAPI.Models;
using System.Data;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

namespace HOPAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostulacionesController : ControllerBase
{
    private readonly SqlDataAccess _db;
    
    public PostulacionesController(SqlDataAccess db)
    {
        _db = db;
    }

    // Aplicar a un servicio (postularse)
    [HttpPost("aplicar")]
    public async Task<IActionResult> Aplicar([FromForm] int servicioId, [FromForm] int prestadorId, [FromForm] string mensaje = "", [FromForm] int cvId = 0)
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
            return Ok(new { mensaje = "Te has postulado exitosamente al servicio" });
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno: " + ex.Message });
        }
    }

    // Obtener postulaciones recibidas (para el dueño del servicio)
    [HttpGet("recibidas/{usuarioId}")]
    public async Task<IActionResult> GetPostulacionesRecibidas(int usuarioId)
    {
        try
        {
            SqlParameter[] p = { new SqlParameter("@UsuarioID", usuarioId) };
            DataTable dt = await _db.ExecuteQueryAsync("ObtenerPostulacionesRecibidas", p);
            
            var lista = dt.AsEnumerable().Select(row => new
            {
                Id = Convert.ToInt32(row["Id"]),
                ServicioId = Convert.ToInt32(row["ServicioID"]),
                ServicioTitulo = row["ServicioTitulo"]?.ToString() ?? "",
                PrestadorId = Convert.ToInt32(row["PrestadorID"]),
                PrestadorNombre = row["PrestadorNombre"]?.ToString() ?? "",
                Mensaje = row["Mensaje"]?.ToString() ?? "",
                Estado = row["Estado"]?.ToString() ?? "pendiente",
                FechaPostulacion = Convert.ToDateTime(row["FechaPostulacion"]),
                Leida = row["Leida"] != DBNull.Value && Convert.ToBoolean(row["Leida"]),
                CvRuta = row["CvRuta"]?.ToString() ?? ""
            }).ToList();
            
            return Ok(lista);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Obtener postulaciones enviadas por un usuario
    [HttpGet("enviadas/{usuarioId}")]
    public async Task<IActionResult> GetPostulacionesEnviadas(int usuarioId)
    {
        try
        {
            SqlParameter[] p = { new SqlParameter("@UsuarioID", usuarioId) };
            DataTable dt = await _db.ExecuteQueryAsync("ObtenerPostulacionesEnviadas", p);
            
            var lista = dt.AsEnumerable().Select(row => new
            {
                Id = Convert.ToInt32(row["Id"]),
                ServicioId = Convert.ToInt32(row["ServicioID"]),
                ServicioTitulo = row["ServicioTitulo"]?.ToString() ?? "",
                PrestadorId = Convert.ToInt32(row["PrestadorID"]),
                Mensaje = row["Mensaje"]?.ToString() ?? "",
                Estado = row["Estado"]?.ToString() ?? "pendiente",
                FechaPostulacion = Convert.ToDateTime(row["FechaPostulacion"]),
                CvRuta = row["CvRuta"]?.ToString() ?? ""
            }).ToList();
            
            return Ok(lista);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Obtener una postulación específica
    [HttpGet("{postulacionId}")]
    public async Task<IActionResult> GetPostulacion(int postulacionId)
    {
        try
        {
            SqlParameter[] p = { new SqlParameter("@PostulacionID", postulacionId) };
            DataTable dt = await _db.ExecuteQueryAsync("ObtenerPostulacionPorId", p);
            
            if (dt.Rows.Count == 0)
                return NotFound(new { error = "Postulación no encontrada" });
                
            var row = dt.Rows[0];
            var postulacion = new
            {
                Id = Convert.ToInt32(row["Id"]),
                ServicioId = Convert.ToInt32(row["ServicioID"]),
                ServicioTitulo = row["ServicioTitulo"]?.ToString() ?? "",
                PrestadorId = Convert.ToInt32(row["PrestadorID"]),
                PrestadorNombre = row["PrestadorNombre"]?.ToString() ?? "",
                Mensaje = row["Mensaje"]?.ToString() ?? "",
                Estado = row["Estado"]?.ToString() ?? "pendiente",
                FechaPostulacion = Convert.ToDateTime(row["FechaPostulacion"]),
                Leida = row["Leida"] != DBNull.Value && Convert.ToBoolean(row["Leida"]),
                CvRuta = row["CvRuta"]?.ToString() ?? ""
            };
            
            return Ok(postulacion);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Actualizar estado de postulación (aceptar/rechazar)
    [HttpPut("actualizar-estado/{postulacionId}")]
    public async Task<IActionResult> ActualizarEstado(int postulacionId, [FromForm] string estado)
    {
        try
        {
            if (estado != "aceptado" && estado != "rechazado")
                return BadRequest(new { error = "Estado no válido" });
                
            SqlParameter[] p = {
                new SqlParameter("@PostulacionID", postulacionId),
                new SqlParameter("@Estado", estado)
            };
            await _db.ExecuteNonQueryAsync("ActualizarEstadoPostulacion", p);
            
            return Ok(new { mensaje = $"Postulación {estado} correctamente" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Marcar postulación como leída
    [HttpPut("marcar-leida/{postulacionId}")]
    public async Task<IActionResult> MarcarLeida(int postulacionId)
    {
        try
        {
            SqlParameter[] p = { new SqlParameter("@PostulacionID", postulacionId) };
            await _db.ExecuteNonQueryAsync("MarcarPostulacionLeida", p);
            return Ok(new { mensaje = "Postulación marcada como leída" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
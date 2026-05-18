using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using HOPAPI.Data;
using System.Data;
using System.Threading.Tasks;
using System;
using System.Linq;

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

    [HttpPost("aplicar")]
    public async Task<IActionResult> Aplicar([FromForm] int servicioId, [FromForm] int prestadorId, [FromForm] string mensaje, [FromForm] int cvId)
    {
        try
        {
            SqlParameter[] p = {
                new SqlParameter("@ServicioID", servicioId),
                new SqlParameter("@PrestadorID", prestadorId),
                new SqlParameter("@Mensaje", string.IsNullOrEmpty(mensaje) ? DBNull.Value : (object)mensaje),
                new SqlParameter("@CvId", cvId)
            };

            DataTable dt = await _db.ExecuteQueryAsync("CrearPostulacion", p);
            
            if (dt.Rows.Count > 0 && dt.Rows[0]["Exito"] != DBNull.Value && Convert.ToInt32(dt.Rows[0]["Exito"]) == 1)
            {
                return Ok(new { mensaje = dt.Rows[0]["Mensaje"]?.ToString() ?? "Postulación enviada correctamente" });
            }
            else
            {
                string error = dt.Rows.Count > 0 ? dt.Rows[0]["Mensaje"]?.ToString() ?? "Error al postularse" : "Error al postularse";
                return BadRequest(new { error = error });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

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
                Estado = row["Estado"]?.ToString() ?? "",
                FechaPostulacion = Convert.ToDateTime(row["FechaPostulacion"]),
                Leida = row["Leida"] != DBNull.Value && Convert.ToBoolean(row["Leida"]),
                CvId = row["CvId"] != DBNull.Value ? Convert.ToInt32(row["CvId"]) : (int?)null,
                CvRuta = row["CvRuta"]?.ToString() ?? "",
                CvNombre = row["CvNombre"]?.ToString() ?? ""
            }).ToList();
            
            return Ok(lista);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("mis-postulaciones/{prestadorId}")]
    public async Task<IActionResult> GetMisPostulaciones(int prestadorId)
    {
        try
        {
            SqlParameter[] p = { new SqlParameter("@PrestadorID", prestadorId) };
            DataTable dt = await _db.ExecuteQueryAsync("ObtenerMisPostulaciones", p);
            
            var lista = dt.AsEnumerable().Select(row => new
            {
                Id = Convert.ToInt32(row["Id"]),
                ServicioId = Convert.ToInt32(row["ServicioID"]),
                ServicioTitulo = row["ServicioTitulo"]?.ToString() ?? "",
                AutorId = Convert.ToInt32(row["AutorId"]),
                AutorNombre = row["AutorNombre"]?.ToString() ?? "",
                Mensaje = row["Mensaje"]?.ToString() ?? "",
                Estado = row["Estado"]?.ToString() ?? "",
                FechaPostulacion = Convert.ToDateTime(row["FechaPostulacion"])
            }).ToList();
            
            return Ok(lista);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("actualizar-estado/{postulacionId}")]
    public async Task<IActionResult> ActualizarEstado(int postulacionId, [FromForm] string estado)
    {
        try
        {
            SqlParameter[] p = {
                new SqlParameter("@PostulacionId", postulacionId),
                new SqlParameter("@Estado", estado)
            };
            await _db.ExecuteNonQueryAsync("ActualizarEstadoPostulacion", p);
            
            return Ok(new { mensaje = "Estado actualizado correctamente" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("marcar-leida/{postulacionId}")]
    public async Task<IActionResult> MarcarLeida(int postulacionId)
    {
        try
        {
            SqlParameter[] p = { new SqlParameter("@PostulacionId", postulacionId) };
            await _db.ExecuteNonQueryAsync("MarcarPostulacionLeida", p);
            return Ok(new { mensaje = "Postulación marcada como leída" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
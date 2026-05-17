using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using HOPAPI.Data;
using System;
using System.Threading.Tasks;
using System.Data;
using HOPAPI.Models;
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

    // POST: api/Postulaciones/aplicar
    [HttpPost("aplicar")]
    public async Task<IActionResult> Postularse(int servicioId, int prestadorId)
    {
        try
        {
            SqlParameter[] parameters = {
                new SqlParameter("@ServicioID", servicioId),
                new SqlParameter("@PrestadorID", prestadorId)
            };

            // Ejecutamos el SP que creamos antes
            await _db.ExecuteNonQueryAsync("CrearPostulacion", parameters);
            
            return Ok(new { mensaje = "Te has postulado exitosamente al servicio." });
        }
        catch (SqlException ex)
        {
            // Captura los RAISERROR que pusimos en el SQL (ej: "Ya te has postulado")
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno: " + ex.Message });
        }
    }

    [HttpGet("servicio/{servicioId}")]
public async Task<IActionResult> ListarPorServicio(int servicioId)
{
    try
    {
        SqlParameter[] parameters = {
            new SqlParameter("@ServicioID", servicioId)
        };

        // 1. Ejecutamos el SP con el nombre correcto que tienes en SQL
        DataTable dt = await _db.ExecuteQueryAsync("ObtenerPostulacionesPorServicio", parameters);
        
        // 2. MAPEADO MANUAL (Esto soluciona el error System.NotSupportedException)
        var lista = dt.AsEnumerable().Select(row => new PostulacionDetalleDto
        {
            PostulacionID = Convert.ToInt32(row["PostulacionID"]),
            Fecha = Convert.ToDateTime(row["fecha"]),
            Estado = row["estado"]?.ToString() ?? "",
            UsuarioNombre = row["UsuarioNombre"]?.ToString() ?? "",
            NombreCompleto = row["NombreCompleto"]?.ToString() ?? "",
            Telefono = row["Telefono"]?.ToString() ?? "",
            FotoURL = row["FotoURL"]?.ToString() ?? ""
        }).ToList();

        return Ok(lista);
    }
    catch (Exception ex)
    {
        return BadRequest(new { error = "Error al obtener postulaciones", detalle = ex.Message });
    }
}
}
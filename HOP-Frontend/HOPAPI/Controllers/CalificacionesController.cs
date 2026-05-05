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
public class CalificacionesController : ControllerBase
{
    private readonly SqlDataAccess _db;
    public CalificacionesController(SqlDataAccess db) => _db = db;

    [HttpPost("crear")]
    public async Task<IActionResult> Crear(
        [FromForm] int servicioId, 
        [FromForm] int calificadorId, 
        [FromForm] int calificadoId, 
        [FromForm] int puntuacion, 
        [FromForm] string comentario)
    {
        Console.WriteLine($"=== CALIFICACION RECIBIDA ===");
        Console.WriteLine($"ServicioId: {servicioId}");
        Console.WriteLine($"CalificadorId: {calificadorId}");
        Console.WriteLine($"CalificadoId: {calificadoId}");
        Console.WriteLine($"Puntuacion: {puntuacion}");
        
        if (puntuacion < 1 || puntuacion > 5)
            return BadRequest(new { error = "La puntuacion debe ser entre 1 y 5" });

        try
        {
            SqlParameter[] p = {
                new SqlParameter("@ServicioID", servicioId),
                new SqlParameter("@CalificadorID", calificadorId),
                new SqlParameter("@CalificadoID", calificadoId),
                new SqlParameter("@Puntuacion", puntuacion),
                new SqlParameter("@Comentario", comentario ?? "")
            };
            
            await _db.ExecuteNonQueryAsync("CrearCalificacion", p);
            Console.WriteLine("Calificacion guardada exitosamente");
            return Ok(new { mensaje = "Calificacion enviada correctamente" });
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"SQL Error: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, new { error = "Error al enviar calificacion: " + ex.Message });
        }
    }
 
    [HttpGet("mis-calificaciones/{usuarioId}")]
    public async Task<IActionResult> GetMisCalificaciones(int usuarioId)
    {
        try
        {
            SqlParameter[] p = { new SqlParameter("@UsuarioID", usuarioId) };
            DataTable dt = await _db.ExecuteQueryAsync("ObtenerCalificacionesUsuario", p);

            var lista = dt.AsEnumerable().Select(row => new
            {
                Id = Convert.ToInt32(row["Id"]),
                ServicioId = Convert.ToInt32(row["ServicioID"]),
                ServicioTitulo = row["ServicioTitulo"]?.ToString() ?? "",
                CalificadorId = Convert.ToInt32(row["CalificadorID"]),
                CalificadorNombre = row["CalificadorNombre"]?.ToString() ?? "",
                Puntuacion = Convert.ToInt32(row["Puntuacion"]),
                Comentario = row["Comentario"]?.ToString() ?? "",
                Fecha = Convert.ToDateTime(row["Fecha"])
            }).ToList();

            return Ok(lista);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("estadisticas/{usuarioId}")]
    public async Task<IActionResult> GetEstadisticas(int usuarioId)
    {
        try
        {
            SqlParameter[] p = { new SqlParameter("@UsuarioID", usuarioId) };
            DataTable dt = await _db.ExecuteQueryAsync("ObtenerEstadisticasCalificaciones", p);

            if (dt.Rows.Count > 0 && dt.Rows[0]["TotalCalificaciones"] != DBNull.Value)
            {
                var row = dt.Rows[0];
                var total = Convert.ToInt32(row["TotalCalificaciones"]);
                var promedio = total > 0 ? Math.Round(Convert.ToDouble(row["Promedio"]), 1) : 0;
                
                return Ok(new
                {
                    total = total,
                    promedio = promedio,
                    estrellas5 = Convert.ToInt32(row["Estrellas5"]),
                    estrellas4 = Convert.ToInt32(row["Estrellas4"]),
                    estrellas3 = Convert.ToInt32(row["Estrellas3"]),
                    estrellas2 = Convert.ToInt32(row["Estrellas2"]),
                    estrellas1 = Convert.ToInt32(row["Estrellas1"])
                });
            }
            return Ok(new { total = 0, promedio = 0, estrellas5 = 0, estrellas4 = 0, estrellas3 = 0, estrellas2 = 0, estrellas1 = 0 });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GetEstadisticas: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
    }
}
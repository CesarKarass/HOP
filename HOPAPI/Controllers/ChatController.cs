using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using HOPAPI.Data;
using HOPAPI.Models;
using System.Data;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace HOPAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly SqlDataAccess _db;
    public ChatController(SqlDataAccess db) => _db = db;

    [HttpPost("enviar")]
    public async Task<IActionResult> Enviar(int emisorId, int receptorId, string mensaje)
    {
        // 1. Obtener ID de conversación
        SqlParameter[] p1 = { new SqlParameter("@User1", emisorId), new SqlParameter("@User2", receptorId) };
        DataTable dt = await _db.ExecuteQueryAsync("ObtenerConversacion", p1);
        int convId = (int)dt.Rows[0]["id"];

        // 2. Enviar mensaje
        SqlParameter[] p2 = {
            new SqlParameter("@ConversacionID", convId),
            new SqlParameter("@EmisorID", emisorId),
            new SqlParameter("@ReceptorID", receptorId),
            new SqlParameter("@Mensaje", mensaje)
        };
        await _db.ExecuteNonQueryAsync("EnviarMensaje", p2);
        return Ok("Mensaje enviado");
    }

   [HttpGet("notificaciones/{usuarioId}")]
public async Task<IActionResult> GetNotificaciones(int usuarioId)
{
    try
    {
        SqlParameter[] p = { new SqlParameter("@UsuarioID", usuarioId) };
        
        // Ejecutamos el SP
        DataTable dt = await _db.ExecuteQueryAsync("ObtenerNotificaciones", p);

        // MAPEADO: Convertimos las filas de la tabla en una lista de NotificacionDto
        var lista = dt.AsEnumerable().Select(row => new NotificacionDto
        {
            Id = Convert.ToInt32(row["id"]),
            Contenido = row["contenido"]?.ToString() ?? ""
        }).ToList();

        return Ok(lista);
    }
    catch (Exception ex)
    {
        return BadRequest(new { error = "No se pudieron obtener las notificaciones", detalle = ex.Message });
    }
}
}
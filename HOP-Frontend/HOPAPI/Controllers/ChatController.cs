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
public class ChatController : ControllerBase
{
    private readonly SqlDataAccess _db;
    public ChatController(SqlDataAccess db) => _db = db;

    [HttpPost("enviar")]
    public async Task<IActionResult> Enviar([FromForm] int emisorId, [FromForm] int receptorId, [FromForm] string mensaje)
    {
        if (string.IsNullOrWhiteSpace(mensaje))
            return BadRequest(new { error = "El mensaje no puede estar vacio" });

        try
        {
            SqlParameter[] p1 = { 
                new SqlParameter("@User1", emisorId), 
                new SqlParameter("@User2", receptorId) 
            };
            DataTable dt = await _db.ExecuteQueryAsync("ObtenerConversacion", p1);
            
            if (dt.Rows.Count == 0)
                return BadRequest(new { error = "Error al obtener conversacion" });
                
            int convId = (int)dt.Rows[0]["id"];

            SqlParameter[] p2 = {
                new SqlParameter("@ConversacionID", convId),
                new SqlParameter("@EmisorID", emisorId),
                new SqlParameter("@ReceptorID", receptorId),
                new SqlParameter("@Mensaje", mensaje)
            };
            await _db.ExecuteNonQueryAsync("EnviarMensaje", p2);
            
            return Ok(new { mensaje = "Mensaje enviado correctamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error al enviar mensaje: " + ex.Message });
        }
    }

    [HttpGet("notificaciones/{usuarioId}")]
    public async Task<IActionResult> GetNotificaciones(int usuarioId)
    {
        try
        {
            SqlParameter[] p = { new SqlParameter("@UsuarioID", usuarioId) };
            DataTable dt = await _db.ExecuteQueryAsync("ObtenerNotificaciones", p);

            var notificaciones = dt.AsEnumerable().Select(row => new NotificacionCompletaDto
            {
                Id = Convert.ToInt32(row["Id"]),
                Contenido = row["Contenido"]?.ToString() ?? "",
                Tipo = row["Tipo"]?.ToString() ?? "sistema",
                Leida = row["Leida"] != DBNull.Value && Convert.ToBoolean(row["Leida"]),
                FechaCreacion = row["FechaCreacion"] != DBNull.Value ? Convert.ToDateTime(row["FechaCreacion"]) : DateTime.Now,
                ReferenciaId = row["ReferenciaID"] != DBNull.Value ? Convert.ToInt32(row["ReferenciaID"]) : (int?)null
            }).ToList();
            
            // Agregar postulaciones recibidas (si el usuario es dueño de servicios)
            SqlParameter[] p2 = { new SqlParameter("@UsuarioID", usuarioId) };
            DataTable dtPostulaciones = await _db.ExecuteQueryAsync("ObtenerPostulacionesRecibidas", p2);
            
            var postulacionesNotif = dtPostulaciones.AsEnumerable().Select(row => new NotificacionCompletaDto
            {
                Id = Convert.ToInt32(row["Id"]) + 100000,
                Contenido = $"Nueva postulación de {row["PrestadorNombre"]} para '{row["ServicioTitulo"]}'",
                Tipo = "postulacion",
                Leida = row["Leida"] != DBNull.Value && Convert.ToBoolean(row["Leida"]),
                FechaCreacion = Convert.ToDateTime(row["FechaPostulacion"]),
                ReferenciaId = Convert.ToInt32(row["Id"])
            }).ToList();
            
            var todas = notificaciones.Concat(postulacionesNotif)
                .OrderByDescending(n => n.FechaCreacion)
                .ToList();
            
            return Ok(todas);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "No se pudieron obtener las notificaciones", detalle = ex.Message });
        }
    }

    [HttpGet("mensajes-recibidos/{usuarioId}")]
    public async Task<IActionResult> GetMensajesRecibidos(int usuarioId)
    {
        try
        {
            SqlParameter[] p = { new SqlParameter("@UsuarioID", usuarioId) };
            DataTable dt = await _db.ExecuteQueryAsync("ObtenerMensajesRecibidos", p);

            var lista = dt.AsEnumerable().Select(row => new
            {
                Id = Convert.ToInt32(row["Id"]),
                EmisorId = Convert.ToInt32(row["EmisorID"]),
                EmisorNombre = row["EmisorNombre"]?.ToString() ?? "",
                Mensaje = row["Mensaje"]?.ToString() ?? "",
                FechaEnvio = Convert.ToDateTime(row["FechaEnvio"]),
                Leido = row["Leido"] != DBNull.Value && Convert.ToBoolean(row["Leido"])
            }).ToList();

            return Ok(lista);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Error al obtener mensajes: " + ex.Message });
        }
    }

    [HttpGet("conversacion/{usuarioId}/{otroUsuarioId}")]
    public async Task<IActionResult> GetConversacion(int usuarioId, int otroUsuarioId)
    {
        try
        {
            SqlParameter[] p = { 
                new SqlParameter("@User1", usuarioId), 
                new SqlParameter("@User2", otroUsuarioId) 
            };
            DataTable dtConv = await _db.ExecuteQueryAsync("ObtenerConversacion", p);
            
            if (dtConv.Rows.Count == 0)
                return Ok(new List<object>());
                
            int conversacionId = (int)dtConv.Rows[0]["id"];
            
            SqlParameter[] pMensajes = { new SqlParameter("@ConversacionID", conversacionId) };
            DataTable dtMensajes = await _db.ExecuteQueryAsync("ObtenerMensajesPorConversacion", pMensajes);
            
            var lista = dtMensajes.AsEnumerable().Select(row => new
            {
                Id = Convert.ToInt32(row["Id"]),
                EmisorId = Convert.ToInt32(row["EmisorID"]),
                EmisorNombre = row["EmisorNombre"]?.ToString() ?? "",
                Mensaje = row["Mensaje"]?.ToString() ?? "",
                FechaEnvio = Convert.ToDateTime(row["FechaEnvio"]),
                EsPropio = Convert.ToInt32(row["EmisorID"]) == usuarioId
            }).ToList();
            
            return Ok(lista);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Error al obtener conversacion: " + ex.Message });
        }
    }

    [HttpPut("marcar-leida/{mensajeId}")]
    public async Task<IActionResult> MarcarLeida(int mensajeId)
    {
        try
        {
            SqlParameter[] p = { new SqlParameter("@MensajeID", mensajeId) };
            await _db.ExecuteNonQueryAsync("MarcarMensajeLeido", p);
            return Ok(new { mensaje = "Mensaje marcado como leido" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Error al marcar mensaje: " + ex.Message });
        }
    }

    [HttpPut("marcar-calificacion-leida/{calificacionId}")]
    public async Task<IActionResult> MarcarCalificacionLeida(int calificacionId)
    {
        try
        {
            SqlParameter[] p = { new SqlParameter("@CalificacionID", calificacionId) };
            await _db.ExecuteNonQueryAsync("MarcarCalificacionLeida", p);
            return Ok(new { mensaje = "Calificacion marcada como leida" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Error al marcar calificacion: " + ex.Message });
        }
    }
}

public class NotificacionCompletaDto
{
    public int Id { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? ReferenciaId { get; set; }
    public int? EmisorId { get; set; }
    public string EmisorNombre { get; set; } = string.Empty;
    public string MensajeContenido { get; set; } = string.Empty;
    public int? Puntuacion { get; set; }
    public string ComentarioCalificacion { get; set; } = string.Empty;
    public string CalificadorNombre { get; set; } = string.Empty;
}
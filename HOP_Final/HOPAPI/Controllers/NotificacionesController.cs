using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using HOPAPI.Data;
using System;
using System.Threading.Tasks;

namespace HOPAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificacionesController : ControllerBase
    {
        private readonly SqlDataAccess _db;

        public NotificacionesController(SqlDataAccess db)
        {
            _db = db;
        }

        // Endpoint para eliminar una notificación
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarNotificacion(int id)
        {
            try
            {
                SqlParameter[] p = { new SqlParameter("@Id", id) };
                await _db.ExecuteNonQueryAsync("EliminarNotificacion", p);
                return Ok(new { mensaje = "Notificación eliminada correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
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
public class ServiciosController : ControllerBase
{
    private readonly SqlDataAccess _db;
    public ServiciosController(SqlDataAccess db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Crear(string titulo, int usuarioId, string ubicacion, int categoriaId, string descripcion)
    {
        SqlParameter[] p = {
            new SqlParameter("@Titulo", titulo),
            new SqlParameter("@UsuarioID", usuarioId),
            new SqlParameter("@Ubicacion", ubicacion),
            new SqlParameter("@CategoriaID", categoriaId),
            new SqlParameter("@Descripcion", descripcion)
        };
        await _db.ExecuteNonQueryAsync("CrearServicio", p);
        return Ok("Servicio publicado");
    }

[HttpPost("buscar")] // Cambiamos a POST porque los navegadores no permiten Body en GET por estándar
public async Task<IActionResult> Listar([FromBody] FiltroServicioDto filtros)
{
    try
    {
        // 1. Preparamos los parámetros para el SP "ObtenerServicios"
        SqlParameter[] parameters = {
            new SqlParameter("@Busqueda", (object?)filtros.Busqueda ?? DBNull.Value),
            new SqlParameter("@CategoriaID", (object?)filtros.CategoriaId ?? DBNull.Value)
        };

        // 2. Ejecutamos la consulta
        DataTable dt = await _db.ExecuteQueryAsync("ObtenerServicios", parameters);
        
        // 3. Mapeamos el DataTable a tu lista de DTOs para evitar el error de serialización
        var lista = dt.AsEnumerable().Select(row => new ServicioAlertaDto
        {
            Id = row.Field<int>("id"),
            Titulo = row.Field<string>("titulo") ?? "",
            Ubicacion = row.Field<string>("ubicacion") ?? "",
            Categoria = row.Field<string>("Categoria") ?? "",
            Autor = row.Field<string>("Autor") ?? "",
            FechaRegistro = row.Field<DateTime>("fechaRegistro")
        }).ToList();

        return Ok(lista);
    }
    catch (Exception ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}

    [HttpPost("postular")]
    public async Task<IActionResult> Postular(int servicioId, int prestadorId)
    {
        try {
            SqlParameter[] p = { new SqlParameter("@ServicioID", servicioId), new SqlParameter("@PrestadorID", prestadorId) };
            await _db.ExecuteNonQueryAsync("CrearPostulacion", p);
            return Ok("Postulación enviada");
        } catch (Exception ex) { return BadRequest(ex.Message); }
    }
}
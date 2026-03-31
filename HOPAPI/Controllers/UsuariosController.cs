using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using HOPAPI.Models;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using HOPAPI.Data;


[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly SqlDataAccess _db;
    public UsuariosController(SqlDataAccess db) => _db = db;

    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar(string nombre, int rolId, string email, string password)
    {
        SqlParameter[] p = {
            new SqlParameter("@Nombre", nombre),
            new SqlParameter("@RolID", rolId),
            new SqlParameter("@Email", email),
            new SqlParameter("@Password", password)
        };
        await _db.ExecuteNonQueryAsync("RegistrarUsuario", p);
        return Ok(new { mensaje = "Usuario creado" });
    }

    [HttpPost("login")]
public async Task<IActionResult> Login(string email, string password)
{
    SqlParameter[] p = { new SqlParameter("@Email", email), new SqlParameter("@Password", password) };
    DataTable dt = await _db.ExecuteQueryAsync("Login", p);
    
    if (dt.Rows.Count == 0) return Unauthorized();

    DataRow row = dt.Rows[0];
    var user = new UsuarioLoginResult {
        Id = (int)row["id"],
        Name = row["name"].ToString()!,
        Rol = row["Rol"].ToString()!,
        NombreCompleto = row["NombreCompleto"].ToString()!
    };

    return Ok(user);
}

    [HttpPut("perfil")]
    public async Task<IActionResult> ActualizarPerfil(int usuarioId, string nombreCompleto, string bio, string foto, string tel)
    {
        SqlParameter[] p = {
            new SqlParameter("@UsuarioID", usuarioId),
            new SqlParameter("@NombreCompleto", nombreCompleto),
            new SqlParameter("@Bio", bio),
            new SqlParameter("@Foto", foto),
            new SqlParameter("@Tel", tel)
        };
        await _db.ExecuteNonQueryAsync("ActualizarPerfil", p);
        return Ok("Perfil actualizado");
    }
}
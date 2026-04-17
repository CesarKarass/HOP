using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using HOPAPI.Models;
using HOPAPI.Data;
using System.Data;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Google.Apis.Auth;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace HOPAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly SqlDataAccess _db;
        private readonly IConfiguration _config;

        public UsuariosController(SqlDataAccess db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // DTO para recibir el token de Google desde Scalar/Frontend
        public class GoogleLoginRequest
        {
            public string Token { get; set; } = string.Empty;
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Token))
                return BadRequest(new { mensaje = "El token es requerido" });

            try
            {
                // 1. Validar el token con Google
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Token);

                // 2. Preparar parámetros para el Stored Procedure
                // Usamos SqlDbType para asegurar que ADO.NET mapee correctamente los tipos
                SqlParameter[] p = {
                    new SqlParameter("@Email", SqlDbType.NVarChar) { Value = payload.Email },
                    new SqlParameter("@Nombre", SqlDbType.NVarChar) { Value = payload.Name }
                };

                // 3. Ejecutar Upsert en SQL Server
                DataTable dt = await _db.ExecuteQueryAsync("UpsertUsuarioGoogle", p);

                if (dt == null || dt.Rows.Count == 0)
                    return BadRequest(new { mensaje = "Error al procesar el usuario en la base de datos" });

                DataRow row = dt.Rows[0];

                // 4. Generar el JWT propio para CLISA
                var tokenPropio = GenerarTokenJWT(row);

                return Ok(new
                {
                    token = tokenPropio,
                    user = new
                    {
                        id = row["id"],
                        name = row["name"],
                        email = row["Email"],
                        rol = row["Rol"]
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Token de Google no válido o error de DB", error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            SqlParameter[] p = { 
                new SqlParameter("@Email", email), 
                new SqlParameter("@Password", password) 
            };
            
            DataTable dt = await _db.ExecuteQueryAsync("Login", p);
            
            if (dt == null || dt.Rows.Count == 0) 
                return Unauthorized(new { mensaje = "Credenciales incorrectas" });

            DataRow row = dt.Rows[0];
            var token = GenerarTokenJWT(row);

            return Ok(new {
                token = token,
                user = new {
                    id = (int)row["id"],
                    name = row["name"].ToString(),
                    rol = row["Rol"].ToString()
                }
            });
        }

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
            return Ok(new { mensaje = "Usuario creado con éxito" });
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
            return Ok(new { mensaje = "Perfil actualizado" });
        }

        // Método privado para centralizar la creación de Tokens
        private string GenerarTokenJWT(DataRow row)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, row["id"].ToString()!),
                new Claim(JwtRegisteredClaimNames.Email, row["Email"].ToString()!),
                new Claim(ClaimTypes.Role, row["Rol"].ToString()!),
                new Claim("name", row["name"].ToString()!)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using HOPAPI.Models;
using System.Data;
using System.Threading.Tasks;
using HOPAPI.Data;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace HOPAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly SqlDataAccess _db;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public UsuariosController(SqlDataAccess db, IWebHostEnvironment env, IConfiguration config)
        {
            _db = db;
            _env = env;
            _config = config;
        }

        // DTO para recibir el token de Google
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
                Console.WriteLine("=== GOOGLE LOGIN START ===");
                Console.WriteLine($"Token recibido (primeros 50 chars): {request.Token.Substring(0, Math.Min(50, request.Token.Length))}...");
                
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Token);
                
                Console.WriteLine($"Email validado: {payload.Email}");
                Console.WriteLine($"Nombre validado: {payload.Name}");

                SqlParameter[] p = {
                    new SqlParameter("@Email", payload.Email),
                    new SqlParameter("@Nombre", payload.Name),
                    new SqlParameter("@Telefono", DBNull.Value),
                    new SqlParameter("@Ubicacion", DBNull.Value)
                };

                DataTable dt = await _db.ExecuteQueryAsync("UpsertUsuarioGoogle", p);

                if (dt == null || dt.Rows.Count == 0)
                {
                    Console.WriteLine("ERROR: No se encontraron datos del usuario en la BD");
                    return BadRequest(new { mensaje = "Error al procesar el usuario en la base de datos" });
                }

                DataRow row = dt.Rows[0];
                Console.WriteLine($"Usuario encontrado/creado - ID: {row["Id"]}, Email: {row["Email"]}");

                var tokenPropio = GenerarTokenJWT(row);

                Console.WriteLine("=== GOOGLE LOGIN SUCCESS ===");
                
                return Ok(new
                {
                    token = tokenPropio,
                    user = new
                    {
                        id = Convert.ToInt32(row["Id"]),
                        name = row["name"]?.ToString() ?? "",
                        email = row["Email"]?.ToString() ?? "",
                        rol = row["Rol"]?.ToString() ?? "usuario"
                    }
                });
            }
            catch (InvalidJwtException ex)
            {
                Console.WriteLine($"ERROR InvalidJwtException: {ex.Message}");
                return BadRequest(new { mensaje = "Token de Google inválido", error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return BadRequest(new { mensaje = "Error al procesar el login con Google", error = ex.Message });
            }
        }

        // NUEVO ENDPOINT: Recibe el token como form-urlencoded (para compatibilidad con el frontend)
        [HttpPost("google-login-alt")]
        public async Task<IActionResult> GoogleLoginAlt([FromForm] string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest(new { mensaje = "El token es requerido" });

            try
            {
                Console.WriteLine("=== GOOGLE LOGIN ALT START ===");
                Console.WriteLine($"Token recibido length: {token.Length}");
                
                var payload = await GoogleJsonWebSignature.ValidateAsync(token);
                
                Console.WriteLine($"Email validado: {payload.Email}");
                Console.WriteLine($"Nombre validado: {payload.Name}");

                SqlParameter[] p = {
                    new SqlParameter("@Email", payload.Email),
                    new SqlParameter("@Nombre", payload.Name),
                    new SqlParameter("@Telefono", DBNull.Value),
                    new SqlParameter("@Ubicacion", DBNull.Value)
                };

                DataTable dt = await _db.ExecuteQueryAsync("UpsertUsuarioGoogle", p);

                if (dt == null || dt.Rows.Count == 0)
                {
                    Console.WriteLine("ERROR: No se encontraron datos del usuario en la BD");
                    return BadRequest(new { mensaje = "Error al procesar el usuario en la base de datos" });
                }

                DataRow row = dt.Rows[0];
                Console.WriteLine($"Usuario encontrado/creado - ID: {row["Id"]}, Email: {row["Email"]}");

                var tokenPropio = GenerarTokenJWT(row);

                Console.WriteLine("=== GOOGLE LOGIN ALT SUCCESS ===");
                
                return Ok(new
                {
                    token = tokenPropio,
                    user = new
                    {
                        id = Convert.ToInt32(row["Id"]),
                        name = row["name"]?.ToString() ?? "",
                        email = row["Email"]?.ToString() ?? "",
                        rol = row["Rol"]?.ToString() ?? "usuario"
                    }
                });
            }
            catch (InvalidJwtException ex)
            {
                Console.WriteLine($"ERROR InvalidJwtException: {ex.Message}");
                return BadRequest(new { mensaje = "Token de Google inválido", error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return BadRequest(new { mensaje = "Error al procesar el login con Google", error = ex.Message });
            }
        }

        // Endpoint de depuración para Google Login
        [HttpPost("google-login-debug")]
        public async Task<IActionResult> GoogleLoginDebug([FromBody] GoogleLoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Token))
                return BadRequest(new { mensaje = "El token es requerido", success = false });

            try
            {
                Console.WriteLine("=== GOOGLE LOGIN DEBUG START ===");
                
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Token);
                
                Console.WriteLine($"Email: {payload.Email}");
                Console.WriteLine($"Name: {payload.Name}");
                
                return Ok(new { 
                    success = true,
                    email = payload.Email,
                    name = payload.Name,
                    givenName = payload.GivenName,
                    familyName = payload.FamilyName,
                    picture = payload.Picture
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                return BadRequest(new { 
                    success = false, 
                    mensaje = "Error al validar token de Google", 
                    error = ex.Message
                });
            }
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromForm] UsuarioRegistroDto usuario)
        {
            if (usuario == null)
            {
                return BadRequest(new { error = "Datos de usuario no proporcionados" });
            }

            // Validar que el email no esté ya registrado
            SqlParameter[] checkParams = {
                new SqlParameter("@Email", usuario.Email)
            };
            DataTable checkDt = await _db.ExecuteQueryAsync("VerificarEmailExistente", checkParams);
            
            if (checkDt.Rows.Count > 0 && Convert.ToInt32(checkDt.Rows[0]["Existe"]) > 0)
            {
                return BadRequest(new { error = "El correo electrónico ya está registrado" });
            }

            SqlParameter[] p = {
                new SqlParameter("@Nombre", usuario.Nombre),
                new SqlParameter("@RolID", usuario.RolID),
                new SqlParameter("@Email", usuario.Email),
                new SqlParameter("@Password", usuario.Password)
            };
            
            await _db.ExecuteNonQueryAsync("RegistrarUsuario", p);
            return Ok(new { mensaje = "Usuario creado exitosamente", email = usuario.Email });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password)
        {
            try
            {
                Console.WriteLine($"=== LOGIN NORMAL ===");
                Console.WriteLine($"Email: '{email}'");
                Console.WriteLine($"Password: '{password}'");
                
                // Primero verificar si el email existe
                SqlParameter[] checkParams = { new SqlParameter("@Email", email) };
                DataTable checkDt = await _db.ExecuteQueryAsync("VerificarEmailExistente", checkParams);
                bool emailExiste = checkDt.Rows.Count > 0 && Convert.ToInt32(checkDt.Rows[0]["Existe"]) > 0;
                
                if (!emailExiste)
                {
                    Console.WriteLine("El email NO existe en la base de datos");
                    return Unauthorized(new { error = "Correo no registrado" });
                }
                
                Console.WriteLine("Email existe, verificando contraseña...");
                
                // Ahora buscar con email y contraseña
                SqlParameter[] p = { 
                    new SqlParameter("@Email", email), 
                    new SqlParameter("@Password", password) 
                };
                DataTable dt = await _db.ExecuteQueryAsync("Login", p);
                
                Console.WriteLine($"Filas encontradas: {dt.Rows.Count}");
                
                if (dt.Rows.Count == 0) 
                {
                    Console.WriteLine("Contraseña incorrecta");
                    return Unauthorized(new { error = "Contraseña incorrecta" });
                }

                DataRow row = dt.Rows[0];
                var user = new UsuarioLoginResult {
                    Id = (int)row["id"],
                    Name = row["name"].ToString()!,
                    Rol = row["Rol"].ToString()!,
                    NombreCompleto = row["NombreCompleto"].ToString()!
                };

                var token = GenerarTokenJWT(row);
                
                Console.WriteLine($"Login exitoso - Usuario ID: {user.Id}, Nombre: {user.Name}");

                return Ok(new
                {
                    token = token,
                    user = user
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en login: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpPut("perfil")]
        public async Task<IActionResult> ActualizarPerfil([FromForm] int usuarioId, [FromForm] string nombreCompleto, [FromForm] string bio, [FromForm] string foto, [FromForm] string tel)
        {
            string fotoUrl = "";
            
            if (!string.IsNullOrEmpty(foto) && foto.StartsWith("data:image"))
            {
                try
                {
                    var imageParts = foto.Split(',');
                    if (imageParts.Length == 2)
                    {
                        var imageData = imageParts[1];
                        var imageBytes = Convert.FromBase64String(imageData);
                        
                        var fileName = $"user_{usuarioId}_{DateTime.Now.Ticks}.jpg";
                        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "perfiles");
                        
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                        
                        fotoUrl = $"{Request.Scheme}://{Request.Host}/uploads/perfiles/{fileName}";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error guardando imagen: {ex.Message}");
                    fotoUrl = "";
                }
            }
            
            SqlParameter[] p = {
                new SqlParameter("@UsuarioID", usuarioId),
                new SqlParameter("@NombreCompleto", nombreCompleto),
                new SqlParameter("@Bio", bio ?? ""),
                new SqlParameter("@Foto", fotoUrl),
                new SqlParameter("@Tel", tel ?? "")
            };
            await _db.ExecuteNonQueryAsync("ActualizarPerfil", p);
            return Ok(new { mensaje = "Perfil actualizado correctamente", fotoUrl = fotoUrl });
        }

        [HttpPut("actualizar-datos")]
        public async Task<IActionResult> ActualizarDatos([FromForm] int usuarioId, [FromForm] string telefono, [FromForm] string ubicacion)
        {
            SqlParameter[] p = {
                new SqlParameter("@UsuarioID", usuarioId),
                new SqlParameter("@Telefono", telefono ?? ""),
                new SqlParameter("@Ubicacion", ubicacion ?? "")
            };
            
            await _db.ExecuteNonQueryAsync("ActualizarDatosUsuario", p);
            return Ok(new { mensaje = "Datos actualizados correctamente" });
        }

        [HttpGet("perfil/{id}")]
        public async Task<IActionResult> ObtenerPerfil(int id)
        {
            SqlParameter[] p = { new SqlParameter("@UsuarioID", id) };
            DataTable dt = await _db.ExecuteQueryAsync("ObtenerPerfil", p);
            
            if (dt.Rows.Count == 0) 
                return NotFound(new { error = "Usuario no encontrado" });
            
            DataRow row = dt.Rows[0];
            var perfil = new {
                id = (int)row["Id"],
                nombre = row["Nombre"].ToString(),
                nombreCompleto = row["NombreCompleto"].ToString(),
                email = row["Email"].ToString(),
                telefono = row["Telefono"].ToString(),
                ubicacion = row["Ubicacion"]?.ToString(),
                bio = row["Bio"]?.ToString(),
                fotoUrl = row["FotoURL"]?.ToString(),
                habilidades = row["Habilidades"]?.ToString()
            };
            
            return Ok(perfil);
        }

        // Método privado para centralizar la creación de Tokens JWT
        private string GenerarTokenJWT(DataRow row)
        {
            var jwtKey = _config["Jwt:Key"] ?? "mi-clave-secreta-super-segura-para-hop-2024";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var userId = row["Id"] != null ? row["Id"].ToString() : row["id"]?.ToString();
            
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId ?? "0"),
                new Claim(JwtRegisteredClaimNames.Email, row["Email"]?.ToString() ?? ""),
                new Claim(ClaimTypes.Role, row["Rol"]?.ToString() ?? "usuario"),
                new Claim("name", row["name"]?.ToString() ?? ""),
                new Claim("NombreCompleto", row["NombreCompleto"]?.ToString() ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "HOPAPI",
                audience: _config["Jwt:Audience"] ?? "HOPFrontend",
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
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
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;

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
                var tokenPropio = GenerarTokenJWT(row);

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
                return BadRequest(new { mensaje = "Error al procesar el login con Google", error = ex.Message });
            }
        }

        [HttpPost("google-login-alt")]
        public async Task<IActionResult> GoogleLoginAlt([FromForm] string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest(new { mensaje = "El token es requerido" });

            try
            {
                Console.WriteLine("=== GOOGLE LOGIN ALT START ===");
                
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
                    return BadRequest(new { mensaje = "Error al procesar el usuario en la base de datos" });

                DataRow row = dt.Rows[0];
                var tokenPropio = GenerarTokenJWT(row);

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
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                return BadRequest(new { mensaje = "Error al procesar el login con Google", error = ex.Message });
            }
        }

        // Verificar si es usuario de Google
        [HttpGet("es-google-user/{usuarioId}")]
        public async Task<IActionResult> EsGoogleUser(int usuarioId)
        {
            try
            {
                SqlParameter[] p = { new SqlParameter("@UsuarioID", usuarioId) };
                DataTable dt = await _db.ExecuteQueryAsync("VerificarGoogleUser", p);
                
                bool esGoogleUser = dt.Rows.Count > 0 && dt.Rows[0]["EsGoogleUser"] != DBNull.Value && Convert.ToBoolean(dt.Rows[0]["EsGoogleUser"]);
                return Ok(new { esGoogleUser = esGoogleUser });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Agregar contraseña a usuario de Google
        [HttpPost("agregar-password-google")]
        public async Task<IActionResult> AgregarPasswordGoogle([FromForm] int usuarioId, [FromForm] string nuevaPassword)
        {
            if (string.IsNullOrEmpty(nuevaPassword) || nuevaPassword.Length < 4)
                return BadRequest(new { error = "La contraseña debe tener al menos 4 caracteres" });
            
            try
            {
                SqlParameter[] p = {
                    new SqlParameter("@UsuarioID", usuarioId),
                    new SqlParameter("@NuevaPassword", nuevaPassword)
                };
                
                DataTable dt = await _db.ExecuteQueryAsync("AgregarPasswordGoogle", p);
                
                if (dt.Rows.Count > 0 && dt.Rows[0]["Exito"] != DBNull.Value && Convert.ToBoolean(dt.Rows[0]["Exito"]))
                    return Ok(new { mensaje = "Contraseña agregada correctamente" });
                else
                    return BadRequest(new { error = "Error al agregar contraseña" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromForm] UsuarioRegistroDto usuario)
        {
            if (usuario == null)
            {
                return BadRequest(new { error = "Datos de usuario no proporcionados" });
            }

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
            SqlParameter[] p = { 
                new SqlParameter("@Email", email), 
                new SqlParameter("@Password", password) 
            };
            DataTable dt = await _db.ExecuteQueryAsync("Login", p);
            
            if (dt.Rows.Count == 0) 
                return Unauthorized(new { error = "Correo o contraseña incorrectos" });

            DataRow row = dt.Rows[0];
            var user = new UsuarioLoginResult {
                Id = (int)row["id"],
                Name = row["name"].ToString()!,
                Rol = row["Rol"].ToString()!,
                NombreCompleto = row["NombreCompleto"].ToString()!
            };

            var token = GenerarTokenJWT(row);

            return Ok(new
            {
                token = token,
                user = user
            });
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

        // ============================================
        // MÉTODOS PARA GESTIÓN DE CVs
        // ============================================

        [HttpPost("subir-cv")]
        public async Task<IActionResult> SubirCV([FromForm] int usuarioId, [FromForm] string titulo, IFormFile archivo)
        {
            try
            {
                if (archivo == null || archivo.Length == 0)
                    return BadRequest(new { error = "No se seleccionó ningún archivo" });
                
                var extension = Path.GetExtension(archivo.FileName).ToLower();
                if (extension != ".pdf" && extension != ".docx" && extension != ".doc")
                    return BadRequest(new { error = "Solo se permiten archivos PDF, DOC o DOCX" });
                
                if (archivo.Length > 8 * 1024 * 1024)
                    return BadRequest(new { error = "El archivo no puede superar los 8MB" });
                
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "cvs", usuarioId.ToString());
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = $"{DateTime.Now.Ticks}_{archivo.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }
                
                var rutaArchivo = $"/uploads/cvs/{usuarioId}/{uniqueFileName}";
                var tituloFinal = string.IsNullOrEmpty(titulo) ? archivo.FileName : titulo;
                
                SqlParameter[] p = {
                    new SqlParameter("@UsuarioId", usuarioId),
                    new SqlParameter("@Titulo", tituloFinal),
                    new SqlParameter("@NombreArchivo", archivo.FileName),
                    new SqlParameter("@RutaArchivo", rutaArchivo),
                    new SqlParameter("@Tamano", archivo.Length)
                };
                
                await _db.ExecuteNonQueryAsync("InsertarCV", p);
                
                return Ok(new { mensaje = "CV subido correctamente", archivo = rutaArchivo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("mis-cvs/{usuarioId}")]
        public async Task<IActionResult> GetMisCVs(int usuarioId)
        {
            try
            {
                SqlParameter[] p = { new SqlParameter("@UsuarioId", usuarioId) };
                DataTable dt = await _db.ExecuteQueryAsync("ObtenerCVsUsuario", p);
                
                var lista = dt.AsEnumerable().Select(row => new
                {
                    Id = row.Field<int>("Id"),
                    Titulo = row.Field<string>("Titulo"),
                    NombreArchivo = row.Field<string>("NombreArchivo"),
                    RutaArchivo = row.Field<string>("RutaArchivo"),
                    Tamano = row.Field<int>("Tamano"),
                    FechaSubida = row.Field<DateTime>("FechaSubida")
                }).ToList();
                
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("eliminar-cv/{cvId}")]
        public async Task<IActionResult> EliminarCV(int cvId)
        {
            try
            {
                SqlParameter[] pGet = { new SqlParameter("@CvId", cvId) };
                DataTable dt = await _db.ExecuteQueryAsync("ObtenerCVPorId", pGet);
                
                if (dt.Rows.Count > 0)
                {
                    string rutaArchivo = dt.Rows[0]["RutaArchivo"].ToString();
                    var filePath = Path.Combine(_env.WebRootPath, rutaArchivo.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }
                
                SqlParameter[] p = { new SqlParameter("@CvId", cvId) };
                await _db.ExecuteNonQueryAsync("EliminarCV", p);
                
                return Ok(new { mensaje = "CV eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("editar-titulo-cv")]
        public async Task<IActionResult> EditarTituloCV([FromForm] int cvId, [FromForm] string nuevoTitulo)
        {
            try
            {
                SqlParameter[] p = {
                    new SqlParameter("@CvId", cvId),
                    new SqlParameter("@NuevoTitulo", nuevoTitulo)
                };
                await _db.ExecuteNonQueryAsync("EditarTituloCV", p);
                return Ok(new { mensaje = "Título actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ============================================
        // MÉTODO PARA CAMBIAR CONTRASEÑA
        // ============================================

        [HttpPost("cambiar-password")]
        public async Task<IActionResult> CambiarPassword([FromForm] int usuarioId, [FromForm] string passwordActual, [FromForm] string nuevaPassword, [FromForm] string confirmarPassword)
        {
            if (string.IsNullOrEmpty(passwordActual) || string.IsNullOrEmpty(nuevaPassword))
            {
                return BadRequest(new { error = "Todos los campos son obligatorios" });
            }
            
            if (nuevaPassword.Length < 4)
            {
                return BadRequest(new { error = "La nueva contraseña debe tener al menos 4 caracteres" });
            }
            
            if (nuevaPassword != confirmarPassword)
            {
                return BadRequest(new { error = "Las contraseñas no coinciden" });
            }
            
            if (nuevaPassword == passwordActual)
            {
                return BadRequest(new { error = "La nueva contraseña debe ser diferente a la actual" });
            }
            
            SqlParameter[] p = {
                new SqlParameter("@UsuarioID", usuarioId),
                new SqlParameter("@PasswordActual", passwordActual),
                new SqlParameter("@NuevaPassword", nuevaPassword)
            };
            
            DataTable dt = await _db.ExecuteQueryAsync("CambiarPassword", p);
            
            if (dt.Rows.Count > 0 && dt.Rows[0]["Exito"] != DBNull.Value && Convert.ToBoolean(dt.Rows[0]["Exito"]))
            {
                return Ok(new { mensaje = "Contraseña actualizada correctamente" });
            }
            else
            {
                string error = dt.Rows.Count > 0 ? dt.Rows[0]["Error"]?.ToString() ?? "Error al cambiar contraseña" : "Error al cambiar contraseña";
                return BadRequest(new { error = error });
            }
        }

        // ============================================
        // MÉTODO PARA ELIMINAR CUENTA
        // ============================================

        [HttpDelete("eliminar-cuenta")]
public async Task<IActionResult> EliminarCuenta([FromForm] int usuarioId)
{
    try
    {
        Console.WriteLine($"=== ELIMINANDO CUENTA: {usuarioId} ===");
        
        // Verificar que el usuario existe
        SqlParameter[] checkParams = { new SqlParameter("@UsuarioId", usuarioId) };
        DataTable checkDt = await _db.ExecuteQueryAsync("VerificarUsuarioExistente", checkParams);
        
        if (checkDt.Rows.Count == 0)
        {
            return BadRequest(new { error = "Usuario no encontrado" });
        }
        
        // Eliminar CVs físicos
        var cvsFolder = Path.Combine(_env.WebRootPath, "uploads", "cvs", usuarioId.ToString());
        if (Directory.Exists(cvsFolder))
            Directory.Delete(cvsFolder, true);
        
        // Eliminar imágenes de perfil
        var perfilFolder = Path.Combine(_env.WebRootPath, "uploads", "perfiles");
        if (Directory.Exists(perfilFolder))
        {
            var archivosPerfil = Directory.GetFiles(perfilFolder, $"user_{usuarioId}_*");
            foreach (var archivo in archivosPerfil)
                System.IO.File.Delete(archivo);
        }
        
        // Eliminar imágenes de servicios
        var serviciosFolder = Path.Combine(_env.WebRootPath, "uploads", "servicios");
        if (Directory.Exists(serviciosFolder))
        {
            var subcarpetas = Directory.GetDirectories(serviciosFolder);
            foreach (var carpeta in subcarpetas)
            {
                if (carpeta.EndsWith(usuarioId.ToString()))
                    Directory.Delete(carpeta, true);
            }
        }
        
        // Eliminar usuario de la base de datos
        SqlParameter[] p = { new SqlParameter("@UsuarioId", usuarioId) };
        await _db.ExecuteNonQueryAsync("EliminarUsuario", p);
        
        Console.WriteLine("=== CUENTA ELIMINADA CORRECTAMENTE ===");
        return Ok(new { mensaje = "Cuenta eliminada correctamente" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR ELIMINANDO CUENTA: {ex.Message}");
        return StatusCode(500, new { error = ex.Message });
    }
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
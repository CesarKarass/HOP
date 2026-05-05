namespace HOPAPI.Models;

public class UsuarioRegistroDto
{
    public string Nombre { get; set; } = string.Empty;
    public int RolID { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UsuarioLoginResult {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
}
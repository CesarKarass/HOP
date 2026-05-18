using System;
namespace HOPAPI.Models;
public class PostulacionDetalleDto
{
    public int PostulacionID { get; set; }
    public DateTime Fecha { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string UsuarioNombre { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string FotoURL { get; set; } = string.Empty;
}
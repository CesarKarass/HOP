using System;

namespace HOPAPI.Models;
public class ServicioDto {
    public string Titulo { get; set; } = string.Empty;
    public int UsuarioID { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public int CategoriaID { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}

public class ServicioAlertaDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
}

public class FiltroServicioDto
{
    public string? Busqueda { get; set; }
    public int? CategoriaId { get; set; }
}

public class NotificacionDto
{
    public int Id { get; set; }
    public string Contenido { get; set; } = string.Empty;
}
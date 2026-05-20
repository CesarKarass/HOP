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
    public string? Descripcion { get; set; }
    public string ImagenURL { get; set; } = string.Empty;
    public int AutorId { get; set; }
    public bool Reclutando { get; set; } = true;
    public int? UsuarioId { get; set; }  // <-- Agregar esta propiedad
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
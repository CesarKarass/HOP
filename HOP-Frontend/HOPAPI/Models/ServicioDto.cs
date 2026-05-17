using System;

namespace HOPAPI.Models;

public class ServicioDto 
{
    public string Titulo { get; set; } = string.Empty;
    public int UsuarioID { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public int CategoriaID { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int? UsuarioId { get; set; }  // <-- AGREGAR ESTA LÍNEA

}

public class ServicioAlertaDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string ImagenURL { get; set; } = string.Empty;
    public int AutorId { get; set; }
}

public class FiltroServicioDto
{
    public string Busqueda { get; set; } = string.Empty;
    public int CategoriaId { get; set; } = 0;
}
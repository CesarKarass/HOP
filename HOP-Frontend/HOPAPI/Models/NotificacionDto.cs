using System;

namespace HOPAPI.Models;

public class NotificacionDto
{
   public int Id { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaLeido { get; set; }
    
    // Propiedades adicionales para mensajes
    public int? EmisorId { get; set; }
    public string? EmisorNombre { get; set; }
    public string? MensajeContenido { get; set; }
    
    // Propiedades adicionales para calificaciones
    public int? Puntuacion { get; set; }
    public string? CalificadorNombre { get; set; }
    public string? ServicioTitulo { get; set; }
    public string? ComentarioCalificacion { get; set; }
}


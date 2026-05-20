using System;

namespace HOPAPI.Models;

public class NotificacionCompletaDto
{
    public int Id { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? ReferenciaId { get; set; }
    public int? EmisorId { get; set; }
    public string EmisorNombre { get; set; } = string.Empty;
    public string MensajeContenido { get; set; } = string.Empty;
    public int? Puntuacion { get; set; }
    public string ComentarioCalificacion { get; set; } = string.Empty;
    public string CalificadorNombre { get; set; } = string.Empty;
    public string ServicioTitulo { get; set; } = string.Empty;
}
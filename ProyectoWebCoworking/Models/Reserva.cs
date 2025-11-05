using System;
using System.Collections.Generic;

namespace ProyectoWebCoworking.Models;

public partial class Reserva
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public int RecursoId { get; set; }

    public int TarifaId { get; set; }

    public DateTime FechaHoraInicio { get; set; }

    public DateTime FechaHoraFin { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Recurso Recurso { get; set; } = null!;

    public virtual Tarifa Tarifa { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}

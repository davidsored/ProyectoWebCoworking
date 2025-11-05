using System;
using System.Collections.Generic;

namespace ProyectoWebCoworking.Models;

public partial class Tarifa
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal Precio { get; set; }

    public string TipoRecurso { get; set; } = null!;

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}

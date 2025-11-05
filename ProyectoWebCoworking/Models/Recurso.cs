using System;
using System.Collections.Generic;

namespace ProyectoWebCoworking.Models;

public partial class Recurso
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int Capacidad { get; set; }

    public string Tipo { get; set; } = null!;

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}

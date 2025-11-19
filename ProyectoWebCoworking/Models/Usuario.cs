using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoWebCoworking.Models;

public partial class Usuario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "Los apellidos es obligatorio")]
    public string Apellidos { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string ContraseñaHash { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public string? Teléfono { get; set; }

    [NotMapped]
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Password { get; set; }

    [NotMapped]
    public string? PasswordActual { get; set; }

    [NotMapped]
    public string? NuevaPassword { get; set; }

    [NotMapped]
    [Compare("NuevaPassword", ErrorMessage = "Las contraseñas no coinciden")]
    public string? ConfirmarPassword { get; set; }

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}

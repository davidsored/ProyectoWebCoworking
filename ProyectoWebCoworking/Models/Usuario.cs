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

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El email no es válido")]
    public string Email { get; set; } = null!;

    public string ContraseñaHash { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public string? Teléfono { get; set; }

    [NotMapped]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$", ErrorMessage = "La contraseña debe tener al menos una mayúscula, un número y un carácter especial.")]
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Password { get; set; }

    [NotMapped]
    public string? PasswordActual { get; set; }

    [NotMapped]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$", ErrorMessage = "La contraseña debe tener al menos una mayúscula, un número y un carácter especial.")]
    public string? NuevaPassword { get; set; }

    [NotMapped]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$", ErrorMessage = "La contraseña debe tener al menos una mayúscula, un número y un carácter especial.")]
    [Compare("NuevaPassword", ErrorMessage = "Las contraseñas no coinciden")]
    public string? ConfirmarPassword { get; set; }

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}

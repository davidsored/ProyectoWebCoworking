using Microsoft.AspNetCore.Mvc;
using ProyectoWebCoworking.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ProyectoWebCoworking.Services;
using System.Threading.Tasks;

namespace ProyectoWebCoworking.Controllers
{
    [Authorize] //Cualquier usuario que inició sesión puede acceder
    public class ReservasClientesController : Controller
    {
        private readonly coworking_dbContext _context;
        private readonly IEmailService _emailService;

        public ReservasClientesController(coworking_dbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        #region Index

        public IActionResult Index()
        {
            //Obtenemos el Id del usuario logueado
            string? usuarioIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioIdString))
            {
                return Challenge();
            }

            int usuarioId = int.Parse(usuarioIdString);

            var reservasVencidas = _context.Reservas.Where(r => r.Usuario.Id == usuarioId && r.Estado == "Confirmada" && r.FechaHoraFin < DateTime.Now).ToList();

            if (reservasVencidas.Any())
            {
                foreach (var reserva in reservasVencidas)
                {
                    reserva.Estado = "Finalizada";
                }
                _context.SaveChanges();
            }

            //Buscamos las reservas de ese usuaio con el recuro, la tarifa y ordenado por la hora de inicio
            var misReservas = _context.Reservas.Where(r => r.UsuarioId == usuarioId).Include(r => r.Recurso).Include(r => r.Tarifa).OrderByDescending(r => r.FechaHoraInicio).ToList();

            return View(misReservas);
        }

        #endregion

        #region CrearReserva

        [HttpGet]
        public IActionResult CrearReserva(int id)
        {
            if(id == 0) 
            { 
                return NotFound();
            }

            var recurso = _context.Recursos.Find(id);
            if (recurso == null)
            { 
                return NotFound();            
            }

            var tarifa = _context.Tarifas.FirstOrDefault(t => t.TipoRecurso == recurso.Tipo);
            decimal precioHora = tarifa != null ? tarifa.Precio : 0;

            //Pasamos el recurso utilizando ViewData y creamos una reserva vacía para que la utilice el formulario.
            ViewData["Recurso"] = recurso;
            ViewData["PrecioHora"] = precioHora;

            DateTime ahora = DateTime.Now;
            DateTime inicio = new DateTime(ahora.Year, ahora.Month, ahora.Day, ahora.Hour, ahora.Minute, 0);
            var reserva = new Reserva
            {
                RecursoId = recurso.Id,
                FechaHoraInicio = inicio,
                FechaHoraFin = inicio.AddHours(1)
            };

            return View(reserva);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearReserva([Bind("RecursoId, FechaHoraInicio, FechaHoraFin")] Reserva reserva)
        {
            try
            {
                //Obtenemos el Id del usuario logueado
                string? usuarioIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioIdString))
                {
                    return Challenge(); //Si el usuario no está logueado, se redirige al Login
                }

                reserva.UsuarioId = int.Parse(usuarioIdString);

                //Buscamos el recurso para encontrar el tipo
                var recurso = _context.Recursos.Find(reserva.RecursoId);
                if (recurso == null)
                {
                    return NotFound();
                }

                //Buscamos la tarifa que coincide con el Tipo de recurso
                var tarifa = _context.Tarifas.FirstOrDefault(t => t.TipoRecurso == recurso.Tipo);
                if (tarifa == null)
                {
                    //Si no hay tarifa para este tipo de recurso, redirigimos la recurso
                    ModelState.AddModelError(string.Empty, "No se encontró una tarifa aplicable para este tipo de recurso.");
                    ViewData["Recurso"] = recurso;
                    return View(reserva);
                }
                reserva.TarifaId = tarifa.Id;
                reserva.Estado = "Confirmada";

                //Eliminamos los errores de los campos manualmente
                ModelState.Remove("UsuarioId");
                ModelState.Remove("TarifaId");
                ModelState.Remove("Estado");
                ModelState.Remove("Usuario");
                ModelState.Remove("Recurso");
                ModelState.Remove("Tarifa");
                
                //Validamos que la fecha fin sea mayor que la de inicio
                if(reserva.FechaHoraInicio >= reserva.FechaHoraFin)
                {
                    ModelState.AddModelError("FechaHoraFin", "La fecha de fin debe ser posterior a la fecha de inicio.");
                }

                string tipoDeseado = recurso.Tipo;

                var recursosCandidatos = _context.Recursos.Where(r => r.Tipo == tipoDeseado).ToList();

                int? recursoLibreId = null;

                foreach (var candidato in recursosCandidatos)
                {
                    bool estaOcupado = _context.Reservas.Any(r => r.RecursoId == candidato.Id && r.Estado != "Cancelada" && r.FechaHoraInicio < reserva.FechaHoraFin && r.FechaHoraFin > reserva.FechaHoraInicio);

                    if (!estaOcupado)
                    {
                        recursoLibreId = candidato.Id;
                        break;
                    }
                }

                if (recursoLibreId != null)
                {
                    reserva.RecursoId = recursoLibreId.Value;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Lo sentimos, este recurso ya está reservado en este horario");

                    ViewData["Recurso"] = recurso;
                    return View(reserva);
                }
                
                if (ModelState.IsValid)
                {                    
                    _context.Reservas.Add(reserva);
                    _context.SaveChanges();

                    //Obtenemos el email de la cookie
                    string emailUsuario = User.FindFirst(ClaimTypes.Email)?.Value;

                    string asunto = $"Confirmación de Reserva - {recurso.Nombre}";
                    string mensaje = $@"Hola, Tu reserva ha sido confirmada con éxito.
                                        Recurso Asignado: {recurso.Nombre} (ID: {reserva.RecursoId})
                                        Fecha Inicio: {reserva.FechaHoraInicio}
                                        Fecha Fin: {reserva.FechaHoraFin}
                                        Precio: {tarifa.Precio} €/hora
                                        Estado: {reserva.Estado}
                                        
                                        ¡Gracias por confiar en nosotros!";

                    //Envío de email
                    if(!string.IsNullOrEmpty(emailUsuario))
                    {
                        await _emailService.SendEmailAsync(emailUsuario, asunto, mensaje);
                    }


                    //En un futuro redirigiremos a la página "Mis Reservas"
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error al guardar la reserva.");
            }

            //Si algo falla, volvemos a mostrar el formulario
            ViewData["Recurso"] = _context.Recursos.Find(reserva.RecursoId);
            return View(reserva);
        }

        #endregion

        #region CancelarReserva

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmado(int id)
        {
            //Obtenemos el Id del usuarios logueado
            string? usuarioIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(string.IsNullOrEmpty(usuarioIdString))
            {
                return Challenge();
            } 

            int usuarioId = int.Parse(usuarioIdString);

            //Buscamos la reserva y comprobamos que se corresponda con el usuario
            var reserva = _context.Reservas.Find(id);

            if (reserva != null) 
            {
                if(reserva.UsuarioId != usuarioId)
                {
                    return Forbid();
                }

                //Si todo está correcto, borramos
                _context.Reservas.Remove(reserva);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Disponibilidad

        [HttpGet]
        public IActionResult ComprobarDisponibilidad(int recursoId, DateTime inicio, DateTime fin)
        {
            var recursoPrototipo = _context.Recursos.Find(recursoId);
            if (recursoPrototipo == null)
            {
                return Json(false);
            }

            var recursosCandidatos = _context.Recursos.Where(r => r.Tipo == recursoPrototipo.Tipo).ToList();

            foreach (var candidato in recursosCandidatos)
            {
                bool estaOcupado = _context.Reservas.Any(r => r.RecursoId == candidato.Id && r.Estado != "Cancelada" && r.FechaHoraInicio < fin && r.FechaHoraFin > inicio);
                if (!estaOcupado)
                {
                    return Json(true);
                }
            }
            return Json(false);
        }

        #endregion
    }
}

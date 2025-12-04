using Microsoft.AspNetCore.Mvc;
using ProyectoWebCoworking.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ProyectoWebCoworking.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ReservasController : Controller
    {
        private readonly coworking_dbContext _context;

        public ReservasController(coworking_dbContext context) 
        {             
            _context = context; 
        }

        #region Index

        public IActionResult Index()
        {
            var reservasVencidas = _context.Reservas.Where(r => r.Estado == "Confirmada" && r.FechaHoraFin < DateTime.Now).ToList();

            if (reservasVencidas.Any()) 
            {
                foreach (var reserva in reservasVencidas)
                {
                    reserva.Estado = "Finalizada";
                }
                _context.SaveChanges();
            }

            //Unimos las tablas para ver los nombres y los Ids
            var listaReservas = _context.Reservas.Include(r => r.Usuario).Include(r => r.Recurso).ToList();
            
            return View(listaReservas);
        }

        #endregion

        #region Details

        [HttpGet]
        public IActionResult Details(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            //Utilizamos .Include() para cargas todas las tablas relacionadas
            var reserva = _context.Reservas.Include(r => r.Usuario).Include(r => r.Recurso).Include(r => r.Tarifa).FirstOrDefault(r => r.Id == id);

            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        #endregion

        #region Delete

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmado(int id)
        {
            var reserva = _context.Reservas.Find(id);
            if(reserva != null)
            {
                _context.Reservas.Remove(reserva);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        
    }
}

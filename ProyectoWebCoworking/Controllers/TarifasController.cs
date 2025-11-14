using Microsoft.AspNetCore.Mvc;
using ProyectoWebCoworking.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace ProyectoWebCoworking.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class TarifasController : Controller
    {
        private readonly coworking_dbContext _context;

        public TarifasController(coworking_dbContext context)
        {
            _context = context;
        }

        #region Index
        public IActionResult Index()
        {
            var listaDeTarifas = _context.Tarifas.ToList();
            
            return View(listaDeTarifas);
        }

        #endregion

        #region Create

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Nombre, Precio, TipoRecurso")] Tarifa tarifa)
        {
            if (ModelState.IsValid)
            {
                _context.Tarifas.Add(tarifa);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(tarifa);
        }
        #endregion

        #region Edit

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var tarifa = _context.Tarifas.Find(id);
            if (tarifa == null)
            {
                return NotFound();
            }

            return View(tarifa);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id, Nombre, Precio, TipoRecurso")] Tarifa tarifa)
        {
            if (id != tarifa.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(tarifa);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(tarifa);
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

            var tarifa = _context.Tarifas.Find(id);
            if(tarifa == null)
            {
                return NotFound();
            }

            return View(tarifa);
        }

        #endregion

        #region Delete

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmado(int id)
        {
            var tarifa = _context.Tarifas.Find(id);

            if (tarifa != null) 
            {
                _context.Tarifas.Remove(tarifa);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProyectoWebCoworking.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace ProyectoWebCoworking.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly coworking_dbContext _context;

        public HomeController(ILogger<HomeController> logger, coworking_dbContext context)
        {
            _logger = logger;
            _context = context;
        }

        #region Index

        public IActionResult Index()
        {
            return View();
        }

        #endregion

        #region Privacy

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        #endregion

        #region Error

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #endregion

        #region AdminPanel

        [Authorize(Roles = "Administrador")]
        public IActionResult AdminPanel() 
        {
            return View();
        }

        #endregion

        #region Catálogo

        [AllowAnonymous] //Con esta etiqueta nos aseguramos que todos puedan ver los datos.
        public IActionResult Catalogo()
        {
            var listaRecursos = _context.Recursos.AsEnumerable().GroupBy(r => r.Tipo).Select(g => g.First()).ToList();

            return View(listaRecursos);
        }

        #endregion
    }
}

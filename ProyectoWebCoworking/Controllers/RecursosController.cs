using Microsoft.AspNetCore.Mvc;
using ProyectoWebCoworking.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace ProyectoWebCoworking.Controllers
{
    [Authorize(Roles = "Administrador")] //Protegemos la clase entera para que solo los administradores puedan acceder a cualquier acción.
    public class RecursosController : Controller
    {

        private readonly coworking_dbContext _context; //Establecemos conexión con la base de datos

        public RecursosController(coworking_dbContext context)
        {
            _context = context;
        }

        //La acción Index será la página principal y mostrará la lista de recursos
        public IActionResult Index()
        {
            //Solicitamos a la base de datos la lista de recursos y la guardamos en una lista
            var listaRecursos = _context.Recursos.ToList();
            
            return View(listaRecursos);
        }
    }
}

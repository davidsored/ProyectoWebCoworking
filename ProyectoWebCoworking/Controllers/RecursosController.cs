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

        #region Index

        //La acción Index será la página principal y mostrará la lista de recursos
        public IActionResult Index()
        {
            //Solicitamos a la base de datos la lista de recursos y la guardamos en una lista
            var listaRecursos = _context.Recursos.ToList();
            
            return View(listaRecursos);
        }

        #endregion

        #region Create
        //Establecemos el formulario para crear un nuevo recurso con los métodos Get para mostrarlo y Post para guardarlo
        [HttpGet]
        public IActionResult Create() 
        {
            return View();        
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Nombre, Tipo, Capacidad")] Recurso recurso)
        {
            //Comprobamos si los datos recibidos son válidos, añadimos el recurso a la base de datos y redirigimos al usuario a la vista
            if (ModelState.IsValid)
            {
                _context.Recursos.Add(recurso);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            
            return View(recurso);
        }

        #endregion

        #region Edit
        //Esta acción busca un recurso por su Id y muestra el formulario para editarlo
        [HttpGet]
        public IActionResult Edit(int id)
        {
            //Validamos el Id
            if (id == 0)
            {
                return NotFound();
            }

            //Buscamos el recurso en la base de datos por su Id y comprobamos si existe
            var recurso = _context.Recursos.Find(id);
            if(recurso == null)
            {
                return NotFound();
            }

            return View(recurso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id, Nombre, Tipo, Capacidad")] Recurso recurso)
        {
            //Comprobamos que el Id de la URL coincide con el Id del formulario
            if (id != recurso.Id)
            {
                return NotFound();
            }

            //Comprobamos si los datos son válidos como en el Edit
            if (ModelState.IsValid)
            {
                _context.Update(recurso);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(recurso);
        }

        #endregion

        #region Details

        [HttpGet]
        public IActionResult Details(int id)
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

            return View(recurso);
        }

        #endregion

        #region Delete

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //Llamamos DeleteConfirmed al método para que no entre en conflicto con el método Get
        public IActionResult DeleteConfirmado(int id)
        {
            //Buscamos el recurso y si existe, se borra
            var recurso = _context.Recursos.Find(id);
            if (recurso != null)
            {
                _context.Recursos.Remove(recurso);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

            #endregion
    }
}

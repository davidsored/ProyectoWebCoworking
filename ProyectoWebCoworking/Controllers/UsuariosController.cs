using Microsoft.AspNetCore.Mvc;
using ProyectoWebCoworking.Models;
using BCrypt.Net;

namespace ProyectoWebCoworking.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly coworking_dbContext _context;

        public UsuariosController(coworking_dbContext context)
        {
            _context = context;
        }

        //El método Get se utiliza para mostrar el formulario de registro
        [HttpGet]
        public IActionResult Registro()
        {
            return View(); //Se buscará un archivo en Views/Usuarios/Registro.cshtml
        }

        //El método Post de utiliza para recibir los datos del formulario de registro
        [HttpPost]
        public IActionResult Registro(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                //Haseamos la contraseña
                usuario.ContraseñaHash = BCrypt.Net.BCrypt.HashPassword(usuario.Password);

                //Añadimos el nuevo usuario a la BD, guardamos los cambios y redirigimos al Login.
                _context.Usuarios.Add(usuario);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }

            return View(usuario);
        }

        //El metodo Get se utuliza para mostrar la página o formulario de login
        [HttpGet]
        public IActionResult Login()
        {
            return View(); //Se buscará un archivo en Views/Usuarios/Login.cshtml
        }

        [HttpPost]
        public IActionResult Login(string Email, string Password)
        {
            //Buscamos al usuario por su Email
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == Email);

            //Comprobamos que el usuario existe y la contraseña es correcta
            if ( usuario != null && BCrypt.Net.BCrypt.Verify(Password, usuario.ContraseñaHash))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Email = "Email o contraseña incorrectos";
            return View();
        }
        
        
        public IActionResult Index()
        {
            return View(); 
        }
    }
}

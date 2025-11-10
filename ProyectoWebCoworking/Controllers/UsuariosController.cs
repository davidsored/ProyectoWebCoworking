using Microsoft.AspNetCore.Mvc;
using ProyectoWebCoworking.Models;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;

namespace ProyectoWebCoworking.Controllers
{
    public class UsuariosController : Controller
    {
        //Establecemos la inyección nde dependecia con la base de datos
        private readonly coworking_dbContext _context;

        public UsuariosController(coworking_dbContext context)
        {
            _context = context;
        }

        #region Registro

        //El método Get se utiliza para mostrar el formulario de registro
        [HttpGet]
        public IActionResult Registro()
        {
            return View(); //Se buscará un archivo en Views/Usuarios/Registro.cshtml
        }

        //El método Post de utiliza para recibir los datos del formulario de registro
        [HttpPost]
        public IActionResult Registro([Bind("Nombre", "Email", "Password", "Teléfono")] Usuario usuario)
        {
            ModelState.Remove("Rol");
            ModelState.Remove("ContraseñaHash");


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
        #endregion

        #region Login

        //El metodo Get se utuliza para mostrar la página o formulario de login
        [HttpGet]
        public IActionResult Login()
        {
            return View(); //Se buscará un archivo en Views/Usuarios/Login.cshtml
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            //Buscamos al usuario por su Email
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == Email);

            //Comprobamos que el usuario existe y la contraseña es correcta
            if ( usuario != null && BCrypt.Net.BCrypt.Verify(Password, usuario.ContraseñaHash))
            {
                //Creamos la lista de datos clave del usuario
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Rol)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true //Casilla de Recordarme
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Email o contraseña incorrectos";
            return View();
        }
        #endregion

        #region Logout
        
        //Creamos el cierre de sesión
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");  
        }

        #endregion 

        public IActionResult Index()
        {
            return View(); 
        }
    }
}

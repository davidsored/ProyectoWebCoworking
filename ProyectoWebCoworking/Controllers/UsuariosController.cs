using Microsoft.AspNetCore.Mvc;
using ProyectoWebCoworking.Models;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

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

        #region Index

        [Authorize(Roles = "Administrador")]
        public IActionResult Index()
        {
            var listaUsarios = _context.Usuarios.ToList();

            return View(listaUsarios); 
        }

        #endregion

        #region EditRol

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult EditRol(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var usuario = _context.Usuarios.Find(id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditRol(int id, [Bind("Id, Rol")] Usuario usuarioForm)
        {
            if (id != usuarioForm.Id)
            {
                return NotFound();
            }

            var usuarioBD = _context.Usuarios.Find(id);
            if (usuarioBD == null)
            {
                return NotFound();
            }

            ModelState.Remove("Nombre");
            ModelState.Remove("Email");
            ModelState.Remove("ContraseñaHash");
            ModelState.Remove("Password");

            if (ModelState.IsValid) 
            {
                //Solo actualizamos el Rol del usuario
                usuarioBD.Rol = usuarioForm.Rol;

                _context.Update(usuarioBD);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(usuarioBD); //Si hay un error, redirigimos al formulario
        }

        #endregion

        #region Delete

        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmado(int id)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario != null) 
            {
                _context.Usuarios.Remove(usuario);
                _context.SaveChanges();
            }
            
            return RedirectToAction(nameof(Index));            
            
        }

        #endregion

        #region PerfilUsuario

        [Authorize]
        [HttpGet]
        public IActionResult MiPerfil()
        {
            //Obtenemos el Id del usuario logueado
            string? usuarioIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioIdString))
            {
                return Challenge();
            }
            int usuarioId = int.Parse(usuarioIdString);

            //Se busca al usuario en la base de datos
            var usuario = _context.Usuarios.Find(usuarioId);
            if(usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult MiPerfil([Bind("Id, Nombre, Apellidos, Email, Teléfono, PasswordActual, NuevaPassword, ConfirmarPassword")] Usuario usuarioForm)
        {
            //Identificamos al usuario logueado
            string? usuarioIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int usuarioIdLogueado = int.Parse(usuarioIdString ?? "0");

            if(usuarioForm.Id != usuarioIdLogueado)
            {
                return Forbid();
            }

            //Obtenemos al usuario de la base de datos
            var usuarioBD = _context.Usuarios.Find(usuarioForm.Id);
            if(usuarioBD == null)
            {
                return NotFound();
            }

            //Validación de la contraseña actual
            if(string.IsNullOrEmpty(usuarioForm.PasswordActual))
            {
                ModelState.AddModelError("PasswordActual", "Debes ingresar tu contraseña actual para guardar cualquier cambio.");
            }
            else
            {
                bool passwordCorrecta = BCrypt.Net.BCrypt.Verify(usuarioForm.PasswordActual, usuarioBD.ContraseñaHash);
                if(!passwordCorrecta)
                {
                    ModelState.AddModelError("PasswordActual", "La contraseña actual es incorrecta.");
                }
            }

            //Validación de la duplicidad del email
            if (usuarioForm.Email != usuarioBD.Email)
            {
                bool emailExiste = _context.Usuarios.Any(u => u.Email == usuarioForm.Email && u.Id != usuarioForm.Id);
                if (emailExiste)
                {
                    ModelState.AddModelError("Email", "Este correo ya está registrado");
                }
            }

            //Cambio de contraseña (solo se accede si se escribe algo en el campo "NuevaPassword")
            bool cambiarPassword = !string.IsNullOrEmpty(usuarioForm.NuevaPassword);
            
            if (cambiarPassword)
            {
                if (usuarioForm.NuevaPassword != usuarioForm.ConfirmarPassword)
                {
                    ModelState.AddModelError("ConfirmarPassword", "Las contraseñas nuevas no coinciden.");
                }
                else
                {
                    //si Todo está bien, hasheamos la nueva contraseña
                    usuarioBD.ContraseñaHash = BCrypt.Net.BCrypt.HashPassword(usuarioForm.NuevaPassword);
                }
            }
                        
            ModelState.Remove("Rol");
            ModelState.Remove("ContraseñaHash");
            ModelState.Remove("Password");            

            if (ModelState.IsValid)
            {
                usuarioBD.Nombre = usuarioForm.Nombre;
                usuarioBD.Apellidos = usuarioForm.Apellidos;
                usuarioBD.Teléfono = usuarioForm.Teléfono;
                usuarioBD.Email = usuarioForm.Email;

                _context.Update(usuarioBD);
                _context.SaveChanges();

                TempData["MensajeExito"] = "¡Datos actualizados correctamente!";
                
                return RedirectToAction(nameof(MiPerfil));
            }

            if(string.IsNullOrEmpty(usuarioForm.Email)) usuarioForm.Email = usuarioBD.Email;

            return View(usuarioForm);
        }

        #endregion
    }
}

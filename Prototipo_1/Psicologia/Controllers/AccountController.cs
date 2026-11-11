
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Psicologia.Data;
using Psicologia.Data.Entities;
using Psicologia.Services.Abstractions;
using System.Security.Claims;

namespace ClinicaPsicologia.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUsuarioService _usuarioService;
        private readonly DataContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public AccountController(
            IAuthService authService,
            IUsuarioService usuarioService,
            DataContext context)
        {
            _authService = authService;
            _usuarioService = usuarioService;
            _context = context;
            _passwordHasher = new PasswordHasher<Usuario>();
        }

        // MOSTRAR INICIO DE SESIÓN
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // PROCESAR INICIO DE SESIÓN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string correo,
            string password)
        {
            if (string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Ingresa tu correo y contraseña.";
                return View();
            }

            var usuario = await _authService.AuthenticateAsync(
                correo.Trim(),
                password
            );

            if (usuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.UsuarioId.ToString()
                ),

                new Claim(ClaimTypes.Name, usuario.Name),

                new Claim(ClaimTypes.Email, usuario.Correo),

                new Claim(ClaimTypes.Role, usuario.Role.Name)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return RedirectToAction("Index", "Home");
        }

        // MOSTRAR CONFIGURACIÓN INICIAL DEL ADMINISTRADOR
        [HttpGet]
        public async Task<IActionResult> SetupAdmin()
        {
            var admin = await _context.Usuarios
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Correo == "kata@gmail.com");

            if (admin == null ||
                admin.Role?.Name != "Administrador" ||
                !string.IsNullOrEmpty(admin.PasswordHash))
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        // GUARDAR CONTRASEÑA INICIAL DEL ADMINISTRADOR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetupAdmin(
            string password,
            string confirmarPassword)
        {
            var admin = await _context.Usuarios
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Correo == "kata@gmail.com");

            if (admin == null ||
                admin.Role?.Name != "Administrador" ||
                !string.IsNullOrEmpty(admin.PasswordHash))
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrWhiteSpace(password) ||
                password.Length < 8)
            {
                ViewBag.Error =
                    "La contraseña debe tener mínimo 8 caracteres.";

                return View();
            }

            if (password != confirmarPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                return View();
            }

            bool configurado = await _usuarioService
                .ConfigurarPasswordInicialAdministradorAsync(
                    "kata@gmail.com",
                    password
                );

            if (!configurado)
            {
                ViewBag.Error =
                    "No fue posible configurar la contraseña.";

                return View();
            }

            TempData["Mensaje"] =
                "Contraseña configurada correctamente. Ya puedes iniciar sesión.";

            return RedirectToAction("Login");
        }

        // CERRAR SESIÓN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Login");
        }

        // ACCESO DENEGADO
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

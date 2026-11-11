
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Psicologia.Data;
using Psicologia.Services.Abstractions;

namespace ClinicaPsicologia.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly DataContext _context;

        public UsuariosController(
            IUsuarioService usuarioService,
            DataContext context)
        {
            _usuarioService = usuarioService;
            _context = context;
        }

        // MOSTRAR FORMULARIO PARA CREAR USUARIO
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarRoles();
            return View();
        }

        // PROCESAR CREACIÓN DEL USUARIO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string nombre,
            string correo,
            string password,
            string confirmarPassword,
            int roleId)
        {
            await CargarRoles();

            // VALIDAR CAMPOS OBLIGATORIOS
            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmarPassword))
            {
                ViewBag.Error = "Todos los campos son obligatorios.";
                return View();
            }

            // VALIDAR CONTRASEÑA
            if (password.Length < 8)
            {
                ViewBag.Error = "La contraseña debe tener mínimo 8 caracteres.";
                return View();
            }

            // CONFIRMAR CONTRASEÑA
            if (password != confirmarPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                return View();
            }

            // VALIDAR QUE EL ROL EXISTA
            bool rolExiste = await _context.Roles
                .AnyAsync(r => r.RoleId == roleId);

            if (!rolExiste)
            {
                ViewBag.Error = "El rol seleccionado no es válido.";
                return View();
            }

            try
            {
                bool creado = await _usuarioService.CrearUsuarioAsync(
                    nombre.Trim(),
                    correo.Trim(),
                    password,
                    roleId
                );

                if (!creado)
                {
                    ViewBag.Error = "Este correo ya está registrado.";
                    return View();
                }

                TempData["Mensaje"] = "Usuario creado correctamente.";

                return RedirectToAction(nameof(Create));
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        // CARGAR ROLES DISPONIBLES
        private async Task CargarRoles()
        {
            var roles = await _context.Roles
                .OrderBy(r => r.Name)
                .ToListAsync();

            ViewBag.Roles = new SelectList(
                roles,
                "RoleId",
                "Name"
            );
        }
    }
}

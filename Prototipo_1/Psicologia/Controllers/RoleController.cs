// Controllers/RoleController.cs
using Microsoft.AspNetCore.Mvc;
using Psicologia.Data.Entities;
using Psicologia.Repositories;

namespace Psicologia.Controllers;

public class RoleController : Controller
{
    private readonly IRoleRepository _repository;

    public RoleController(IRoleRepository repository)
    {
        _repository = repository;
    }

    public async Task<IActionResult> Index() =>
        View(await _repository.GetAllAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name)
    {
        name = name?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "El nombre es obligatorio.";
        }
        else if (name.Length > 100)
        {
            TempData["Error"] = "El nombre del rol no puede superar los 100 caracteres.";
        }
        else
        {
            var exists = (await _repository.GetAllAsync())
                .Any(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                TempData["Error"] = $"El rol \"{name}\" ya existe.";
            }
            else
            {
                await _repository.AddAsync(new Role { Name = name });
                TempData["Success"] = "Rol agregado correctamente.";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _repository.DeleteAsync(id);
        TempData["Success"] = "Rol eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
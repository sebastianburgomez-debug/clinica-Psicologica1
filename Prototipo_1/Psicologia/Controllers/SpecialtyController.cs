// Controllers/SpecialtyController.cs
using Microsoft.AspNetCore.Mvc;
using Psicologia.Data.Entities;
using Psicologia.Repositories;

namespace Psicologia.Controllers;

public class SpecialtyController : Controller
{
    private readonly ISpecialtyRepository _repository;

    public SpecialtyController(ISpecialtyRepository repository)
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
            TempData["Error"] = "El nombre de la especialidad no puede superar los 100 caracteres.";
        }
        else
        {
            var exists = (await _repository.GetAllAsync())
                .Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                TempData["Error"] = $"La especialidad \"{name}\" ya existe.";
            }
            else
            {
                await _repository.AddAsync(new Specialty { Name = name });
                TempData["Success"] = "Especialidad agregada correctamente.";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _repository.DeleteAsync(id);
        TempData["Success"] = "Especialidad eliminada.";
        return RedirectToAction(nameof(Index));
    }
}
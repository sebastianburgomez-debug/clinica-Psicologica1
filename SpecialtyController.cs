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

    public IActionResult Index() => View(_repository.GetAll());

    [HttpPost]
    public IActionResult Create(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            _repository.Add(new Specialty { Name = name });
        }
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        _repository.Delete(id);
        return RedirectToAction(nameof(Index));
    }
}
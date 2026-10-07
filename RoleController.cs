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

    public IActionResult Index() => View(_repository.GetAll());

    [HttpPost]
    public IActionResult Create(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            _repository.Add(new Role { Name = name });
        }
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        _repository.Delete(id);
        return RedirectToAction(nameof(Index));
    }
}
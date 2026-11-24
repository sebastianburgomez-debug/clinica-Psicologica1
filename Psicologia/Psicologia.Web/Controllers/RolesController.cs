using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Attributes;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.DTOs.Role;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Controllers
{
    public class RolesController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IRolesService _service;

        public RolesController(INotyfService notyfService, IRolesService service)
        {
            _notyfService = notyfService;
            _service = service;
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "roles.show")]
        public async Task<IActionResult> Index([FromQuery] PaginationRequest request)
        {
            Response<PaginationResponse<RoleDTO>> response = await _service.GetPaginationAsync(request);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return RedirectToAction("Index", "Home");
            }

            return View(response.Result);
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "roles.create")]
        public async Task<IActionResult> Create()
        {
            Response<List<PermissionsForRoleDTO>> permissionsResponse = await _service.GetPermissionsAsync();

            if (!permissionsResponse.IsSuccess)
            {
                _notyfService.Error(permissionsResponse.Message);
                return RedirectToAction(nameof(Index));
            }

            CreateRoleDTO dto = new CreateRoleDTO
            {
                Permissions = permissionsResponse.Result
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "roles.create")]
        public async Task<IActionResult> Create([FromForm] CreateRoleDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                dto.Permissions = await GetPermissionsForRoleAsync(dto.PermissionIds);
                return View(dto);
            }

            Response<CreateRoleDTO> response = await _service.CreateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                dto.Permissions = await GetPermissionsForRoleAsync(dto.PermissionIds);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "roles.update")]
        public async Task<IActionResult> Edit([FromRoute] Guid id)
        {
            Response<RoleDTO> response = await _service.GetOneAsync(id);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }

            UpdateRoleDTO dto = new UpdateRoleDTO
            {
                Id = response.Result.Id,
                Name = response.Result.Name,
                Permissions = response.Result.Permissions
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "roles.update")]
        public async Task<IActionResult> Edit([FromForm] UpdateRoleDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                dto.Permissions = await GetPermissionsForRoleAsync(dto.PermissionIds);
                return View(dto);
            }

            Response<RoleDTO> response = await _service.UpdateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                dto.Permissions = await GetPermissionsForRoleAsync(dto.PermissionIds);
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "roles.delete")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            Response response = await _service.DeleteAsync(id);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
            }
            else
            {
                _notyfService.Success(response.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        // Recarga la lista de permisos conservando los que el usuario ya había marcado
        private async Task<List<PermissionsForRoleDTO>> GetPermissionsForRoleAsync(string? permissionIds)
        {
            Response<List<PermissionsForRoleDTO>> permissionsResponse = await _service.GetPermissionsAsync();

            List<PermissionsForRoleDTO> permissions = permissionsResponse.Result ?? [];

            if (!string.IsNullOrWhiteSpace(permissionIds))
            {
                List<Guid> selectedIds = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(permissionIds) ?? [];

                permissions.ForEach(p => p.Selected = selectedIds.Contains(p.Id));
            }

            return permissions;
        }
    }
}

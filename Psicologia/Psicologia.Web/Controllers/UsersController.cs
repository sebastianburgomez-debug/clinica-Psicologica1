using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Attributes;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.DTOs.User;
using Psicologia.Web.Helpers.Abstractions;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IUsersService _usersService;
        private readonly ISelectListHelper _selectListHelper;

        public UsersController(INotyfService notyfService, IUsersService usersService, ISelectListHelper selectListHelper)
        {
            _notyfService = notyfService;
            _usersService = usersService;
            _selectListHelper = selectListHelper;
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "users.show")]
        public async Task<IActionResult> Index([FromQuery] PaginationRequest request)
        {
            Response<PaginationResponse<UserDTO>> response = await _usersService.GetPaginationAsync(request);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return RedirectToAction("Index", "Home");
            }

            return View(response.Result);
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "users.create")]
        public async Task<IActionResult> Create()
        {
            CreateUserDTO dto = new CreateUserDTO
            {
                Roles = await _selectListHelper.GetRolesSelectListAsync()
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "users.create")]
        public async Task<IActionResult> Create([FromForm] CreateUserDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                dto.Roles = await _selectListHelper.GetRolesSelectListAsync();
                return View(dto);
            }

            Response<UserDTO> response = await _usersService.CreateAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                dto.Roles = await _selectListHelper.GetRolesSelectListAsync();
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "users.update")]
        public async Task<IActionResult> Edit([FromRoute] Guid id)
        {
            Response<UserDTO> response = await _usersService.GetOneAsync(id);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }

            UpdateUserDTO dto = new UpdateUserDTO
            {
                Id = response.Result.Id,
                Name = response.Result.Name,
                Email = response.Result.Email,
                RoleId = response.Result.RoleId,
                Roles = await _selectListHelper.GetRolesSelectListAsync()
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "users.update")]
        public async Task<IActionResult> Edit([FromForm] UpdateUserDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                dto.Roles = await _selectListHelper.GetRolesSelectListAsync();
                return View(dto);
            }

            Response<UserDTO> response = await _usersService.UpdateUserAsync(dto);

            if (!response.IsSuccess)
            {
                _notyfService.Error(response.Message);
                dto.Roles = await _selectListHelper.GetRolesSelectListAsync();
                return View(dto);
            }

            _notyfService.Success(response.Message);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permissionCode: "users.delete")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            Response<UserDTO> userResponse = await _usersService.GetOneAsync(id);

            if (userResponse.Result?.Email == User.Identity?.Name)
            {
                _notyfService.Error("No puede eliminar su propio usuario");
                return RedirectToAction(nameof(Index));
            }

            Response response = await _usersService.DeleteAsync(id);

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
    }
}

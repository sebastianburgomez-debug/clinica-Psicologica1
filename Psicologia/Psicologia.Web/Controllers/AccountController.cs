using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Psicologia.Web.Core;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.DTOs.Account;
using Psicologia.Web.Services.Abstractions;
using MySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Psicologia.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly INotyfService _notyfService;
        private readonly IUsersService _usersService;
        private readonly IMapper _mapper;

        public AccountController(IAccountService accountService, INotyfService notyfService, IUsersService usersService, IMapper mapper)
        {
            _accountService = accountService;
            _notyfService = notyfService;
            _usersService = usersService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (_accountService.AccountUserIsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginDTO dto, [FromQuery] string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                Response<MySignInResult> result = await _accountService.LoginAsync(dto);

                if (result.IsSuccess)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos");
                _notyfService.Error("Email o contraseña incorrectos");
            }
            else
            {
                _notyfService.Error("Debe ajustar los errores de validación");
            }

            return View(dto);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UpdateUser()
        {
            string? email = User?.Identity?.Name;

            if (string.IsNullOrEmpty(email))
            {
                _notyfService.Error("El email es requerido");
                return RedirectToAction("Index", "Home");
            }

            Response<User> userResponse = await _usersService.GetUserByEmailAsync(email);

            if (!userResponse.IsSuccess || userResponse.Result is null)
            {
                _notyfService.Error(userResponse.Message);
                return RedirectToAction("Index", "Home");
            }

            AccountUserDTO dto = _mapper.Map<AccountUserDTO>(userResponse.Result);

            return View(dto);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser([FromForm] AccountUserDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _notyfService.Error("Debe ajustar los errores de validación");
                return View(dto);
            }

            Response<AccountUserDTO> result = await _accountService.UpdateAccountUserAsync(dto);

            if (result.IsSuccess)
            {
                _notyfService.Success(result.Message);
            }
            else
            {
                _notyfService.Error(result.Message);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}

using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Attributes;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.DTOs.Audit;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Controllers
{
    // Administración del Sistema: auditoría de accesos y cambios (solo lectura)
    public class AuditLogsController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IAuditService _service;

        public AuditLogsController(INotyfService notyfService, IAuditService service)
        {
            _notyfService = notyfService;
            _service = service;
        }

        [HttpGet]
        [CustomAuthorize(permissionCode: "audit.show")]
        public async Task<IActionResult> Index([FromQuery] PaginationRequest request)
        {
            Response<PaginationResponse<AuditLogDTO>> response = await _service.GetPaginationAsync(request);

            if (!response.IsSuccess || response.Result is null)
            {
                _notyfService.Error(response.Errors?.FirstOrDefault() ?? response.Message);
                return RedirectToAction("Index", "Home");
            }

            return View(response.Result);
        }
    }
}

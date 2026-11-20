using Psicologia.Web.Core;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.DTOs.Audit;

namespace Psicologia.Web.Services.Abstractions
{
    public interface IAuditService
    {
        // userName: se envía explícitamente cuando aún no hay sesión (ej. inicio de sesión)
        public Task LogAsync(string action, string entity, string? entityId, string description, string? userName = null);
        public Task<Response<PaginationResponse<AuditLogDTO>>> GetPaginationAsync(PaginationRequest request);
    }
}

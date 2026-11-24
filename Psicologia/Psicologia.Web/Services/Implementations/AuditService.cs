using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.Data;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.DTOs.Audit;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Services.Implementations
{
    public class AuditService : CustomQueryableOperationsService, IAuditService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditService> _logger;

        public AuditService(DataContext context,
                            IMapper mapper,
                            IHttpContextAccessor httpContextAccessor,
                            ILogger<AuditService> logger) : base(context, mapper)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task LogAsync(string action, string entity, string? entityId, string description, string? userName = null)
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;

            AuditLog log = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserName = Truncate(userName ?? httpContext?.User?.Identity?.Name ?? "Sistema", 256),
                Action = Truncate(action, 32)!,
                Entity = Truncate(entity, 64)!,
                EntityId = Truncate(entityId, 64),
                Description = Truncate(description, 512)!,
                IpAddress = Truncate(httpContext?.Connection.RemoteIpAddress?.ToString(), 64),
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _context.AuditLogs.AddAsync(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // La auditoría nunca debe interrumpir la operación principal
                _context.Entry(log).State = EntityState.Detached;
                _logger.LogError(ex, "No fue posible registrar la auditoría: {Action} {Entity}", action, entity);
            }
        }

        public async Task<Response<PaginationResponse<AuditLogDTO>>> GetPaginationAsync(PaginationRequest request)
        {
            IQueryable<AuditLog> query = _context.AuditLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                string filter = request.Filter.Trim().ToLower();

                query = query.Where(a => (a.UserName != null && a.UserName.ToLower().Contains(filter))
                                         || a.Action.ToLower().Contains(filter)
                                         || a.Entity.ToLower().Contains(filter)
                                         || a.Description.ToLower().Contains(filter));
            }

            query = query.OrderByDescending(a => a.CreatedAt);

            return await GetPagedListAsync<AuditLogDTO, AuditLog>(request, query);
        }

        private static string? Truncate(string? value, int maxLength)
        {
            if (value is null)
            {
                return null;
            }

            return value.Length <= maxLength ? value : value[..maxLength];
        }
    }
}

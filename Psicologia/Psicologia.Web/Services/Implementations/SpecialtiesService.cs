using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Extensions;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.Data;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.Data.Enums;
using Psicologia.Web.DTOs.Specialty;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Services.Implementations
{
    public class SpecialtiesService : CustomQueryableOperationsService, ISpecialtiesService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IAccountService _accountService;
        private readonly IAuditService _auditService;

        public SpecialtiesService(DataContext context,
                                  IMapper mapper,
                                  IAccountService accountService,
                                  IAuditService auditService) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _accountService = accountService;
            _auditService = auditService;
        }

        public async Task<Response<CreateSpecialtyDTO>> CreateAsync(CreateSpecialtyDTO dto)
        {
            try
            {
                string name = dto.Name.Trim();

                bool exists = await _context.Specialties.AnyAsync(e => e.Name.ToLower() == name.ToLower());

                if (exists)
                {
                    return Response<CreateSpecialtyDTO>.Failure($"La especialidad \"{name}\" ya existe.");
                }

                dto.Name = name;
                dto.Description = dto.Description?.Trim();

                // Quien puede aprobar crea la especialidad aprobada; el resto la deja pendiente
                bool canApprove = await _accountService.AccountUserIsAuthorizedAsync(Env.APPROVALS_MANAGE_PERMISSION);
                User? accountUser = await _accountService.GetAccountUserAsync();

                Specialty specialty = new Specialty
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Description = dto.Description,
                    IsActive = true,
                    Status = canApprove ? ApprovalStatus.Approved : ApprovalStatus.Pending,
                    RequestedBy = accountUser?.Email,
                    RequestedAt = DateTime.UtcNow,
                    ReviewedBy = canApprove ? accountUser?.Email : null,
                    ReviewedAt = canApprove ? DateTime.UtcNow : null
                };

                await _context.Specialties.AddAsync(specialty);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.CREATE, "Especialidad", specialty.Id.ToString(),
                                             $"Se registró la especialidad '{specialty.Name}' ({specialty.Status.GetDisplayName()})");

                string message = canApprove
                    ? "Especialidad creada con éxito"
                    : "Especialidad registrada. Queda pendiente de aprobación por la Dirección";

                return Response<CreateSpecialtyDTO>.Success(dto, message);
            }
            catch (Exception ex)
            {
                return Response<CreateSpecialtyDTO>.Failure(ex);
            }
        }

        public async Task<Response> DeleteAsync(Guid id)
        {
            Specialty? specialty = await _context.Specialties.AsNoTracking()
                                                             .FirstOrDefaultAsync(s => s.Id == id);

            if (specialty is null)
            {
                return Response.Failure($"No existe registro con id {id}");
            }

            Response permissionResponse = await CheckCanModifyAsync(specialty);

            if (!permissionResponse.IsSuccess)
            {
                return permissionResponse;
            }

            bool inUse = await _context.DoctorProfiles.AnyAsync(d => d.SpecialtyId == id)
                         || await _context.ClinicalServices.AnyAsync(cs => cs.SpecialtyId == id);

            if (inUse)
            {
                return Response.Failure("La especialidad está asociada a doctores o servicios. Desactívela en lugar de eliminarla.");
            }

            Response response = await DeleteAsync<Specialty>(id);

            if (response.IsSuccess)
            {
                await _auditService.LogAsync(AuditActions.DELETE, "Especialidad", id.ToString(), $"Se eliminó la especialidad '{specialty.Name}'");
            }

            return response;
        }

        public async Task<Response<SpecialtyDTO>> GetOneAsync(Guid id)
        {
            return await GetOneAsync<SpecialtyDTO, Specialty>(id);
        }

        public async Task<Response<PaginationResponse<SpecialtyDTO>>> GetPaginationAsync(PaginationRequest request)
        {
            IQueryable<Specialty> query = _context.Specialties.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                string filter = request.Filter.Trim().ToLower();

                // SELECT * FROM Specialties WHERE LOWER(Name) LIKE '%filter%'
                query = query.Where(e => e.Name.ToLower().Contains(filter));
            }

            query = query.OrderBy(e => e.Name);

            return await GetPagedListAsync<SpecialtyDTO, Specialty>(request, query);
        }

        public async Task<Response<SpecialtyDTO>> UpdateAsync(UpdateSpecialtyDTO dto)
        {
            try
            {
                string name = dto.Name.Trim();

                Specialty? specialty = await _context.Specialties.FirstOrDefaultAsync(s => s.Id == dto.Id);

                if (specialty is null)
                {
                    return Response<SpecialtyDTO>.Failure($"No existe registro con id {dto.Id}");
                }

                Response permissionResponse = await CheckCanModifyAsync(specialty);

                if (!permissionResponse.IsSuccess)
                {
                    return Response<SpecialtyDTO>.Failure(permissionResponse.Message ?? "No autorizado");
                }

                bool exists = await _context.Specialties.AnyAsync(e => e.Id != dto.Id && e.Name.ToLower() == name.ToLower());

                if (exists)
                {
                    return Response<SpecialtyDTO>.Failure($"La especialidad \"{name}\" ya existe.");
                }

                // Se actualizan solo los campos editables: estado y aprobación se conservan
                specialty.Name = name;
                specialty.Description = dto.Description?.Trim();

                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.UPDATE, "Especialidad", specialty.Id.ToString(), $"Se actualizó la especialidad '{specialty.Name}'");

                SpecialtyDTO dtoResponse = _mapper.Map<SpecialtyDTO>(specialty);

                return Response<SpecialtyDTO>.Success(dtoResponse, "Registro actualizado con éxito");
            }
            catch (Exception ex)
            {
                return Response<SpecialtyDTO>.Failure(ex);
            }
        }

        public async Task<Response> ToggleAsync(ToggleSpecialtyStatusDTO dto)
        {
            try
            {
                Specialty? specialty = await _context.Specialties.FirstOrDefaultAsync(s => s.Id == dto.SpecialtyId);

                if (specialty is null)
                {
                    return Response.Failure($"No existe especialidad con id: {dto.SpecialtyId}");
                }

                Response permissionResponse = await CheckCanModifyAsync(specialty);

                if (!permissionResponse.IsSuccess)
                {
                    return permissionResponse;
                }

                specialty.IsActive = !dto.Deactivate;

                await _context.SaveChangesAsync();

                await _auditService.LogAsync(dto.Deactivate ? AuditActions.DEACTIVATE : AuditActions.ACTIVATE,
                                             "Especialidad",
                                             specialty.Id.ToString(),
                                             $"Especialidad '{specialty.Name}' {(dto.Deactivate ? "desactivada" : "activada")}");

                return Response.Success(dto.Deactivate ? "Especialidad desactivada con éxito" : "Especialidad activada con éxito");
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }

        // Quien no puede aprobar solo puede modificar las especialidades pendientes que él mismo solicitó
        private async Task<Response> CheckCanModifyAsync(Specialty specialty)
        {
            if (await _accountService.AccountUserIsAuthorizedAsync(Env.APPROVALS_MANAGE_PERMISSION))
            {
                return Response.Success();
            }

            User? accountUser = await _accountService.GetAccountUserAsync();

            bool isOwnPendingRequest = specialty.Status == ApprovalStatus.Pending
                                       && accountUser is not null
                                       && specialty.RequestedBy == accountUser.Email;

            if (!isOwnPendingRequest)
            {
                return Response.Failure("Solo la Dirección puede modificar especialidades aprobadas o solicitadas por otros usuarios.");
            }

            return Response.Success();
        }
    }
}

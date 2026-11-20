using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.Data;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.Data.Enums;
using Psicologia.Web.DTOs.ClinicalService;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Services.Implementations
{
    // Catálogo de servicios psicológicos
    public class ClinicalServicesService : CustomQueryableOperationsService, IClinicalServicesService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IAuditService _auditService;

        public ClinicalServicesService(DataContext context, IMapper mapper, IAuditService auditService) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _auditService = auditService;
        }

        public async Task<Response<CreateClinicalServiceDTO>> CreateAsync(CreateClinicalServiceDTO dto)
        {
            string name = dto.Name.Trim();

            bool exists = await _context.ClinicalServices.AnyAsync(e => e.Name.ToLower() == name.ToLower());

            if (exists)
            {
                return Response<CreateClinicalServiceDTO>.Failure($"El servicio \"{name}\" ya existe.");
            }

            Response specialtyValidation = await ValidateSpecialtyAsync(dto.SpecialtyId);

            if (!specialtyValidation.IsSuccess)
            {
                return Response<CreateClinicalServiceDTO>.Failure(specialtyValidation.Message ?? "Especialidad inválida");
            }

            dto.Name = name;
            dto.Description = dto.Description?.Trim();

            Response<CreateClinicalServiceDTO> response = await CreateAsync<CreateClinicalServiceDTO, ClinicalService>(dto);

            if (response.IsSuccess)
            {
                await _auditService.LogAsync(AuditActions.CREATE, "Servicio", null, $"Se creó el servicio '{name}'");
            }

            return response;
        }

        public async Task<Response> DeleteAsync(Guid id)
        {
            ClinicalService? service = await _context.ClinicalServices.AsNoTracking()
                                                                      .FirstOrDefaultAsync(cs => cs.Id == id);
            if (service is null)
            {
                return Response.Failure($"No existe registro con id {id}");
            }

            if (await _context.Appointments.AnyAsync(a => a.ClinicalServiceId == id))
            {
                return Response.Failure("El servicio tiene citas asociadas. Desactívelo en lugar de eliminarlo.");
            }

            Response response = await DeleteAsync<ClinicalService>(id);

            if (response.IsSuccess)
            {
                await _auditService.LogAsync(AuditActions.DELETE, "Servicio", id.ToString(), $"Se eliminó el servicio '{service.Name}'");
            }

            return response;
        }

        public async Task<Response<ClinicalServiceDTO>> GetOneAsync(Guid id)
        {
            IQueryable<ClinicalService> query = _context.ClinicalServices.AsNoTracking()
                                                                         .Include(cs => cs.Specialty);

            return await GetOneAsync<ClinicalServiceDTO, ClinicalService>(id, query);
        }

        public async Task<Response<PaginationResponse<ClinicalServiceDTO>>> GetPaginationAsync(PaginationRequest request)
        {
            IQueryable<ClinicalService> query = _context.ClinicalServices.AsNoTracking()
                                                                         .Include(cs => cs.Specialty);

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                string filter = request.Filter.Trim().ToLower();

                query = query.Where(cs => cs.Name.ToLower().Contains(filter)
                                          || (cs.Specialty != null && cs.Specialty.Name.ToLower().Contains(filter)));
            }

            query = query.OrderByDescending(cs => cs.IsActive)
                         .ThenBy(cs => cs.Name);

            return await GetPagedListAsync<ClinicalServiceDTO, ClinicalService>(request, query);
        }

        public async Task<Response<ClinicalServiceDTO>> UpdateAsync(UpdateClinicalServiceDTO dto)
        {
            try
            {
                string name = dto.Name.Trim();

                ClinicalService? service = await _context.ClinicalServices.FirstOrDefaultAsync(cs => cs.Id == dto.Id);

                if (service is null)
                {
                    return Response<ClinicalServiceDTO>.Failure($"No existe registro con id {dto.Id}");
                }

                bool exists = await _context.ClinicalServices.AnyAsync(e => e.Id != dto.Id && e.Name.ToLower() == name.ToLower());

                if (exists)
                {
                    return Response<ClinicalServiceDTO>.Failure($"El servicio \"{name}\" ya existe.");
                }

                Response specialtyValidation = await ValidateSpecialtyAsync(dto.SpecialtyId, service.SpecialtyId);

                if (!specialtyValidation.IsSuccess)
                {
                    return Response<ClinicalServiceDTO>.Failure(specialtyValidation.Message ?? "Especialidad inválida");
                }

                // Se actualizan solo los campos editables: el estado (IsActive) se conserva
                service.Name = name;
                service.Description = dto.Description?.Trim();
                service.DurationMinutes = dto.DurationMinutes;
                service.ReferenceRate = dto.ReferenceRate;
                service.SpecialtyId = dto.SpecialtyId;

                await _context.SaveChangesAsync();

                await _auditService.LogAsync(AuditActions.UPDATE, "Servicio", service.Id.ToString(), $"Se actualizó el servicio '{service.Name}'");

                return Response<ClinicalServiceDTO>.Success(_mapper.Map<ClinicalServiceDTO>(service), "Registro actualizado con éxito");
            }
            catch (Exception ex)
            {
                return Response<ClinicalServiceDTO>.Failure(ex);
            }
        }

        public async Task<Response> ToggleAsync(ToggleClinicalServiceStatusDTO dto)
        {
            try
            {
                ClinicalService? service = await _context.ClinicalServices.FirstOrDefaultAsync(cs => cs.Id == dto.ClinicalServiceId);

                if (service is null)
                {
                    return Response.Failure($"No existe servicio con id: {dto.ClinicalServiceId}");
                }

                service.IsActive = !dto.Deactivate;

                await _context.SaveChangesAsync();

                await _auditService.LogAsync(dto.Deactivate ? AuditActions.DEACTIVATE : AuditActions.ACTIVATE,
                                             "Servicio",
                                             service.Id.ToString(),
                                             $"Servicio '{service.Name}' {(dto.Deactivate ? "desactivado" : "activado")}");

                return Response.Success(dto.Deactivate ? "Servicio desactivado con éxito" : "Servicio activado con éxito");
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }

        // La especialidad es opcional; si se indica debe estar aprobada y activa (o ser la que ya tenía)
        private async Task<Response> ValidateSpecialtyAsync(Guid? specialtyId, Guid? currentSpecialtyId = null)
        {
            if (specialtyId is null || specialtyId == currentSpecialtyId)
            {
                return Response.Success();
            }

            bool valid = await _context.Specialties.AnyAsync(s => s.Id == specialtyId && s.IsActive && s.Status == ApprovalStatus.Approved);

            return valid
                ? Response.Success()
                : Response.Failure("La especialidad seleccionada no existe, no está activa o no ha sido aprobada");
        }
    }
}

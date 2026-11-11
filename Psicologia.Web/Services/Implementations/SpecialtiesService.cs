using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.Data;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.DTOs.Specialty;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Services.Implementations
{
    public class SpecialtiesService : CustomQueryableOperationsService, ISpecialtiesService
    {
        private readonly DataContext _context;

        public SpecialtiesService(DataContext context, IMapper mapper) : base(context, mapper)
        {
            _context = context;
        }

        public async Task<Response<CreateSpecialtyDTO>> CreateAsync(CreateSpecialtyDTO dto)
        {
            string name = dto.Name.Trim();

            bool exists = await _context.Specialties.AnyAsync(e => e.Name.ToLower() == name.ToLower());

            if (exists)
            {
                return Response<CreateSpecialtyDTO>.Failure($"La especialidad \"{name}\" ya existe.");
            }

            dto.Name = name;

            return await CreateAsync<CreateSpecialtyDTO, Specialty>(dto);
        }

        public async Task<Response> DeleteAsync(Guid id)
        {
            return await DeleteAsync<Specialty>(id);
        }

        public async Task<Response<SpecialtyDTO>> GetOneAsync(Guid id)
        {
            return await GetOneAsync<SpecialtyDTO, Specialty>(id);
        }

        public async Task<Response<PaginationResponse<SpecialtyDTO>>> GetPaginationAsync(PaginationRequest request)
        {
            IQueryable<Specialty> query = _context.Specialties.AsQueryable();

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
            string name = dto.Name.Trim();

            bool exists = await _context.Specialties.AnyAsync(e => e.Id != dto.Id && e.Name.ToLower() == name.ToLower());

            if (exists)
            {
                return Response<SpecialtyDTO>.Failure($"La especialidad \"{name}\" ya existe.");
            }

            dto.Name = name;

            Response<UpdateSpecialtyDTO> result = await UpdateAsync<UpdateSpecialtyDTO, Specialty>(dto, dto.Id);

            if (!result.IsSuccess || result.Result is null)
            {
                return Response<SpecialtyDTO>.Failure(result.Message ?? "Ha ocurrido un error", result.Errors);
            }

            SpecialtyDTO dtoResponse = new SpecialtyDTO
            {
                Id = result.Result.Id,
                Name = result.Result.Name
            };

            return Response<SpecialtyDTO>.Success(dtoResponse, result.Message ?? "Registro actualizado con éxito");
        }
    }
}

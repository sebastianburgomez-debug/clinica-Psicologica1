using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.Data;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.DTOs.Role;
using Psicologia.Web.Services.Abstractions;
using System.Text.Json;

namespace Psicologia.Web.Services.Implementations
{
    public class RolesService : CustomQueryableOperationsService, IRolesService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public RolesService(DataContext context, IMapper mapper) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Response<CreateRoleDTO>> CreateAsync(CreateRoleDTO dto)
        {
            string name = dto.Name.Trim();

            bool exists = await _context.PsicologiaRoles.AnyAsync(e => e.Name.ToLower() == name.ToLower());

            if (exists)
            {
                return Response<CreateRoleDTO>.Failure($"El rol \"{name}\" ya existe.");
            }

            dto.Name = name;

            using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Role
                    Role role = _mapper.Map<Role>(dto);

                    role.Id = Guid.NewGuid();

                    await _context.PsicologiaRoles.AddAsync(role);

                    await _context.SaveChangesAsync();

                    // Permissions
                    List<Guid> permissionIds = GetPermissionIds(dto.PermissionIds);

                    foreach (Guid permissionId in permissionIds)
                    {
                        RolePermission rolePermission = new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = permissionId
                        };

                        await _context.RolePermissions.AddAsync(rolePermission);
                    }

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return Response<CreateRoleDTO>.Success(dto, "Rol creado con éxito");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Response<CreateRoleDTO>.Failure(ex);
                }
            }
        }

        public async Task<Response> DeleteAsync(Guid id)
        {
            Role? role = await _context.PsicologiaRoles.AsNoTracking()
                                                       .FirstOrDefaultAsync(r => r.Id == id);

            if (role?.Name == Env.SUPER_ADMIN_ROLE_NAME)
            {
                return Response.Failure("No se puede eliminar el rol de Super Administrador.");
            }

            if (await _context.Users.AnyAsync(u => u.RoleId == id))
            {
                return Response.Failure("No se puede eliminar el rol porque hay usuarios asociados a él.");
            }

            return await DeleteAsync<Role>(id);
        }

        public async Task<Response<RoleDTO>> GetOneAsync(Guid id)
        {
            Response<RoleDTO> response = await GetOneAsync<RoleDTO, Role>(id);

            if (!response.IsSuccess || response.Result is null)
            {
                return response;
            }

            RoleDTO dto = response.Result;

            List<PermissionsForRoleDTO> permissions = await _context.Permissions.OrderBy(p => p.Module)
                                                                                .Select(p => new PermissionsForRoleDTO
            {
                Id = p.Id,
                Name = p.Name,
                Module = p.Module,
                Code = p.Code,
                Selected = _context.RolePermissions.Any(rp => rp.PermissionId == p.Id && rp.RoleId == dto.Id)
            }).ToListAsync();

            dto.Permissions = permissions;

            return Response<RoleDTO>.Success(dto);
        }

        public async Task<Response<PaginationResponse<RoleDTO>>> GetPaginationAsync(PaginationRequest request)
        {
            IQueryable<Role> query = _context.PsicologiaRoles.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                string filter = request.Filter.Trim().ToLower();

                // SELECT * FROM Roles WHERE LOWER(Name) LIKE '%filter%'
                query = query.Where(e => e.Name.ToLower().Contains(filter));
            }

            query = query.OrderBy(e => e.Name);

            return await GetPagedListAsync<RoleDTO, Role>(request, query);
        }

        public async Task<Response<List<PermissionsForRoleDTO>>> GetPermissionsAsync()
        {
            IQueryable<Permission> query = _context.Permissions.AsNoTracking()
                                                               .OrderBy(p => p.Module);

            Response<List<PermissionDTO>> permissionsResponse = await GetCompleteListAsync<PermissionDTO, Permission>(query);

            if (!permissionsResponse.IsSuccess || permissionsResponse.Result is null)
            {
                return Response<List<PermissionsForRoleDTO>>.Failure(permissionsResponse.Message ?? "Ha ocurrido un error", permissionsResponse.Errors);
            }

            List<PermissionsForRoleDTO> permissions = permissionsResponse.Result.Select(p => new PermissionsForRoleDTO
            {
                Id = p.Id,
                Name = p.Name,
                Module = p.Module,
                Code = p.Code,
                Selected = false,
            }).ToList();

            return Response<List<PermissionsForRoleDTO>>.Success(permissions);
        }

        public async Task<Response<RoleDTO>> UpdateAsync(UpdateRoleDTO dto)
        {
            try
            {
                string name = dto.Name.Trim();

                Role? currentRole = await _context.PsicologiaRoles.AsNoTracking()
                                                                  .FirstOrDefaultAsync(r => r.Id == dto.Id);

                if (currentRole is null)
                {
                    return Response<RoleDTO>.Failure($"No existe registro con id {dto.Id}");
                }

                if (currentRole.Name == Env.SUPER_ADMIN_ROLE_NAME || name == Env.SUPER_ADMIN_ROLE_NAME)
                {
                    return Response<RoleDTO>.Failure("No se puede modificar el rol de Super Administrador.");
                }

                bool exists = await _context.PsicologiaRoles.AnyAsync(e => e.Id != dto.Id && e.Name.ToLower() == name.ToLower());

                if (exists)
                {
                    return Response<RoleDTO>.Failure($"El rol \"{name}\" ya existe.");
                }

                dto.Name = name;

                // Role
                Role role = _mapper.Map<Role>(dto);

                _context.PsicologiaRoles.Update(role);

                // Permissions
                List<Guid> permissionIds = GetPermissionIds(dto.PermissionIds);

                // Delete old permissions
                List<RolePermission> oldRolePermissions = await _context.RolePermissions.Where(rp => rp.RoleId == role.Id)
                                                                                        .ToListAsync();
                _context.RolePermissions.RemoveRange(oldRolePermissions);

                // Create new permissions
                foreach (Guid permissionId in permissionIds)
                {
                    RolePermission rolePermission = new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissionId
                    };

                    await _context.RolePermissions.AddAsync(rolePermission);
                }

                await _context.SaveChangesAsync();

                RoleDTO dtoResponse = new RoleDTO
                {
                    Id = role.Id,
                    Name = role.Name
                };

                return Response<RoleDTO>.Success(dtoResponse, "Rol actualizado con éxito");
            }
            catch (Exception ex)
            {
                return Response<RoleDTO>.Failure(ex);
            }
        }

        // PermissionIds llega desde la vista como un arreglo JSON: ["guid1","guid2"]
        private static List<Guid> GetPermissionIds(string? permissionIds)
        {
            if (string.IsNullOrWhiteSpace(permissionIds))
            {
                return [];
            }

            return JsonSerializer.Deserialize<List<Guid>>(permissionIds) ?? [];
        }
    }
}

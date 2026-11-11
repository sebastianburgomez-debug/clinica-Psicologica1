using Psicologia.Web.Core;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.DTOs.Role;

namespace Psicologia.Web.Services.Abstractions
{
    public interface IRolesService
    {
        public Task<Response<CreateRoleDTO>> CreateAsync(CreateRoleDTO dto);
        public Task<Response> DeleteAsync(Guid id);
        public Task<Response<RoleDTO>> GetOneAsync(Guid id);
        public Task<Response<PaginationResponse<RoleDTO>>> GetPaginationAsync(PaginationRequest request);
        public Task<Response<RoleDTO>> UpdateAsync(UpdateRoleDTO dto);
        public Task<Response<List<PermissionsForRoleDTO>>> GetPermissionsAsync();
    }
}

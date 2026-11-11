using Microsoft.AspNetCore.Identity;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.DTOs.User;

namespace Psicologia.Web.Services.Abstractions
{
    public interface IUsersService
    {
        public Task<Response<IdentityResult>> AddUserAsync(User user, string password);
        public Task<Response<UserDTO>> CreateAsync(CreateUserDTO dto);
        public Task<Response> DeleteAsync(Guid id);
        public Task<Response<UserDTO>> GetOneAsync(Guid id);
        public Task<Response<PaginationResponse<UserDTO>>> GetPaginationAsync(PaginationRequest request);
        public Task<Response<User>> GetUserByEmailAsync(string email);
        public Task<Response<User>> GetUserByIdAsync(string id);
        public Task<Response<UserDTO>> UpdateUserAsync(UpdateUserDTO dto);
    }
}

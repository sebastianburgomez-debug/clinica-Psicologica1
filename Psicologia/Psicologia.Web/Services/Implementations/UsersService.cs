using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Psicologia.Web.Core;
using Psicologia.Web.Core.Pagination;
using Psicologia.Web.Data;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.DTOs.User;
using Psicologia.Web.Services.Abstractions;

namespace Psicologia.Web.Services.Implementations
{
    public class UsersService : IUsersService
    {
        private readonly UserManager<User> _userManager;
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IAuditService _auditService;

        public UsersService(UserManager<User> userManager, DataContext context, IMapper mapper, IAuditService auditService)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
            _auditService = auditService;
        }

        public async Task<Response<IdentityResult>> AddUserAsync(User user, string password)
        {
            IdentityResult result = await _userManager.CreateAsync(user, password);

            return new Response<IdentityResult>
            {
                Result = result,
                IsSuccess = result.Succeeded,
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        public async Task<Response<UserDTO>> CreateAsync(CreateUserDTO dto)
        {
            try
            {
                string email = dto.Email.Trim();

                bool exists = await _context.Users.AnyAsync(u => u.Email == email);

                if (exists)
                {
                    return Response<UserDTO>.Failure($"Ya existe un usuario con el correo \"{email}\".");
                }

                dto.Email = email;
                dto.Name = dto.Name.Trim();

                User user = _mapper.Map<User>(dto);

                user.Id = Guid.NewGuid().ToString();

                Response<IdentityResult> response = await AddUserAsync(user, dto.Password);

                if (!response.IsSuccess)
                {
                    return Response<UserDTO>.Failure("No se pudo crear el usuario", response.Errors);
                }

                // TODO: Send token by email for confirmation
                Response<string> token = await GenerateConfirmationTokenAsync(user);

                await ConfirmUserAsync(user, token.Result!);

                UserDTO userDto = _mapper.Map<UserDTO>(user);

                await _auditService.LogAsync(AuditActions.CREATE, "Usuario", user.Id, $"Se creó el usuario '{user.Name}' ({user.Email})");

                return Response<UserDTO>.Success(userDto, "Usuario creado con éxito");
            }
            catch (Exception ex)
            {
                return Response<UserDTO>.Failure(ex);
            }
        }

        public async Task<Response> DeleteAsync(Guid id)
        {
            try
            {
                User? user = await _userManager.FindByIdAsync(id.ToString());

                if (user is null)
                {
                    return Response.Failure($"No existe usuario con id: '{id}'");
                }

                // Un usuario con información clínica o profesional asociada no se elimina físicamente:
                // debe desactivarse (borrado lógico) desde el módulo de Personal.
                bool hasRelatedData = await _context.DoctorProfiles.AnyAsync(d => d.UserId == user.Id)
                                      || await _context.Appointments.AnyAsync(a => a.PatientId == user.Id)
                                      || await _context.ClinicalRecords.AnyAsync(r => r.PatientId == user.Id);

                if (hasRelatedData)
                {
                    return Response.Failure("El usuario tiene información clínica o profesional asociada. Desactívelo en lugar de eliminarlo.");
                }

                string description = $"Se eliminó el usuario '{user.Name}' ({user.Email})";

                IdentityResult result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    return Response.Failure("No se pudo eliminar el usuario", result.Errors.Select(e => e.Description).ToList());
                }

                await _auditService.LogAsync(AuditActions.DELETE, "Usuario", id.ToString(), description);

                return Response.Success("Usuario eliminado con éxito");
            }
            catch (Exception ex)
            {
                return Response.Failure(ex);
            }
        }

        public async Task<Response<PaginationResponse<UserDTO>>> GetPaginationAsync(PaginationRequest request)
        {
            try
            {
                IQueryable<User> queryable = _context.Users.Include(u => u.Role)
                                                           .AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.Filter))
                {
                    string filter = request.Filter.Trim().ToLower();

                    queryable = queryable.Where(u => u.Name.ToLower().Contains(filter)
                                                     || u.Email!.ToLower().Contains(filter)
                                                     || u.Role!.Name.ToLower().Contains(filter));
                }

                queryable = queryable.OrderBy(u => u.Name);

                PagedList<User> list = await PagedList<User>.ToPagedListAsync(queryable, request);

                PaginationResponse<UserDTO> dto = new PaginationResponse<UserDTO>
                {
                    List = _mapper.Map<PagedList<UserDTO>>(list),
                    CurrentPage = list.CurrentPage,
                    TotalPages = list.TotalPages,
                    RecordsPerPage = list.RecordsPerPage,
                    TotalCount = list.TotalCount,
                    Filter = request.Filter,
                };

                return Response<PaginationResponse<UserDTO>>.Success(dto);
            }
            catch (Exception ex)
            {
                return Response<PaginationResponse<UserDTO>>.Failure(ex);
            }
        }

        public async Task<Response<User>> GetUserByEmailAsync(string email)
        {
            try
            {
                User? user = await _context.Users.Include(u => u.Role)
                                                 .FirstOrDefaultAsync(u => u.Email == email);

                if (user is null)
                {
                    return Response<User>.Failure($"No existe usuario con email: '{email}'");
                }

                return Response<User>.Success(user);
            }
            catch (Exception ex)
            {
                return Response<User>.Failure(ex);
            }
        }

        public async Task<Response<User>> GetUserByIdAsync(string id)
        {
            try
            {
                User? user = await _context.Users.Include(u => u.Role)
                                                 .FirstOrDefaultAsync(u => u.Id == id);

                if (user is null)
                {
                    return Response<User>.Failure($"No existe usuario con id: '{id}'");
                }

                return Response<User>.Success(user);
            }
            catch (Exception ex)
            {
                return Response<User>.Failure(ex);
            }
        }

        public async Task<Response<UserDTO>> GetOneAsync(Guid id)
        {
            try
            {
                Response<User> response = await GetUserByIdAsync(id.ToString());

                if (!response.IsSuccess)
                {
                    return Response<UserDTO>.Failure(response.Message ?? "Ha ocurrido un error", response.Errors);
                }

                UserDTO dto = _mapper.Map<UserDTO>(response.Result);

                return Response<UserDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                return Response<UserDTO>.Failure(ex);
            }
        }

        public async Task<Response<UserDTO>> UpdateUserAsync(UpdateUserDTO dto)
        {
            try
            {
                Response<User> response = await GetUserByIdAsync(dto.Id.ToString()!);

                if (!response.IsSuccess || response.Result is null)
                {
                    return Response<UserDTO>.Failure(response.Message ?? "Ha ocurrido un error", response.Errors);
                }

                User user = response.Result;
                Guid previousRoleId = user.RoleId;

                user.Name = dto.Name.Trim();
                user.RoleId = dto.RoleId == Guid.Empty ? user.RoleId : dto.RoleId;

                _context.Users.Update(user);

                await _context.SaveChangesAsync();

                string roleChange = previousRoleId != user.RoleId ? " (cambio de rol)" : string.Empty;
                await _auditService.LogAsync(AuditActions.UPDATE, "Usuario", user.Id, $"Se actualizó el usuario '{user.Name}' ({user.Email}){roleChange}");

                UserDTO userDto = _mapper.Map<UserDTO>(user);

                return Response<UserDTO>.Success(userDto, "Usuario actualizado con éxito");
            }
            catch (Exception ex)
            {
                return Response<UserDTO>.Failure(ex);
            }
        }

        private async Task<Response<IdentityResult>> ConfirmUserAsync(User user, string token)
        {
            IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);

            return new Response<IdentityResult>
            {
                Result = result,
                IsSuccess = result.Succeeded
            };
        }

        private async Task<Response<string>> GenerateConfirmationTokenAsync(User user)
        {
            string result = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            return Response<string>.Success(result);
        }
    }
}

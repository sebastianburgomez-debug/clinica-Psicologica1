using AutoMapper;
using Psicologia.Web.Data.Entities;
using Psicologia.Web.DTOs.Account;
using Psicologia.Web.DTOs.Role;
using Psicologia.Web.DTOs.Specialty;
using Psicologia.Web.DTOs.User;

namespace Psicologia.Web.Core
{
    public class AutoMapperProfiles : Profile
    {//de dtos a clases normales
        public AutoMapperProfiles()
        {
            CreateMap<Specialty, CreateSpecialtyDTO>().ReverseMap();
            CreateMap<Specialty, UpdateSpecialtyDTO>().ReverseMap();
            CreateMap<Specialty, SpecialtyDTO>().ReverseMap();

            CreateMap<Role, CreateRoleDTO>().ReverseMap();
            CreateMap<Role, UpdateRoleDTO>().ReverseMap();
            CreateMap<Role, RoleDTO>().ReverseMap();

            CreateMap<Permission, PermissionDTO>().ReverseMap();

            CreateMap<User, AccountUserDTO>().ForMember(dto => dto.Id, entity => entity.MapFrom(e => Guid.Parse(e.Id)))
                                             .ForMember(dto => dto.RoleName, entity => entity.MapFrom(e => e.Role!.Name));

            CreateMap<User, UserDTO>().ForMember(dto => dto.Id, entity => entity.MapFrom(e => Guid.Parse(e.Id)))
                                      .ForMember(dto => dto.Roles, entity => entity.Ignore());

            CreateMap<User, CreateUserDTO>().ForMember(dto => dto.Password, entity => entity.Ignore())
                                            .ForMember(dto => dto.Roles, entity => entity.Ignore())
                                            .ReverseMap()
                                            .ForMember(e => e.UserName, dto => dto.MapFrom(d => d.Email))
                                            .ForMember(e => e.Role, dto => dto.Ignore());

            CreateMap<User, UpdateUserDTO>().ForMember(dto => dto.Id, entity => entity.MapFrom(e => Guid.Parse(e.Id)))
                                            .ForMember(dto => dto.Roles, entity => entity.Ignore());
        }
    }
}

using AutoMapper;
using Shop_ProjForWeb.Application.DTOs.Permission;
using Shop_ProjForWeb.Core.Domain.Entities;



namespace Shop_ProjForWeb.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Permission, PermissionDto>().ReverseMap();
        CreateMap<Permission, CreatePermissionDto>().ReverseMap();
        CreateMap<Permission, UpdatePermissionDto>().ReverseMap();
    }
}


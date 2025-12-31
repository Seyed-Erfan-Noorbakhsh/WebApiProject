using Shop_ProjForWeb.Application.DTOs.Permission;

namespace Shop_ProjForWeb.Application.Interfaces;

public interface IPermissionService
{
    Task<PermissionDto?> GetPermissionByIdAsync(int id);
    Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
    Task<PermissionDto> CreatePermissionAsync(CreatePermissionDto dto);
    Task<PermissionDto> UpdatePermissionAsync(int id, UpdatePermissionDto dto);
    Task<bool> DeletePermissionAsync(int id);
}


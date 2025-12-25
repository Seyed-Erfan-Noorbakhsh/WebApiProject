using Shop_ProjForWeb.Application.DTOs.Role;

namespace Shop_ProjForWeb.Application.Interfaces;

public interface IRoleService
{
    Task<RoleDto?> GetRoleByIdAsync(int id);
    Task<RoleDto?> GetRoleByNameAsync(string name);
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);
    Task<RoleDto> UpdateRoleAsync(int id, UpdateRoleDto dto);
    Task<bool> DeleteRoleAsync(int id);
    Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId);
    Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId);
}


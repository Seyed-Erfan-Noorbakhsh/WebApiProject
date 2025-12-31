using AutoMapper;
using Shop_ProjForWeb.Application.DTOs.Role;
using Shop_ProjForWeb.Application.Interfaces;
using Shop_ProjForWeb.Domain.Entities;
using Shop_ProjForWeb.Domain.Interfaces;

namespace Shop_ProjForWeb.Application.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public RoleService(IUnitOfWork unitOfWork, IMapper mapper, ILoggerService logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<RoleDto?> GetRoleByIdAsync(int id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        return role == null ? null : await MapToDtoAsync(role);
    }

    public async Task<RoleDto?> GetRoleByNameAsync(string name)
    {
        var roles = await _unitOfWork.Roles.FindAsync(r => r.Name == name && !r.IsDeleted);
        var role = roles.FirstOrDefault();
        return role == null ? null : await MapToDtoAsync(role);
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _unitOfWork.Roles.FindAsync(r => !r.IsDeleted);
        var roleDtos = new List<RoleDto>();
        
        foreach (var role in roles)
        {
            roleDtos.Add(await MapToDtoAsync(role));
        }
        
        return roleDtos;
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
    {
        _logger.LogInformation("Creating role: {RoleName}", dto.Name);

        var existingRole = (await _unitOfWork.Roles.FindAsync(r => r.Name == dto.Name)).FirstOrDefault();
        if (existingRole != null)
        {
            throw new InvalidOperationException("Role name already exists");
        }

        var role = new Role
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Roles.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();

        // Assign permissions
        foreach (var permissionId in dto.PermissionIds)
        {
            var permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId);
            if (permission != null)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permissionId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.RolePermissions.AddAsync(rolePermission);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Role {RoleName} created successfully", role.Name);

        return await MapToDtoAsync(role);
    }

    public async Task<RoleDto> UpdateRoleAsync(int id, UpdateRoleDto dto)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null)
        {
            throw new KeyNotFoundException("Role not found");
        }

        if (!string.IsNullOrEmpty(dto.Name))
            role.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Description))
            role.Description = dto.Description;
        if (dto.IsActive.HasValue)
            role.IsActive = dto.IsActive.Value;

        role.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Roles.UpdateAsync(role);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Role {RoleId} updated successfully", id);
        return await MapToDtoAsync(role);
    }

    public async Task<bool> DeleteRoleAsync(int id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null)
        {
            return false;
        }

        role.IsDeleted = true;
        role.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Roles.UpdateAsync(role);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Role {RoleId} deleted", id);
        return true;
    }

    public async Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId)
    {
        var existing = (await _unitOfWork.RolePermissions.FindAsync(rp => 
            rp.RoleId == roleId && rp.PermissionId == permissionId)).FirstOrDefault();

        if (existing != null)
        {
            return false;
        }

        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.RolePermissions.AddAsync(rolePermission);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Permission {PermissionId} assigned to role {RoleId}", permissionId, roleId);
        return true;
    }

    public async Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId)
    {
        var rolePermission = (await _unitOfWork.RolePermissions.FindAsync(rp => 
            rp.RoleId == roleId && rp.PermissionId == permissionId)).FirstOrDefault();

        if (rolePermission == null)
        {
            return false;
        }

        await _unitOfWork.RolePermissions.DeleteAsync(rolePermission);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Permission {PermissionId} removed from role {RoleId}", permissionId, roleId);
        return true;
    }

    private async Task<RoleDto> MapToDtoAsync(Role role)
    {
        var permissions = role.RolePermissions
            .Select(rp => $"{rp.Permission.Resource}.{rp.Permission.Action}")
            .ToList();

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsActive = role.IsActive,
            Permissions = permissions
        };
    }
}


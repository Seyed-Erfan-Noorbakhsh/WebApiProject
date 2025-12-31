using AutoMapper;
using Shop_ProjForWeb.Application.DTOs.Permission;
using Shop_ProjForWeb.Application.Interfaces;
using Shop_ProjForWeb.Domain.Entities;
using Shop_ProjForWeb.Domain.Interfaces;

namespace Shop_ProjForWeb.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public PermissionService(IUnitOfWork unitOfWork, IMapper mapper, ILoggerService logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PermissionDto?> GetPermissionByIdAsync(int id)
    {
        var permission = await _unitOfWork.Permissions.GetByIdAsync(id);
        return permission == null ? null : _mapper.Map<PermissionDto>(permission);
    }

    public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
    {
        var permissions = await _unitOfWork.Permissions.FindAsync(p => !p.IsDeleted);
        return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
    }

    public async Task<PermissionDto> CreatePermissionAsync(CreatePermissionDto dto)
    {
        _logger.LogInformation("Creating permission: {PermissionName}", dto.Name);

        var existingPermission = (await _unitOfWork.Permissions.FindAsync(p => 
            p.Name == dto.Name || (p.Resource == dto.Resource && p.Action == dto.Action))).FirstOrDefault();

        if (existingPermission != null)
        {
            throw new InvalidOperationException("Permission already exists");
        }

        var permission = new Permission
        {
            Name = dto.Name,
            Description = dto.Description,
            Resource = dto.Resource,
            Action = dto.Action,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Permissions.AddAsync(permission);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Permission {PermissionName} created successfully", permission.Name);
        return _mapper.Map<PermissionDto>(permission);
    }

    public async Task<PermissionDto> UpdatePermissionAsync(int id, UpdatePermissionDto dto)
    {
        var permission = await _unitOfWork.Permissions.GetByIdAsync(id);
        if (permission == null)
        {
            throw new KeyNotFoundException("Permission not found");
        }

        if (!string.IsNullOrEmpty(dto.Name))
            permission.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Description))
            permission.Description = dto.Description;
        if (!string.IsNullOrEmpty(dto.Resource))
            permission.Resource = dto.Resource;
        if (!string.IsNullOrEmpty(dto.Action))
            permission.Action = dto.Action;

        permission.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Permissions.UpdateAsync(permission);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Permission {PermissionId} updated successfully", id);
        return _mapper.Map<PermissionDto>(permission);
    }

    public async Task<bool> DeletePermissionAsync(int id)
    {
        var permission = await _unitOfWork.Permissions.GetByIdAsync(id);
        if (permission == null)
        {
            return false;
        }

        permission.IsDeleted = true;
        permission.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Permissions.UpdateAsync(permission);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Permission {PermissionId} deleted", id);
        return true;
    }
}


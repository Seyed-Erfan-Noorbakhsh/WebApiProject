using AutoMapper;
using Shop_ProjForWeb.Application.DTOs.User;
using Shop_ProjForWeb.Application.Interfaces;
using Shop_ProjForWeb.Core.Domain.Entities;
using Shop_ProjForWeb.Core.Application.DTOs.User;
using Shop_ProjForWeb.Domain.Interfaces;
using BCrypt.Net;

namespace Shop_ProjForWeb.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILoggerService logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        return user == null ? null : await MapToDtoAsync(user);
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var users = await _unitOfWork.Users.FindAsync(u => u.Username == username && !u.IsDeleted);
        var user = users.FirstOrDefault();
        return user == null ? null : await MapToDtoAsync(user);
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var users = await _unitOfWork.Users.FindAsync(u => u.Email == email && !u.IsDeleted);
        var user = users.FirstOrDefault();
        return user == null ? null : await MapToDtoAsync(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.Users.FindAsync(u => !u.IsDeleted);
        var userDtos = new List<UserDto>();
        
        foreach (var user in users)
        {
            userDtos.Add(await MapToDtoAsync(user));
        }
        
        return userDtos;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        _logger.LogInformation("Creating user: {Username}", dto.Username);

        var existingUser = (await _unitOfWork.Users.FindAsync(u => 
            u.Username == dto.Username || u.Email == dto.Email)).FirstOrDefault();

        if (existingUser != null)
        {
            throw new InvalidOperationException("Username or email already exists");
        }

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assign roles
        foreach (var roleId in dto.RoleIds)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
            if (role != null)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.UserRoles.AddAsync(userRole);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("User {Username} created successfully", user.Username);

        return await MapToDtoAsync(user);
    }

    public async Task<UserDto> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        if (!string.IsNullOrEmpty(dto.Email))
            user.Email = dto.Email;
        if (!string.IsNullOrEmpty(dto.FirstName))
            user.FirstName = dto.FirstName;
        if (!string.IsNullOrEmpty(dto.LastName))
            user.LastName = dto.LastName;
        if (dto.IsActive.HasValue)
            user.IsActive = dto.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User {UserId} updated successfully", id);
        return await MapToDtoAsync(user);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            return false;
        }

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User {UserId} deleted", id);
        return true;
    }

    public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
    {
        var existing = (await _unitOfWork.UserRoles.FindAsync(ur => 
            ur.UserId == userId && ur.RoleId == roleId)).FirstOrDefault();

        if (existing != null)
        {
            return false;
        }

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.UserRoles.AddAsync(userRole);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Role {RoleId} assigned to user {UserId}", roleId, userId);
        return true;
    }

    public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
    {
        var userRole = (await _unitOfWork.UserRoles.FindAsync(ur => 
            ur.UserId == userId && ur.RoleId == roleId)).FirstOrDefault();

        if (userRole == null)
        {
            return false;
        }

        await _unitOfWork.UserRoles.DeleteAsync(userRole);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Role {RoleId} removed from user {UserId}", roleId, userId);
        return true;
    }

    public async Task<bool> ActivateUserAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeactivateUserAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private async Task<UserDto> MapToDtoAsync(User user)
    {
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => $"{rp.Permission.Resource}.{rp.Permission.Action}")
            .Distinct()
            .ToList();

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            LastLoginAt = user.LastLoginAt,
            Roles = roles,
            Permissions = permissions
        };
    }
}


using Microsoft.EntityFrameworkCore;
using Shop_ProjForWeb.Core.Domain.Entities;

namespace Shop_ProjForWeb.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Roles.AnyAsync())
            return;

        var adminRole = new Role
        {
            Name = "Admin",
            Description = "Administrator role with full access",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var userRole = new Role
        {
            Name = "User",
            Description = "Standard user role",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Roles.AddRangeAsync(adminRole, userRole);
        await context.SaveChangesAsync();

        var permissions = new List<Permission>
        {
            new() { Name = "Product.Create", Resource = "Product", Action = "Create", CreatedAt = DateTime.UtcNow },
            new() { Name = "Product.Read",   Resource = "Product", Action = "Read",   CreatedAt = DateTime.UtcNow },
            new() { Name = "Product.Update", Resource = "Product", Action = "Update", CreatedAt = DateTime.UtcNow },
            new() { Name = "Product.Delete", Resource = "Product", Action = "Delete", CreatedAt = DateTime.UtcNow },
            new() { Name = "User.Create",    Resource = "User",    Action = "Create", CreatedAt = DateTime.UtcNow },
            new() { Name = "User.Read",      Resource = "User",    Action = "Read",   CreatedAt = DateTime.UtcNow },
            new() { Name = "User.Update",    Resource = "User",    Action = "Update", CreatedAt = DateTime.UtcNow },
            new() { Name = "User.Delete",    Resource = "User",    Action = "Delete", CreatedAt = DateTime.UtcNow }
        };

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();

        var adminRolePermissions = permissions.Select(p => new RolePermission
        {
            RoleId = adminRole.Id,
            PermissionId = p.Id,
            CreatedAt = DateTime.UtcNow
        });

        await context.RolePermissions.AddRangeAsync(adminRolePermissions);
        await context.SaveChangesAsync();

        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@shop.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            FirstName = "Admin",
            LastName = "User",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Users.AddAsync(adminUser);
        await context.SaveChangesAsync();

        await context.UserRoles.AddAsync(new UserRole
        {
            UserId = adminUser.Id,
            RoleId = adminRole.Id,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }
}

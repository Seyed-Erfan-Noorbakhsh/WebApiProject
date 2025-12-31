using Microsoft.EntityFrameworkCore;
using Shop_ProjForWeb.Domain.Entities;
using BCrypt.Net;

namespace Shop_ProjForWeb.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Roles.AnyAsync())
        {
            return; // Database already seeded
        }

        // Create default roles
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

        // Create default permissions
        var permissions = new List<Permission>
        {
            new() { Name = "Product.Create", Description = "Create products", Resource = "Product", Action = "Create", CreatedAt = DateTime.UtcNow },
            new() { Name = "Product.Read", Description = "Read products", Resource = "Product", Action = "Read", CreatedAt = DateTime.UtcNow },
            new() { Name = "Product.Update", Description = "Update products", Resource = "Product", Action = "Update", CreatedAt = DateTime.UtcNow },
            new() { Name = "Product.Delete", Description = "Delete products", Resource = "Product", Action = "Delete", CreatedAt = DateTime.UtcNow },
            new() { Name = "User.Create", Description = "Create users", Resource = "User", Action = "Create", CreatedAt = DateTime.UtcNow },
            new() { Name = "User.Read", Description = "Read users", Resource = "User", Action = "Read", CreatedAt = DateTime.UtcNow },
            new() { Name = "User.Update", Description = "Update users", Resource = "User", Action = "Update", CreatedAt = DateTime.UtcNow },
            new() { Name = "User.Delete", Description = "Delete users", Resource = "User", Action = "Delete", CreatedAt = DateTime.UtcNow },
            new() { Name = "Role.Create", Description = "Create roles", Resource = "Role", Action = "Create", CreatedAt = DateTime.UtcNow },
            new() { Name = "Role.Read", Description = "Read roles", Resource = "Role", Action = "Read", CreatedAt = DateTime.UtcNow },
            new() { Name = "Role.Update", Description = "Update roles", Resource = "Role", Action = "Update", CreatedAt = DateTime.UtcNow },
            new() { Name = "Role.Delete", Description = "Delete roles", Resource = "Role", Action = "Delete", CreatedAt = DateTime.UtcNow }
        };

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();

        // Assign all permissions to Admin role
        var adminRolePermissions = permissions.Select(p => new RolePermission
        {
            RoleId = adminRole.Id,
            PermissionId = p.Id,
            CreatedAt = DateTime.UtcNow
        });

        await context.RolePermissions.AddRangeAsync(adminRolePermissions);
        await context.SaveChangesAsync();

        // Create default admin user
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

        // Assign Admin role to admin user
        var adminUserRole = new UserRole
        {
            UserId = adminUser.Id,
            RoleId = adminRole.Id,
            CreatedAt = DateTime.UtcNow
        };

        await context.UserRoles.AddAsync(adminUserRole);
        await context.SaveChangesAsync();
    }
}


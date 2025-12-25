using Shop_ProjForWeb.Core.Domain.Entities;

namespace Shop_ProjForWeb.Core.Domain.Interfaces;
public interface IUnitOfWork
{
    IRepository<User> Users { get; }
    IRepository<Role> Roles { get; }
    IRepository<Permission> Permissions { get; }
    IRepository<UserRole> UserRoles { get; }
    IRepository<RolePermission> RolePermissions { get; }
    IRepository<AuditLog> AuditLogs { get; }
    IRepository<RefreshToken> RefreshTokens { get; }
    IRepository<EmailVerificationToken> EmailVerificationTokens { get; }
    IRepository<PasswordResetToken> PasswordResetTokens { get; }

    Task<int> SaveChangesAsync();
}
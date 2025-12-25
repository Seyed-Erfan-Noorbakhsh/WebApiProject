using Shop_ProjForWeb.Domain.Entities;

namespace Shop_ProjForWeb.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
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
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}


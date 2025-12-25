using Shop_ProjForWeb.Domain.Entities;
using Shop_ProjForWeb.Domain.Interfaces;
using Shop_ProjForWeb.Infrastructure.Data;

namespace Shop_ProjForWeb.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<User> Users => new Repository<User>(_context);
    public IRepository<Role> Roles => new Repository<Role>(_context);
    public IRepository<Permission> Permissions => new Repository<Permission>(_context);
    public IRepository<UserRole> UserRoles => new Repository<UserRole>(_context);
    public IRepository<RolePermission> RolePermissions => new Repository<RolePermission>(_context);
    public IRepository<AuditLog> AuditLogs => new Repository<AuditLog>(_context);
    public IRepository<RefreshToken> RefreshTokens => new Repository<RefreshToken>(_context);
    public IRepository<EmailVerificationToken> EmailVerificationTokens => new Repository<EmailVerificationToken>(_context);
    public IRepository<PasswordResetToken> PasswordResetTokens => new Repository<PasswordResetToken>(_context);

    public Task<int> SaveChangesAsync()
        => _context.SaveChangesAsync();
}

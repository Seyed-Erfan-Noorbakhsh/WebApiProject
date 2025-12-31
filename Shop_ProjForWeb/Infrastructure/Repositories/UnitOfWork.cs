using Microsoft.EntityFrameworkCore.Storage;
using Shop_ProjForWeb.Domain.Entities;
using Shop_ProjForWeb.Domain.Interfaces;
using Shop_ProjForWeb.Infrastructure.Data;

namespace Shop_ProjForWeb.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    private IRepository<User>? _users;
    private IRepository<Role>? _roles;
    private IRepository<Permission>? _permissions;
    private IRepository<UserRole>? _userRoles;
    private IRepository<RolePermission>? _rolePermissions;
    private IRepository<AuditLog>? _auditLogs;
    private IRepository<RefreshToken>? _refreshTokens;
    private IRepository<EmailVerificationToken>? _emailVerificationTokens;
    private IRepository<PasswordResetToken>? _passwordResetTokens;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<User> Users =>
        _users ??= new Repository<User>(_context);

    public IRepository<Role> Roles =>
        _roles ??= new Repository<Role>(_context);

    public IRepository<Permission> Permissions =>
        _permissions ??= new Repository<Permission>(_context);

    public IRepository<UserRole> UserRoles =>
        _userRoles ??= new Repository<UserRole>(_context);

    public IRepository<RolePermission> RolePermissions =>
        _rolePermissions ??= new Repository<RolePermission>(_context);

    public IRepository<AuditLog> AuditLogs =>
        _auditLogs ??= new Repository<AuditLog>(_context);

    public IRepository<RefreshToken> RefreshTokens =>
        _refreshTokens ??= new Repository<RefreshToken>(_context);

    public IRepository<EmailVerificationToken> EmailVerificationTokens =>
        _emailVerificationTokens ??= new Repository<EmailVerificationToken>(_context);

    public IRepository<PasswordResetToken> PasswordResetTokens =>
        _passwordResetTokens ??= new Repository<PasswordResetToken>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}


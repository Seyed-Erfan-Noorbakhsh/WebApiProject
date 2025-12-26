using Microsoft.EntityFrameworkCore;
using Shop_ProjForWeb.Core.Application.Interfaces;
using Shop_ProjForWeb.Core.Domain.Entities;
using Shop_ProjForWeb.Infrastructure.Persistent.DbContext;

namespace Shop_ProjForWeb.Infrastructure.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly SupermarketDbContext _context;

    public AuditRepository(SupermarketDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog auditLog)
    {
        await _context.AuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetEntityAuditTrailAsync(string entityName, Guid entityId)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetUserAuditTrailAsync(string userId)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetAuditTrailAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= endDate.Value);
        }

        return await query
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }
}
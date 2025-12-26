using Shop_ProjForWeb.Core.Domain.Entities;

namespace Shop_ProjForWeb.Core.Application.Interfaces;

public interface IAuditRepository
{
    Task AddAsync(AuditLog auditLog);
    Task<List<AuditLog>> GetEntityAuditTrailAsync(string entityName, Guid entityId);
    Task<List<AuditLog>> GetUserAuditTrailAsync(string userId);
    Task<List<AuditLog>> GetAuditTrailAsync(DateTime? startDate = null, DateTime? endDate = null);
}
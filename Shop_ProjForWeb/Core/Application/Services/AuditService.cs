using System.Text.Json;
using Shop_ProjForWeb.Core.Application.Interfaces;
using Shop_ProjForWeb.Core.Domain.Entities;

namespace Shop_ProjForWeb.Core.Application.Services;

public class AuditService(IAuditRepository auditRepository, IInventoryTransactionRepository inventoryTransactionRepository)
{
    private readonly IAuditRepository _auditRepository = auditRepository;
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository = inventoryTransactionRepository;

    public async Task LogCreateAsync<T>(T entity, string? userId = null) where T : BaseEntity
    {
        var auditLog = new AuditLog
        {
            EntityName = typeof(T).Name,
            EntityId = entity.Id,
            Action = "CREATE",
            NewValues = JsonSerializer.Serialize(entity),
            UserId = userId,
            Timestamp = DateTime.UtcNow
        };

        await _auditRepository.AddAsync(auditLog);
    }

    public async Task LogUpdateAsync<T>(T oldEntity, T newEntity, string? userId = null) where T : BaseEntity
    {
        var auditLog = new AuditLog
        {
            EntityName = typeof(T).Name,
            EntityId = newEntity.Id,
            Action = "UPDATE",
            OldValues = JsonSerializer.Serialize(oldEntity),
            NewValues = JsonSerializer.Serialize(newEntity),
            UserId = userId,
            Timestamp = DateTime.UtcNow
        };

        await _auditRepository.AddAsync(auditLog);
    }

    public async Task LogDeleteAsync<T>(T entity, string? userId = null) where T : BaseEntity
    {
        var auditLog = new AuditLog
        {
            EntityName = typeof(T).Name,
            EntityId = entity.Id,
            Action = "DELETE",
            OldValues = JsonSerializer.Serialize(entity),
            UserId = userId,
            Timestamp = DateTime.UtcNow
        };

        await _auditRepository.AddAsync(auditLog);
    }

    public async Task LogAsync(string entityName, Guid entityId, string action, string? details = null, string? userId = null)
    {
        var auditLog = new AuditLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValues = null,
            NewValues = details,
            UserId = userId ?? "System",
            Timestamp = DateTime.UtcNow
        };

        await _auditRepository.AddAsync(auditLog);
    }

    public async Task LogInventoryChangeAsync(Guid inventoryId, string transactionType, int quantity, int previousQty, int newQty, string reason, Guid? orderId = null, string userId = "System")
    {
        // Create inventory transaction record
        var transaction = transactionType.ToUpper() switch
        {
            "RESERVE" => InventoryTransaction.CreateReservation(inventoryId, quantity, previousQty, newQty, orderId),
            "COMMIT" => InventoryTransaction.CreateCommit(inventoryId, quantity, previousQty, newQty, orderId),
            "RELEASE" => InventoryTransaction.CreateRelease(inventoryId, quantity, previousQty, newQty, orderId),
            _ => InventoryTransaction.CreateAdjustment(inventoryId, quantity, previousQty, newQty, reason, userId)
        };

        await _inventoryTransactionRepository.AddAsync(transaction);

        // Also create general audit log
        await LogAsync("Inventory", inventoryId, transactionType, 
            $"{reason}. Quantity changed from {previousQty} to {newQty} (change: {quantity})", userId);
    }

    public async Task LogOrderCreationAsync(Guid orderId, Guid userId, decimal totalPrice, int itemCount)
    {
        await LogAsync("Order", orderId, "Created", 
            $"Order created for user {userId} with {itemCount} items, total: {totalPrice:C}", userId.ToString());
    }

    public async Task LogOrderStatusChangeAsync(Guid orderId, string fromStatus, string toStatus, string? reason = null, string? userId = null)
    {
        var details = $"Status changed from {fromStatus} to {toStatus}";
        if (!string.IsNullOrEmpty(reason))
        {
            details += $". Reason: {reason}";
        }

        await LogAsync("Order", orderId, "StatusChange", details, userId);
    }

    public async Task LogVipUpgradeAsync(Guid userId, decimal totalSpending, decimal triggeringOrderTotal)
    {
        await LogAsync("User", userId, "VipUpgrade", 
            $"User upgraded to VIP. Total spending: {totalSpending:C}, triggering order: {triggeringOrderTotal:C}");
    }

    public async Task<List<AuditLog>> GetEntityAuditTrailAsync(string entityName, Guid entityId)
    {
        return await _auditRepository.GetEntityAuditTrailAsync(entityName, entityId);
    }

    public async Task<List<AuditLog>> GetUserAuditTrailAsync(string userId)
    {
        return await _auditRepository.GetUserAuditTrailAsync(userId);
    }

    public async Task<List<InventoryTransaction>> GetInventoryAuditTrailAsync(Guid inventoryId)
    {
        return await _inventoryTransactionRepository.GetByInventoryIdAsync(inventoryId);
    }

    public async Task<List<InventoryTransaction>> GetOrderInventoryAuditTrailAsync(Guid orderId)
    {
        return await _inventoryTransactionRepository.GetByOrderIdAsync(orderId);
    }
}
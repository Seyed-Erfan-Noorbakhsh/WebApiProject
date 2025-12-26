namespace Shop_ProjForWeb.Core.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? UserId { get; set; } // Who made the change
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    private static readonly string[] ValidActions = { "CREATE", "UPDATE", "DELETE", "SOFT_DELETE", "RESTORE" };

    public override void ValidateEntity()
    {
        base.ValidateEntity();
        
        ValidateStringProperty(EntityName, nameof(EntityName), minLength: 1, maxLength: 100);
        
        if (EntityId == Guid.Empty)
            throw new ArgumentException("EntityId cannot be empty");
        
        ValidateStringProperty(Action, nameof(Action), minLength: 1, maxLength: 50);
        
        if (!ValidActions.Contains(Action))
            throw new ArgumentException($"Invalid action: {Action}. Valid actions are: {string.Join(", ", ValidActions)}");
        
        if (!string.IsNullOrEmpty(UserId))
        {
            ValidateStringProperty(UserId, nameof(UserId), maxLength: 100);
        }
        
        if (Timestamp == default)
            throw new ArgumentException("Timestamp must be set");
        
        // Validate that UPDATE and DELETE actions have OldValues
        if ((Action == "UPDATE" || Action == "DELETE" || Action == "SOFT_DELETE") && string.IsNullOrEmpty(OldValues))
            throw new InvalidOperationException($"{Action} actions must include OldValues");
        
        // Validate that CREATE and UPDATE actions have NewValues
        if ((Action == "CREATE" || Action == "UPDATE" || Action == "RESTORE") && string.IsNullOrEmpty(NewValues))
            throw new InvalidOperationException($"{Action} actions must include NewValues");
    }

    public static AuditLog CreateLog(string entityName, Guid entityId, string action, string? oldValues = null, string? newValues = null, string? userId = null)
    {
        var log = new AuditLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues,
            NewValues = newValues,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        };
        log.ValidateEntity();
        return log;
    }
}
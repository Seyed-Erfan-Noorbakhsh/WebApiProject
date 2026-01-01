namespace Shop_ProjForWeb.Core.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }


     protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public virtual void ValidateEntity()
    {
        if (Id == Guid.Empty)
            throw new ArgumentException("Entity ID cannot be empty");
        
        if (CreatedAt == default)
            throw new ArgumentException("CreatedAt must be set");
        
        if (UpdatedAt == default)
            throw new ArgumentException("UpdatedAt must be set");
        
        if (IsDeleted && DeletedAt == null)
            throw new InvalidOperationException("DeletedAt must be set when entity is soft deleted");
    }
}
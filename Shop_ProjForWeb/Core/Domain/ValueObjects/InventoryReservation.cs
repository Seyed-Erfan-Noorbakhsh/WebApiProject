namespace Shop_ProjForWeb.Core.Domain.ValueObjects;

public class InventoryReservation
{
    public Guid ReservationId { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public DateTime ReservedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public ReservationStatus Status { get; set; }

    public InventoryReservation(Guid productId, int quantity, TimeSpan? duration = null)
    {
        ReservationId = Guid.NewGuid();
        ProductId = productId;
        Quantity = quantity;
        ReservedAt = DateTime.UtcNow;
        ExpiresAt = ReservedAt.Add(duration ?? TimeSpan.FromMinutes(15)); // Default 15 minutes
        Status = ReservationStatus.Active;
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public bool CanBeCommitted => Status == ReservationStatus.Active && !IsExpired;

    public bool CanBeReleased => Status == ReservationStatus.Active;
}

public enum ReservationStatus
{
    Active,
    Committed,
    Released,
    Expired
}
namespace Shop_ProjForWeb.Core.Domain.Entities;

public class Product : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public decimal BasePrice { get; set; }
    public int DiscountPercent { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
    
    // Navigation Properties
    public Inventory? Inventory { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public override void ValidateEntity()
    {
        base.ValidateEntity();
        
        ValidateStringProperty(Name, nameof(Name), minLength: 1, maxLength: 200);
        
        if (!string.IsNullOrEmpty(Description))
        {
            ValidateStringProperty(Description, nameof(Description), maxLength: 1000);
        }
        
        if (!string.IsNullOrEmpty(Category))
        {
            ValidateStringProperty(Category, nameof(Category), maxLength: 100);
        }
        
        ValidateDecimalProperty(BasePrice, nameof(BasePrice), minValue: 0);
        ValidateIntProperty(DiscountPercent, nameof(DiscountPercent), minValue: 0, maxValue: 100);
        
        if (!string.IsNullOrEmpty(ImageUrl))
        {
            ValidateStringProperty(ImageUrl, nameof(ImageUrl), maxLength: 500);
            if (!IsValidUrl(ImageUrl))
                throw new ArgumentException("ImageUrl format is invalid");
        }
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out _);
    }

    public void ValidatePrice()
    {
        if (BasePrice <= 0)
            throw new ArgumentException("Product price must be greater than zero");
    }

    public void ValidateDiscountPercent()
    {
        if (DiscountPercent < 0 || DiscountPercent > 100)
            throw new ArgumentException("Discount percent must be between 0 and 100");
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new ArgumentException("Product price must be greater than zero");
        
        BasePrice = newPrice;
        UpdatedAt = DateTime.UtcNow;
        ValidateEntity();
    }

    public void UpdateDiscount(int newDiscountPercent)
    {
        if (newDiscountPercent < 0 || newDiscountPercent > 100)
            throw new ArgumentException("Discount percent must be between 0 and 100");
        
        DiscountPercent = newDiscountPercent;
        UpdatedAt = DateTime.UtcNow;
        ValidateEntity();
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        ValidateEntity();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        ValidateEntity();
    }
}

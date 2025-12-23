namespace Shop_ProjForWeb.Infrastructure.Persistent.DbContext;

using Microsoft.EntityFrameworkCore;
using Shop_ProjForWeb.Core.Domain.Entities;

public class SupermarketDbContext : DbContext
{
    public SupermarketDbContext(DbContextOptions<SupermarketDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired();
            entity.Property(e => e.IsVip).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Product Configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.BasePrice).IsRequired();
            entity.Property(e => e.DiscountPercent).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Inventory Configuration
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.LowStockFlag).IsRequired();
            entity.Property(e => e.LastUpdatedAt).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Order Configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.TotalPrice).IsRequired();
            entity.Property(e => e.PaidAt).IsRequired(false);
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // OrderItem Configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderId).IsRequired();
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.UnitPrice).IsRequired();
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.DiscountApplied).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}

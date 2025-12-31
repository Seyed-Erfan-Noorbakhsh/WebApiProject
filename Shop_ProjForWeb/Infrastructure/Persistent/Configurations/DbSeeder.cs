using Shop_ProjForWeb.Core.Domain.Entities;
using Shop_ProjForWeb.Infrastructure.Persistent.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Shop_ProjForWeb.Infrastructure.Persistent
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(SupermarketDbContext db)
        {
            if (!await db.Users.AnyAsync())
            {
                var normalUser = new User
                {
                    FullName = "Normal Customer",
                    IsVip = false,
                };

                var vipUser = new User
                {
                    FullName = "VIP Customer",
                    IsVip = true,
                };

                await db.Users.AddRangeAsync(normalUser, vipUser);
            }

            if (!await db.Products.AnyAsync())
            {
                var product1 = new Product
                {
                    Name = "Apple",
                    BasePrice = 100,
                    DiscountPercent = 5,
                    IsActive = true
                };

                var product2 = new Product
                {
                    Name = "Banana",
                    BasePrice = 50,
                    DiscountPercent = 0,
                    IsActive = true
                };

                var product3 = new Product
                {
                    Name = "Milk",
                    BasePrice = 200,
                    DiscountPercent = 10,
                    IsActive = true
                };

                await db.Products.AddRangeAsync(product1, product2, product3);

                // Seed inventory for each product
                var inventory1 = new Inventory
                {
                    ProductId = product1.Id,
                    Quantity = 50,
                    LowStockFlag = false,
                    LastUpdatedAt = DateTime.UtcNow
                };

                var inventory2 = new Inventory
                {
                    ProductId = product2.Id,
                    Quantity = 100,
                    LowStockFlag = false,
                    LastUpdatedAt = DateTime.UtcNow
                };

                var inventory3 = new Inventory
                {
                    ProductId = product3.Id,
                    Quantity = 30,
                    LowStockFlag = false,
                    LastUpdatedAt = DateTime.UtcNow
                };

                await db.Inventories.AddRangeAsync(inventory1, inventory2, inventory3);
            }

            await db.SaveChangesAsync();
        }
    }
}

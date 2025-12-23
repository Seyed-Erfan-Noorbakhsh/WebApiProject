using Shop_ProjForWeb.Core.Application.Interfaces;
using Shop_ProjForWeb.Core.Application.Services;
using Shop_ProjForWeb.Infrastructure.Persistent.DbContext;
using Shop_ProjForWeb.Infrastructure.Persistent;
using Shop_ProjForWeb.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<SupermarketDbContext>(options =>
    options.UseSqlite("Data Source=supermarket.db"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddScoped<PricingService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<VipUpgradeService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply migrations and seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SupermarketDbContext>();
    db.Database.Migrate();

    await DbSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

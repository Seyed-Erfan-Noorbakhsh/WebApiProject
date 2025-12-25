using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MediatR;
using Serilog;
using AspNetCoreRateLimit;
using FluentValidation;
using FluentValidation.AspNetCore;

using Shop_ProjForWeb.Core.Application.Configuration;
using Shop_ProjForWeb.Core.Application.Interfaces;
using Shop_ProjForWeb.Core.Application.Services;


using Shop_ProjForWeb.Infrastructure.Repositories;

using Shop_ProjForWeb.Presentation.Middleware;
using Shop_ProjForWeb.Infrastructure.Logging;
using Shop_ProjForWeb.Infrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

#region Logging (Serilog)
SerilogConfiguration.ConfigureSerilog(builder.Configuration);
builder.Host.UseSerilog();
#endregion

#region Database (SQLite – Supermarket)
// builder.Services.AddDbContext<SupermarketDbContext>(options =>
//     options.UseSqlite("Data Source=supermarket.db"));
#endregion

#region MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
#endregion

#region Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
#endregion




#region Application Services
builder.Services.AddScoped<PricingService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<VipUpgradeService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ProductImageService>();
builder.Services.AddScoped<AgifyService>();
#endregion

#region Configuration
builder.Services.Configure<FileUploadOptions>(
    builder.Configuration.GetSection("FileUpload"));
#endregion

#region Controllers + Validation
builder.Services.AddControllers()
    .AddFluentValidation();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
#endregion

#region Swagger + JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Shop Web API",
        Version = "v1",
        Description = "Shop API with Orders, Auth, Logging and Security"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
#endregion

#region Authentication & Authorization
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SUPER_SECRET_KEY";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ShopAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ShopAPI";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();
#endregion

#region Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(
    builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
#endregion

#region CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
#endregion

var app = builder.Build();

#region Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSecurityHeaders();
app.UseIpRateLimiting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapControllers();
#endregion

Log.Information("Application started successfully");
app.Run();

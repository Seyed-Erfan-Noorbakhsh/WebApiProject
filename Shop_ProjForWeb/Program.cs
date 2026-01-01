using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shop_ProjForWeb.Application.Interfaces;
using Shop_ProjForWeb.Application.Mappings;
using Shop_ProjForWeb.Application.Services;
using Shop_ProjForWeb.Infrastructure.Data;
using Shop_ProjForWeb.Infrastructure.Extensions;
using Shop_ProjForWeb.Infrastructure.Logging;
using Shop_ProjForWeb.Infrastructure.Middleware;
using Serilog;
using AspNetCoreRateLimit;
using FluentValidation;
using FluentValidation.AspNetCore;
using Shop_ProjForWeb.Application.Validators;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
SerilogConfiguration.ConfigureSerilog(builder.Configuration);
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddFluentValidation();
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Shop Web API",
        Version = "v1",
        Description = "Web API with Authentication, Authorization, and Logging"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
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

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Shop_ProjForWeb.Infrastructure")));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
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

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Request Logging (transient, not scoped)
builder.Services.AddTransient<RequestLoggingMiddleware>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Security Headers (should be early in pipeline)
app.UseSecurityHeaders();

// Rate Limiting
app.UseIpRateLimiting();

// CORS
app.UseCors("AllowAll");

// Request Logging (before exception handling to log all requests)
app.UseRequestLogging();

// Exception Handling (should be before other middleware that might throw)
app.UseExceptionHandling();

// Audit Logging
app.UseAuditLogging();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await Shop_ProjForWeb.Infrastructure.Data.DbInitializer.InitializeAsync(context);
}

Log.Information("Application starting up");

app.Run();

//hhh//
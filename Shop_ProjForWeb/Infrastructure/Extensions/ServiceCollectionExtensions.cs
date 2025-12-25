using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shop_ProjForWeb.Application.Interfaces;
using Shop_ProjForWeb.Domain.Interfaces;
using Shop_ProjForWeb.Infrastructure.Data;
using Shop_ProjForWeb.Infrastructure.Logging;
using Shop_ProjForWeb.Infrastructure.Repositories;

namespace Shop_ProjForWeb.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Shop_ProjForWeb.Infrastructure")));

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Logger Service
        services.AddSingleton<ILoggerService, SerilogLoggerService>();

        return services;
    }
}


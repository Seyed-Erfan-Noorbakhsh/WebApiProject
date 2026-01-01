using System.Text;
using Shop_ProjForWeb.Core.Domain.Entities;

using Shop_ProjForWeb.Domain.Interfaces;

namespace Shop_ProjForWeb.Infrastructure.Middleware;

public class AuditLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLogMiddleware> _logger;

    public AuditLogMiddleware(RequestDelegate next, ILogger<AuditLogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
    {
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var method = context.Request.Method;
        var path = context.Request.Path;

        try
        {
            await _next(context);

            // Log successful requests
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = $"{method} {path}",
                    EntityType = "HTTP Request",
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Timestamp = DateTime.UtcNow
                };

                await unitOfWork.AuditLogs.AddAsync(auditLog);
                await unitOfWork.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AuditLogMiddleware");
            
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = $"{method} {path} - ERROR",
                EntityType = "HTTP Request",
                IpAddress = ipAddress,
                UserAgent = userAgent,
                NewValues = ex.Message,
                Timestamp = DateTime.UtcNow
            };

            await unitOfWork.AuditLogs.AddAsync(auditLog);
            await unitOfWork.SaveChangesAsync();
            
            throw;
        }
    }
}

public static class AuditLogMiddlewareExtensions
{
    public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuditLogMiddleware>();
    }
}


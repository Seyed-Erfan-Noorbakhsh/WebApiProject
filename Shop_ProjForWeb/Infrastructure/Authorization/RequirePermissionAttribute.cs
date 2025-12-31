using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Shop_ProjForWeb.Infrastructure.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string _resource;
    private readonly string _action;

    public RequirePermissionAttribute(string resource, string action)
    {
        _resource = resource;
        _action = action;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var permission = $"{_resource}.{_action}";
        var hasPermission = user.Claims.Any(c => 
            c.Type == "permission" && c.Value == permission);

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}


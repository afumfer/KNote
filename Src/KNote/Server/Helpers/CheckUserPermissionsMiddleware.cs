using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;

namespace KNote.Server.Helpers;

// For use this middleware
// app.UseMiddleware<CheckUserPermissionsMiddleware>(httpContextAccessor, myResourcesContainerRootPath);

public class CheckUserPermissionsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _alternativeContentPath;

    public CheckUserPermissionsMiddleware(RequestDelegate next, IHttpContextAccessor httpContextAccessor, string alternativeContentPath)
    {
        _next = next;
        _httpContextAccessor = httpContextAccessor;
        _alternativeContentPath = alternativeContentPath;
    }

    public async Task Invoke(HttpContext context)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        try
        {
            if (httpContext.User.Identity.IsAuthenticated)
            {
                IIdentity identity = httpContext.User.Identity;
                string currentUsername = identity.Name;

                if (!UserHasPermission(_alternativeContentPath, currentUsername))
                {
                    await HandleExceptionAsync(context, "The user does not have permission to access the content alternate path");
                }
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex.Message);            
        }
    }

    private bool UserHasPermission(string path, string username)
    {
        bool hasAccess = false;

        DirectoryInfo di = new DirectoryInfo(path);
        DirectorySecurity acl = di.GetAccessControl();
        AuthorizationRuleCollection rules = acl.GetAccessRules(true, true, typeof(NTAccount));

        foreach (FileSystemAccessRule rule in rules)
        {
            if (string.Equals(rule.IdentityReference.Value, username, StringComparison.InvariantCultureIgnoreCase) && (rule.FileSystemRights & FileSystemRights.Read) == FileSystemRights.Read)
            {
                hasAccess = true;
                break;
            }
        }

        return hasAccess;
    }

    private async Task HandleExceptionAsync(HttpContext context, string message)
    {       
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        
        string errorMessage = $"Error: {message}.";
        await context.Response.WriteAsync(errorMessage);        
    }

}

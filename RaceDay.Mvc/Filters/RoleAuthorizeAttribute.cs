using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Mvc.Filters;

/// <summary>
/// Protects an MVC page using the session set at login.
/// Not logged in            -> redirected to the login page
/// Logged in, wrong role    -> redirected to the Access Denied page
///
/// [RoleAuthorize]                  any logged-in user
/// [RoleAuthorize("Organiser")]     Organisers only
/// [RoleAuthorize("Participant")]   Participants only
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RoleAuthorizeAttribute : ActionFilterAttribute
{
    private readonly string[] _roles;

    public RoleAuthorizeAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var userId = session.GetInt32("UserId");
        var role = session.GetString("Role");

        if (userId is null || role is null)
        {
            if (context.Controller is Controller controller)
            {
                controller.TempData["Error"] = "Please log in to continue.";
            }

            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        if (_roles.Length > 0 && !_roles.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
            return;
        }

        base.OnActionExecuting(context);
    }
}

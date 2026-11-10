using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Mvc.Controllers;

/// <summary>Base class that gives every controller easy access to the logged-in user.</summary>
public abstract class RaceDayController : Controller
{
    protected int? CurrentUserId => HttpContext.Session.GetInt32("UserId");
    protected string? CurrentRole => HttpContext.Session.GetString("Role");
    protected string DisplayName => HttpContext.Session.GetString("DisplayName") ?? string.Empty;

    /// <summary>
    /// Used when the API says 401 even though the MVC session looks logged in
    /// (for example, the API was restarted). Clears the session and asks the user to log in again.
    /// </summary>
    protected IActionResult SessionExpired()
    {
        HttpContext.Session.Clear();
        TempData["Error"] = "Your session has expired. Please log in again.";
        return RedirectToAction("Login", "Auth");
    }
}

using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

public class AuthController : RaceDayController
{
    private readonly RaceDayApiClient _api;

    public AuthController(RaceDayApiClient api)
    {
        _api = api;
    }

    // ---------------------------------------------------------------- register

    [HttpGet]
    public IActionResult Register()
    {
        if (CurrentUserId.HasValue) return RedirectByRole(CurrentRole);
        return View(new RegisterVm());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var request = new RegisterRequest
        {
            FullName = vm.FullName.Trim(),
            Email = vm.Email.Trim(),
            Password = vm.Password,
            Role = vm.Role,
            PhoneNumber = string.IsNullOrWhiteSpace(vm.PhoneNumber) ? null : vm.PhoneNumber.Trim(),
            DateOfBirth = vm.DateOfBirth.HasValue ? DateOnly.FromDateTime(vm.DateOfBirth.Value) : null
        };

        var result = await _api.SendAsync(HttpMethod.Post, "api/auth/register", request);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Registration failed. Please try again.");
            return View(vm);
        }

        TempData["Success"] = "Registration successful. Please log in.";
        return RedirectToAction(nameof(Login));
    }

    // ------------------------------------------------------------------- login

    [HttpGet]
    public IActionResult Login()
    {
        if (CurrentUserId.HasValue) return RedirectByRole(CurrentRole);
        return View(new LoginVm());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // Start from a clean MVC session, then let the API client store the API cookie in it.
        HttpContext.Session.Clear();

        var result = await _api.LoginAsync(new LoginRequest
        {
            Email = vm.Email.Trim(),
            Password = vm.Password
        });

        if (!result.Success || result.Data is null)
        {
            var message = result.IsUnauthorized
                ? "Invalid email or password."
                : result.Error ?? "Login failed. Please try again.";

            ModelState.AddModelError(string.Empty, message);
            return View(vm);
        }

        var auth = result.Data;
        HttpContext.Session.SetInt32("UserId", auth.UserId);
        HttpContext.Session.SetString("Role", auth.Role);
        HttpContext.Session.SetString("DisplayName", string.IsNullOrWhiteSpace(auth.FullName) ? auth.Email : auth.FullName);

        return RedirectByRole(auth.Role);
    }

    // ------------------------------------------------------------------ logout

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Ask the API to end its session too; the MVC session is cleared either way.
        await _api.SendAsync(HttpMethod.Post, "api/auth/logout");
        HttpContext.Session.Clear();

        TempData["Success"] = "You have been logged out.";
        return RedirectToAction("Index", "Home");
    }

    // ----------------------------------------------------------------- helpers

    private IActionResult RedirectByRole(string? role) =>
        string.Equals(role, "Organiser", StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction("Index", "Organiser")
            : RedirectToAction("Index", "Participant");
}

using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

/// <summary>Profile page shared by both roles: view and update your own details.</summary>
[RoleAuthorize]
public class ProfileController : RaceDayController
{
    private readonly RaceDayApiClient _api;

    public ProfileController(RaceDayApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _api.GetAsync<ProfileDto>("api/users/me");
        if (result.IsUnauthorized) return SessionExpired();

        if (!result.Success || result.Data is null)
        {
            TempData["Error"] = result.Error ?? "Your profile could not be loaded.";
            return RedirectToAction("Index", "Home");
        }

        return View(ToVm(result.Data));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileVm vm)
    {
        if (vm.DateOfBirth.HasValue &&
            (vm.DateOfBirth.Value.Date > DateTime.Today || vm.DateOfBirth.Value.Year < 1900))
        {
            ModelState.AddModelError(nameof(vm.DateOfBirth), "Please enter a valid date of birth.");
        }

        if (!ModelState.IsValid)
        {
            await FillReadOnlyFieldsAsync(vm);
            return View(vm);
        }

        var request = new UpdateProfileRequest
        {
            FullName = vm.FullName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(vm.PhoneNumber) ? null : vm.PhoneNumber.Trim(),
            DateOfBirth = vm.DateOfBirth.HasValue ? DateOnly.FromDateTime(vm.DateOfBirth.Value) : null
        };

        var result = await _api.SendAsync<ProfileDto>(HttpMethod.Put, "api/users/me", request);
        if (result.IsUnauthorized) return SessionExpired();

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Your profile could not be updated.");
            await FillReadOnlyFieldsAsync(vm);
            return View(vm);
        }

        // Keep the name in the menu bar up to date.
        HttpContext.Session.SetString("DisplayName", request.FullName);

        TempData["Success"] = "Your profile was updated.";
        return RedirectToAction(nameof(Index));
    }

    private async Task FillReadOnlyFieldsAsync(ProfileVm vm)
    {
        var current = await _api.GetAsync<ProfileDto>("api/users/me");
        if (current.Data is null) return;

        vm.Email = current.Data.Email;
        vm.Role = current.Data.Role;
        vm.RegisteredDate = current.Data.RegisteredDate;
    }

    private static ProfileVm ToVm(ProfileDto dto) => new()
    {
        FullName = dto.FullName,
        PhoneNumber = dto.PhoneNumber,
        DateOfBirth = dto.DateOfBirth?.ToDateTime(TimeOnly.MinValue),
        Email = dto.Email,
        Role = dto.Role,
        RegisteredDate = dto.RegisteredDate
    };
}

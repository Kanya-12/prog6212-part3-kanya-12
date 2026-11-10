using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

[RoleAuthorize("Organiser")]
public class OrganiserController : RaceDayController
{
    private static readonly string[] EnrolmentStatuses = { "Pending", "Confirmed", "Cancelled" };
    private static readonly string[] ResultStatuses = { "Finished", "DNF", "DNS" };

    private readonly RaceDayApiClient _api;

    public OrganiserController(RaceDayApiClient api)
    {
        _api = api;
    }

    // =================================================================== dashboard

    public async Task<IActionResult> Index()
    {
        var mine = await _api.GetAsync<List<EventDto>>("api/events/mine");
        if (mine.IsUnauthorized) return SessionExpired();

        var vm = new OrganiserDashboardVm();

        if (!mine.Success || mine.Data is null)
        {
            TempData["Error"] = mine.Error ?? "Your events could not be loaded.";
            return View(vm);
        }

        var today = DateOnly.FromDateTime(DateTime.Now);

        foreach (var ev in mine.Data.OrderBy(e => e.EventDate))
        {
            var enrolments = await _api.GetAsync<List<EnrolmentDto>>($"api/events/{ev.EventId}/enrolments");
            if (enrolments.IsUnauthorized) return SessionExpired();

            var list = enrolments.Data ?? new List<EnrolmentDto>();

            vm.Events.Add(new EventSummaryVm
            {
                Event = ev,
                TotalEnrolments = list.Count,
                ConfirmedEnrolments = list.Count(e => e.Status == "Confirmed"),
                IsUpcoming = ev.EventDate >= today
            });
        }

        vm.TotalEvents = vm.Events.Count;
        vm.UpcomingEvents = vm.Events.Count(e => e.IsUpcoming);
        vm.TotalEnrolments = vm.Events.Sum(e => e.TotalEnrolments);
        vm.NextEventDate = vm.Events
            .Where(e => e.IsUpcoming)
            .Select(e => (DateOnly?)e.Event.EventDate)
            .FirstOrDefault();

        return View(vm);
    }

    // ================================================================ events: list

    public async Task<IActionResult> Events()
    {
        var result = await _api.GetAsync<List<EventDto>>("api/events/mine");
        if (result.IsUnauthorized) return SessionExpired();

        if (!result.Success || result.Data is null)
        {
            TempData["Error"] = result.Error ?? "Your events could not be loaded.";
            return View(new List<EventDto>());
        }

        return View(result.Data.OrderBy(e => e.EventDate).ToList());
    }

    // ============================================================== events: create

    [HttpGet]
    public IActionResult CreateEvent()
    {
        return View(new EventFormVm { EventDate = DateTime.Today.AddDays(30) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(EventFormVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _api.SendAsync<EventDto>(HttpMethod.Post, "api/events",
            BuildEventRequest<CreateEventRequest>(vm));
        if (result.IsUnauthorized) return SessionExpired();

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "The event could not be created.");
            return View(vm);
        }

        TempData["Success"] = $"\"{vm.EventName}\" was created. Next, add its categories.";

        return result.Data is null
            ? RedirectToAction(nameof(Events))
            : RedirectToAction(nameof(Categories), new { id = result.Data.EventId });
    }

    // =============================================================== events: edit

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        var (ev, failure) = await LoadOwnEventAsync(id);
        if (failure != null) return failure;

        return View(new EventFormVm
        {
            EventId = ev!.EventId,
            EventName = ev.EventName,
            Description = ev.Description,
            EventDate = ev.EventDate.ToDateTime(TimeOnly.MinValue),
            Location = ev.Location,
            DistanceKm = ev.DistanceKm,
            EventType = ev.EventType
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(int id, EventFormVm vm)
    {
        vm.EventId = id;
        if (!ModelState.IsValid) return View(vm);

        var result = await _api.SendAsync<EventDto>(HttpMethod.Put, $"api/events/{id}",
            BuildEventRequest<UpdateEventRequest>(vm));
        if (result.IsUnauthorized) return SessionExpired();

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "The event could not be updated.");
            return View(vm);
        }

        TempData["Success"] = $"\"{vm.EventName}\" was updated.";
        return RedirectToAction(nameof(Events));
    }

    // ============================================================== events: delete

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var (ev, failure) = await LoadOwnEventAsync(id);
        if (failure != null) return failure;

        var result = await _api.SendAsync(HttpMethod.Delete, $"api/events/{id}");
        if (result.IsUnauthorized) return SessionExpired();

        if (result.Success)
        {
            TempData["Success"] = $"\"{ev!.EventName}\" was deleted.";
        }
        else
        {
            TempData["Error"] = result.Error ?? "The event could not be deleted.";
        }

        return RedirectToAction(nameof(Events));
    }

    // ================================================================== categories

    [HttpGet]
    public async Task<IActionResult> Categories(int id)
    {
        var (ev, failure) = await LoadOwnEventAsync(id);
        if (failure != null) return failure;

        return View(await BuildCategoriesPageAsync(ev!, new CategoryFormVm { EventId = id }));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(CategoryFormVm form)
    {
        var (ev, failure) = await LoadOwnEventAsync(form.EventId);
        if (failure != null) return failure;

        if (!ModelState.IsValid)
        {
            return View("Categories", await BuildCategoriesPageAsync(ev!, form));
        }

        var result = await _api.SendAsync<CategoryDto>(HttpMethod.Post,
            $"api/events/{form.EventId}/categories", BuildCategoryRequest<CreateCategoryRequest>(form));
        if (result.IsUnauthorized) return SessionExpired();

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "The category could not be added.");
            return View("Categories", await BuildCategoriesPageAsync(ev!, form));
        }

        TempData["Success"] = $"The category \"{form.CategoryName}\" was added.";
        return RedirectToAction(nameof(Categories), new { id = form.EventId });
    }

    [HttpGet]
    public async Task<IActionResult> EditCategory(int id, int eventId)
    {
        var (ev, failure) = await LoadOwnEventAsync(eventId);
        if (failure != null) return failure;

        var categories = await _api.GetAsync<List<CategoryDto>>($"api/events/{eventId}/categories");
        if (categories.IsUnauthorized) return SessionExpired();

        var category = categories.Data?.FirstOrDefault(c => c.CategoryId == id);
        if (category is null)
        {
            TempData["Error"] = "That category could not be found.";
            return RedirectToAction(nameof(Categories), new { id = eventId });
        }

        ViewBag.EventName = ev!.EventName;

        return View(new CategoryFormVm
        {
            CategoryId = category.CategoryId,
            EventId = category.EventId,
            CategoryName = category.CategoryName,
            DistanceKm = category.DistanceKm,
            MinAge = category.MinAge,
            MaxAge = category.MaxAge,
            EntryFee = category.EntryFee
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(CategoryFormVm form)
    {
        var (ev, failure) = await LoadOwnEventAsync(form.EventId);
        if (failure != null) return failure;

        ViewBag.EventName = ev!.EventName;
        if (!ModelState.IsValid) return View(form);

        var result = await _api.SendAsync<CategoryDto>(HttpMethod.Put,
            $"api/categories/{form.CategoryId}", BuildCategoryRequest<UpdateCategoryRequest>(form));
        if (result.IsUnauthorized) return SessionExpired();

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "The category could not be updated.");
            return View(form);
        }

        TempData["Success"] = $"The category \"{form.CategoryName}\" was updated.";
        return RedirectToAction(nameof(Categories), new { id = form.EventId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id, int eventId)
    {
        var (_, failure) = await LoadOwnEventAsync(eventId);
        if (failure != null) return failure;

        var result = await _api.SendAsync(HttpMethod.Delete, $"api/categories/{id}");
        if (result.IsUnauthorized) return SessionExpired();

        if (result.Success)
        {
            TempData["Success"] = "The category was deleted.";
        }
        else
        {
            TempData["Error"] = result.Error ?? "The category could not be deleted.";
        }

        return RedirectToAction(nameof(Categories), new { id = eventId });
    }

    // ================================================================== enrolments

    [HttpGet]
    public async Task<IActionResult> Enrolments(int id)
    {
        var (ev, failure) = await LoadOwnEventAsync(id);
        if (failure != null) return failure;

        var result = await _api.GetAsync<List<EnrolmentDto>>($"api/events/{id}/enrolments");
        if (result.IsUnauthorized) return SessionExpired();

        if (!result.Success)
        {
            TempData["Error"] = result.Error ?? "The enrolments could not be loaded.";
        }

        return View(new EnrolmentsPageVm
        {
            Event = ev!,
            Enrolments = (result.Data ?? new List<EnrolmentDto>()).OrderBy(e => e.EnrolmentDate).ToList()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateEnrolmentStatus(int enrolmentId, int eventId, string status)
    {
        var (_, failure) = await LoadOwnEventAsync(eventId);
        if (failure != null) return failure;

        if (!EnrolmentStatuses.Contains(status))
        {
            TempData["Error"] = "Please choose Pending, Confirmed or Cancelled.";
            return RedirectToAction(nameof(Enrolments), new { id = eventId });
        }

        var result = await _api.SendAsync(HttpMethod.Put, $"api/enrolments/{enrolmentId}/status",
            new UpdateEnrolmentStatusRequest { Status = status });
        if (result.IsUnauthorized) return SessionExpired();

        if (result.Success)
        {
            TempData["Success"] = $"The enrolment is now {status}.";
        }
        else
        {
            TempData["Error"] = result.Error ?? "The status could not be updated.";
        }

        return RedirectToAction(nameof(Enrolments), new { id = eventId });
    }

    // ===================================================================== results

    [HttpGet]
    public async Task<IActionResult> Results(int id)
    {
        var (ev, failure) = await LoadOwnEventAsync(id);
        if (failure != null) return failure;

        var enrolments = await _api.GetAsync<List<EnrolmentDto>>($"api/events/{id}/enrolments");
        if (enrolments.IsUnauthorized) return SessionExpired();

        var results = await _api.GetAsync<List<ResultDto>>($"api/events/{id}/results");
        if (results.IsUnauthorized) return SessionExpired();

        if (!enrolments.Success)
        {
            TempData["Error"] = enrolments.Error ?? "The enrolments could not be loaded.";
        }

        var existing = results.Data ?? new List<ResultDto>();

        var rows = (enrolments.Data ?? new List<EnrolmentDto>())
            .Where(e => e.Status != "Cancelled")
            .OrderBy(e => e.ParticipantName)
            .Select(e => new ResultRowVm
            {
                Enrolment = e,
                Result = existing.FirstOrDefault(r => r.EnrolmentId == e.EnrolmentId)
            })
            .ToList();

        return View(new ResultsPageVm { Event = ev!, Rows = rows });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResult(ResultFormVm vm)
    {
        var (_, failure) = await LoadOwnEventAsync(vm.EventId);
        if (failure != null) return failure;

        IActionResult Back() => RedirectToAction("Results", new { id = vm.EventId });

        if (!ResultStatuses.Contains(vm.Status))
        {
            TempData["Error"] = "Please choose Finished, DNF or DNS.";
            return Back();
        }

        TimeSpan? finishTime = null;
        if (!string.IsNullOrWhiteSpace(vm.FinishTime))
        {
            if (!TryParseFinishTime(vm.FinishTime, out var parsed))
            {
                TempData["Error"] = "Enter the finish time as hh:mm:ss, for example 01:45:30.";
                return Back();
            }

            finishTime = parsed;
        }

        if (vm.Status == "Finished")
        {
            if (finishTime is null)
            {
                TempData["Error"] = "A finish time is required for a finisher.";
                return Back();
            }

            if (vm.FinishPosition is null or < 1)
            {
                TempData["Error"] = "Enter the finishing position (1 or higher).";
                return Back();
            }
        }

        if (vm.CategoryPosition is < 1)
        {
            TempData["Error"] = "The category position must be 1 or higher.";
            return Back();
        }

        // DNF and DNS entries are saved without a time or positions.
        var finished = vm.Status == "Finished";

        var request = new CreateResultRequest
        {
            EnrolmentId = vm.EnrolmentId,
            FinishTime = finished ? finishTime : null,
            FinishPosition = finished ? vm.FinishPosition : null,
            CategoryPosition = finished ? vm.CategoryPosition : null,
            Status = vm.Status
        };

        var result = vm.ResultId.HasValue
            ? await _api.SendAsync(HttpMethod.Put, $"api/results/{vm.ResultId.Value}", request)
            : await _api.SendAsync(HttpMethod.Post, $"api/enrolments/{vm.EnrolmentId}/result", request);

        if (result.IsUnauthorized) return SessionExpired();

        if (result.Success)
        {
            TempData["Success"] = "The result was saved and is now published to the participant.";
        }
        else
        {
            TempData["Error"] = result.Error ?? "The result could not be saved.";
        }

        return Back();
    }

    // ==================================================================== helpers

    /// <summary>
    /// Loads an event and confirms it belongs to the logged-in Organiser. Anything else
    /// sends the user back to their events list with a clear message.
    /// </summary>
    private async Task<(EventDto? Event, IActionResult? Failure)> LoadOwnEventAsync(int id)
    {
        var result = await _api.GetAsync<EventDto>($"api/events/{id}");
        if (result.IsUnauthorized) return (null, SessionExpired());

        if (!result.Success || result.Data is null)
        {
            TempData["Error"] = result.StatusCode == System.Net.HttpStatusCode.NotFound
                ? "That event could not be found."
                : result.Error ?? "The event could not be loaded.";
            return (null, RedirectToAction(nameof(Events)));
        }

        if (result.Data.OrganiserId != CurrentUserId)
        {
            TempData["Error"] = "You can only manage your own events.";
            return (null, RedirectToAction(nameof(Events)));
        }

        return (result.Data, null);
    }

    private async Task<CategoriesPageVm> BuildCategoriesPageAsync(EventDto ev, CategoryFormVm form)
    {
        var categories = await _api.GetAsync<List<CategoryDto>>($"api/events/{ev.EventId}/categories");

        return new CategoriesPageVm
        {
            Event = ev,
            Categories = categories.Data ?? new List<CategoryDto>(),
            Form = form
        };
    }

    private static T BuildEventRequest<T>(EventFormVm vm) where T : CreateEventRequest, new() => new()
    {
        EventName = vm.EventName.Trim(),
        Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim(),
        EventDate = DateOnly.FromDateTime(vm.EventDate!.Value),
        Location = vm.Location.Trim(),
        DistanceKm = vm.DistanceKm!.Value,
        EventType = vm.EventType
    };

    private static T BuildCategoryRequest<T>(CategoryFormVm vm) where T : CreateCategoryRequest, new() => new()
    {
        CategoryName = vm.CategoryName.Trim(),
        DistanceKm = vm.DistanceKm!.Value,
        MinAge = vm.MinAge!.Value,
        MaxAge = vm.MaxAge,
        EntryFee = vm.EntryFee!.Value
    };

    /// <summary>Accepts hh:mm:ss only, with hours 0 to 23.</summary>
    private static bool TryParseFinishTime(string text, out TimeSpan time)
    {
        time = default;
        var parts = text.Trim().Split(':');

        if (parts.Length != 3 ||
            !int.TryParse(parts[0], out var hours) ||
            !int.TryParse(parts[1], out var minutes) ||
            !int.TryParse(parts[2], out var seconds))
        {
            return false;
        }

        if (hours is < 0 or > 23 || minutes is < 0 or > 59 || seconds is < 0 or > 59)
        {
            return false;
        }

        time = new TimeSpan(hours, minutes, seconds);
        return true;
    }
}

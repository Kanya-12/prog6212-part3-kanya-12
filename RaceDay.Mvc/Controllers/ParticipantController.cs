using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

[RoleAuthorize("Participant")]
public class ParticipantController : RaceDayController
{
    private readonly RaceDayApiClient _api;

    public ParticipantController(RaceDayApiClient api)
    {
        _api = api;
    }

    // ============================================================ events + filters

    public async Task<IActionResult> Index([FromQuery] EventFilterVm filter)
    {
        var result = await _api.GetAsync<List<EventDto>>("api/events");
        if (result.IsUnauthorized) return SessionExpired();

        var vm = new ParticipantEventsVm { Filter = filter };

        if (!result.Success || result.Data is null)
        {
            TempData["Error"] = result.Error ?? "The events could not be loaded.";
            return View(vm);
        }

        var today = DateOnly.FromDateTime(DateTime.Now);
        var upcoming = result.Data.Where(e => e.EventDate >= today).ToList();
        vm.TotalUpcoming = upcoming.Count;

        IEnumerable<EventDto> query = upcoming;

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(e =>
                e.EventName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                e.Location.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (e.Description ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filter.EventType))
        {
            query = query.Where(e => e.EventType == filter.EventType);
        }

        if (filter.MinDistance.HasValue)
        {
            query = query.Where(e => e.DistanceKm >= filter.MinDistance.Value);
        }

        if (filter.MaxDistance.HasValue)
        {
            query = query.Where(e => e.DistanceKm <= filter.MaxDistance.Value);
        }

        if (filter.DateFrom.HasValue)
        {
            var from = DateOnly.FromDateTime(filter.DateFrom.Value);
            query = query.Where(e => e.EventDate >= from);
        }

        if (filter.DateTo.HasValue)
        {
            var to = DateOnly.FromDateTime(filter.DateTo.Value);
            query = query.Where(e => e.EventDate <= to);
        }

        vm.Events = query.OrderBy(e => e.EventDate).ThenBy(e => e.EventName).ToList();
        return View(vm);
    }

    // ================================================================ event details

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var eventResult = await _api.GetAsync<EventDto>($"api/events/{id}");
        if (eventResult.IsUnauthorized) return SessionExpired();

        if (!eventResult.Success || eventResult.Data is null)
        {
            TempData["Error"] = "That event could not be found.";
            return RedirectToAction(nameof(Index));
        }

        var categories = await _api.GetAsync<List<CategoryDto>>($"api/events/{id}/categories");
        if (categories.IsUnauthorized) return SessionExpired();

        var routes = await _api.GetAsync<List<RouteDto>>($"api/events/{id}/routes");
        if (routes.IsUnauthorized) return SessionExpired();

        var mine = await _api.GetAsync<List<EnrolmentDto>>("api/users/me/enrolments");
        if (mine.IsUnauthorized) return SessionExpired();

        var today = DateOnly.FromDateTime(DateTime.Now);

        return View(new EventDetailsVm
        {
            Event = eventResult.Data,
            Categories = (categories.Data ?? new List<CategoryDto>()).OrderBy(c => c.DistanceKm).ToList(),
            Routes = routes.Data ?? new List<RouteDto>(),
            MyEnrolment = mine.Data?.FirstOrDefault(e => e.EventId == id),
            IsPast = eventResult.Data.EventDate < today
        });
    }

    // ===================================================================== enrol

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int id, int categoryId)
    {
        if (categoryId <= 0)
        {
            TempData["Error"] = "Please choose a category before entering.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var eventResult = await _api.GetAsync<EventDto>($"api/events/{id}");
        if (eventResult.IsUnauthorized) return SessionExpired();

        if (eventResult.Success && eventResult.Data is not null &&
            eventResult.Data.EventDate < DateOnly.FromDateTime(DateTime.Now))
        {
            TempData["Error"] = "That event has already taken place.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var result = await _api.SendAsync(HttpMethod.Post, $"api/events/{id}/enrolments",
            new CreateEnrolmentRequest { CategoryId = categoryId });
        if (result.IsUnauthorized) return SessionExpired();

        if (!result.Success)
        {
            TempData["Error"] = result.Error ?? "You could not be entered. Please try again.";
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Success"] = "You are entered! Your enrolment is shown below.";
        return RedirectToAction(nameof(MyEvents));
    }

    // ================================================================= my events

    [HttpGet]
    public async Task<IActionResult> MyEvents()
    {
        var enrolments = await _api.GetAsync<List<EnrolmentDto>>("api/users/me/enrolments");
        if (enrolments.IsUnauthorized) return SessionExpired();

        var events = await _api.GetAsync<List<EventDto>>("api/events");
        if (events.IsUnauthorized) return SessionExpired();

        if (!enrolments.Success)
        {
            TempData["Error"] = enrolments.Error ?? "Your enrolments could not be loaded.";
        }

        var eventMap = (events.Data ?? new List<EventDto>()).ToDictionary(e => e.EventId);
        var today = DateOnly.FromDateTime(DateTime.Now);

        var rows = (enrolments.Data ?? new List<EnrolmentDto>())
            .Select(en =>
            {
                eventMap.TryGetValue(en.EventId, out var ev);

                return new MyEventRowVm
                {
                    Enrolment = en,
                    EventDate = ev?.EventDate,
                    Location = ev?.Location ?? string.Empty,
                    IsUpcoming = ev is not null && ev.EventDate >= today
                };
            })
            .OrderByDescending(r => r.IsUpcoming)
            .ThenBy(r => r.EventDate)
            .ToList();

        return View(rows);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        var result = await _api.SendAsync(HttpMethod.Delete, $"api/enrolments/{id}");
        if (result.IsUnauthorized) return SessionExpired();

        if (result.Success)
        {
            TempData["Success"] = "You have withdrawn from the event.";
        }
        else
        {
            TempData["Error"] = result.Error ?? "You could not be withdrawn. Please try again.";
        }

        return RedirectToAction(nameof(MyEvents));
    }

    // ================================================================ my results

    [HttpGet]
    public async Task<IActionResult> MyResults()
    {
        var results = await _api.GetAsync<List<ResultDto>>("api/users/me/results");
        if (results.IsUnauthorized) return SessionExpired();

        var enrolments = await _api.GetAsync<List<EnrolmentDto>>("api/users/me/enrolments");
        if (enrolments.IsUnauthorized) return SessionExpired();

        var events = await _api.GetAsync<List<EventDto>>("api/events");
        if (events.IsUnauthorized) return SessionExpired();

        if (!results.Success)
        {
            TempData["Error"] = results.Error ?? "Your results could not be loaded.";
        }

        var myResults = results.Data ?? new List<ResultDto>();
        var enrolmentMap = (enrolments.Data ?? new List<EnrolmentDto>()).ToDictionary(e => e.EnrolmentId);
        var eventMap = (events.Data ?? new List<EventDto>()).ToDictionary(e => e.EventId);

        // The public results for each event tell us how many people finished.
        var finishersByEvent = new Dictionary<int, int>();
        foreach (var eventId in myResults.Select(r => r.EventId).Distinct())
        {
            var all = await _api.GetAsync<List<ResultDto>>($"api/events/{eventId}/results");
            finishersByEvent[eventId] = all.Data?.Count(r => r.Status == "Finished") ?? 0;
        }

        var rows = myResults
            .Select(r =>
            {
                enrolmentMap.TryGetValue(r.EnrolmentId, out var enrolment);
                eventMap.TryGetValue(r.EventId, out var ev);

                return new MyResultRowVm
                {
                    Result = r,
                    EventDate = ev?.EventDate,
                    CategoryName = enrolment?.CategoryName ?? "-",
                    TotalFinishers = finishersByEvent.GetValueOrDefault(r.EventId)
                };
            })
            .OrderByDescending(r => r.EventDate)
            .ToList();

        return View(rows);
    }
}

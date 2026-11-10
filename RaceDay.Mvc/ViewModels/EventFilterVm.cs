namespace RaceDay.Mvc.ViewModels;

/// <summary>The filters on the Participant events list, read from the query string.</summary>
public class EventFilterVm
{
    public string? Search { get; set; }
    public string? EventType { get; set; }
    public decimal? MinDistance { get; set; }
    public decimal? MaxDistance { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    public bool IsFiltered =>
        !string.IsNullOrWhiteSpace(Search) ||
        !string.IsNullOrWhiteSpace(EventType) ||
        MinDistance.HasValue || MaxDistance.HasValue ||
        DateFrom.HasValue || DateTo.HasValue;
}

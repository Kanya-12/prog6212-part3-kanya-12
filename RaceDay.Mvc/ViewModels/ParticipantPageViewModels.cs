using RaceDay.Contracts;
using RaceDay.Mvc.Helpers;

namespace RaceDay.Mvc.ViewModels;

public class ParticipantEventsVm
{
    public EventFilterVm Filter { get; set; } = new();
    public List<EventDto> Events { get; set; } = new();
    public int TotalUpcoming { get; set; }
}

public class EventDetailsVm
{
    public EventDto Event { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public List<RouteDto> Routes { get; set; } = new();
    public EnrolmentDto? MyEnrolment { get; set; }
    public bool IsPast { get; set; }
}

public class MyEventRowVm
{
    public EnrolmentDto Enrolment { get; set; } = new();
    public DateOnly? EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool IsUpcoming { get; set; }
}

public class MyResultRowVm
{
    public ResultDto Result { get; set; } = new();
    public DateOnly? EventDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int TotalFinishers { get; set; }

    /// <summary>For example "47th of 312 finishers".</summary>
    public string PositionText
    {
        get
        {
            if (Result.Status != "Finished" || !Result.FinishPosition.HasValue)
            {
                return "-";
            }

            var position = Result.FinishPosition.Value;

            // If only some results are published so far, a total smaller than the
            // position would read strangely, so show just the place.
            return TotalFinishers >= position
                ? $"{UiFormat.Ordinal(position)} of {TotalFinishers} finishers"
                : $"{UiFormat.Ordinal(position)} place";
        }
    }
}

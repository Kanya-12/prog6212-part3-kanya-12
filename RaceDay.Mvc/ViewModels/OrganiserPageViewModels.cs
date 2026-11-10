using RaceDay.Contracts;

namespace RaceDay.Mvc.ViewModels;

public class EventSummaryVm
{
    public EventDto Event { get; set; } = new();
    public int TotalEnrolments { get; set; }
    public int ConfirmedEnrolments { get; set; }
    public bool IsUpcoming { get; set; }
}

public class OrganiserDashboardVm
{
    public List<EventSummaryVm> Events { get; set; } = new();
    public int TotalEvents { get; set; }
    public int UpcomingEvents { get; set; }
    public int TotalEnrolments { get; set; }
    public DateOnly? NextEventDate { get; set; }
}

public class CategoriesPageVm
{
    public EventDto Event { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public CategoryFormVm Form { get; set; } = new();
}

public class EnrolmentsPageVm
{
    public EventDto Event { get; set; } = new();
    public List<EnrolmentDto> Enrolments { get; set; } = new();
}

public class ResultRowVm
{
    public EnrolmentDto Enrolment { get; set; } = new();
    public ResultDto? Result { get; set; }
}

public class ResultsPageVm
{
    public EventDto Event { get; set; } = new();
    public List<ResultRowVm> Rows { get; set; } = new();
}

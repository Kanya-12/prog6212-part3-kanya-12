namespace RaceDay.Mvc.ViewModels;

/// <summary>One participant's result, posted from a row on the Results page.</summary>
public class ResultFormVm
{
    public int EventId { get; set; }
    public int EnrolmentId { get; set; }

    /// <summary>Has a value when a result already exists and is being corrected.</summary>
    public int? ResultId { get; set; }

    /// <summary>Typed as hh:mm:ss.</summary>
    public string? FinishTime { get; set; }

    public int? FinishPosition { get; set; }
    public int? CategoryPosition { get; set; }
    public string Status { get; set; } = "Finished";
}

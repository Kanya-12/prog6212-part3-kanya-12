using System.Globalization;

namespace RaceDay.Mvc.Helpers;

/// <summary>Small display helpers so every page shows dates, times, money and statuses the same way.</summary>
public static class UiFormat
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static string Date(DateOnly date) => date.ToString("d MMM yyyy", Inv);

    public static string Date(DateTime date) => date.ToString("d MMM yyyy", Inv);

    /// <summary>Shows a finish time as hh:mm:ss, or a dash when there is none.</summary>
    public static string Time(TimeSpan? time) =>
        time.HasValue ? time.Value.ToString(@"hh\:mm\:ss", Inv) : "-";

    /// <summary>Same as Time, but an empty string for form fields.</summary>
    public static string TimeInput(TimeSpan? time) =>
        time.HasValue ? time.Value.ToString(@"hh\:mm\:ss", Inv) : string.Empty;

    public static string Money(decimal amount) => "R " + amount.ToString("N2", Inv);

    public static string Distance(decimal km) => km.ToString("0.##", Inv) + " km";

    public static string AgeRange(int minAge, int? maxAge) =>
        maxAge.HasValue ? $"{minAge} to {maxAge} years" : $"{minAge}+ years";

    /// <summary>47 becomes 47th, 2 becomes 2nd, 11 becomes 11th.</summary>
    public static string Ordinal(int number)
    {
        var abs = Math.Abs(number);
        string suffix;

        if (abs % 100 is >= 11 and <= 13)
        {
            suffix = "th";
        }
        else
        {
            suffix = (abs % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            };
        }

        return number + suffix;
    }

    /// <summary>Maps a status such as Confirmed to its colour class in site.css.</summary>
    public static string StatusClass(string? status) =>
        "status-" + (status ?? string.Empty).Trim().ToLowerInvariant();
}

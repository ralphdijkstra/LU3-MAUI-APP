using ClockApp.Application.UseCases.Hours;
using ClockApp.Domain.Entities;

namespace ClockApp.Maui.ViewModels;

public class DayHourItemViewModel
{
    public DayHourItemViewModel(HourEntry entry)
    {
        HourId = entry.HourId;
        Date = entry.Date;
        FromSeconds = entry.FromSeconds;
        Seconds = entry.Seconds;
        BreakSeconds = entry.BreakSeconds;
        UserName = entry.UserName;
        UserId = entry.UserId;
        StartTime = HourFormatting.FormatTimeOfDay(entry.FromSeconds);
        EndTime = HourFormatting.FormatEndTimeOfDay(entry.FromSeconds, entry.Seconds);
        Total = HourFormatting.FormatDuration(entry.Seconds);
    }

    public int HourId { get; }

    public DateOnly Date { get; }

    public int FromSeconds { get; }

    public int Seconds { get; }

    public int BreakSeconds { get; }

    public string UserName { get; }

    public int? UserId { get; }

    public string StartTime { get; }

    public string EndTime { get; }

    public string Total { get; }

    public bool HasUserName => !string.IsNullOrWhiteSpace(UserName);
}

using ClockApp.Domain.Entities;

namespace ClockApp.Maui.ViewModels;

public class ClockedInUserItemViewModel
{
    public ClockedInUserItemViewModel(ClockedInUser user)
    {
        DisplayName = user.DisplayName;
        CheckedInSince = $"since {user.CheckedInAt.ToLocalTime():HH:mm}";
        StatusLabel = user.IsOnBreak ? "Break" : "Working";
        IsOnBreak = user.IsOnBreak;
    }

    public string DisplayName { get; }

    public string CheckedInSince { get; }

    public string StatusLabel { get; }

    public bool IsOnBreak { get; }
}

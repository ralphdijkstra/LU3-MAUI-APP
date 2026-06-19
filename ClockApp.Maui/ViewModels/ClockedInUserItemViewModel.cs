using ClockApp.Domain.Entities;

namespace ClockApp.Maui.ViewModels;

public class ClockedInUserItemViewModel
{
    public ClockedInUserItemViewModel(ClockedInUser user)
    {
        DisplayName = user.DisplayName;
        CheckedInSince = $"sinds {user.CheckedInAt.ToLocalTime():HH:mm}";
        StatusLabel = user.IsOnBreak ? "Pauze" : "Aan het werk";
        IsOnBreak = user.IsOnBreak;
    }

    public string DisplayName { get; }

    public string CheckedInSince { get; }

    public string StatusLabel { get; }

    public bool IsOnBreak { get; }
}

using CommunityToolkit.Mvvm.ComponentModel;

namespace ClockApp.Maui.ViewModels;

public partial class WorkLocationItemViewModel : ObservableObject
{
    public WorkLocationItemViewModel(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public string Id { get; }

    public string Name { get; }

    [ObservableProperty]
    private bool _isSelected;

    public override string ToString() => Name;
}

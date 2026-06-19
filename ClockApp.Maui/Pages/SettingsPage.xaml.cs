using ClockApp.Maui.ViewModels;

namespace ClockApp.Maui.Pages;

public partial class SettingsPage : MvvmContentPage<SettingsViewModel>
{
    public SettingsPage(SettingsViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}

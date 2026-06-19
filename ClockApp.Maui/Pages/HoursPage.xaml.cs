using ClockApp.Maui.ViewModels;

namespace ClockApp.Maui.Pages;

public partial class HoursPage : MvvmContentPage<HoursViewModel>
{
    public HoursPage(HoursViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}

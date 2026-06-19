using ClockApp.Maui.ViewModels;

namespace ClockApp.Maui.Pages;

public partial class LoginPage : MvvmContentPage<LoginViewModel>
{
    public LoginPage(LoginViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}

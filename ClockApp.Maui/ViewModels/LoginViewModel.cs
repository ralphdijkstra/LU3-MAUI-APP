using ClockApp.Application.Interfaces;
using ClockApp.Application.Models;
using ClockApp.Maui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClockApp.Maui.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IUserContext _userContext;
    private readonly IAppNavigator _navigator;

    public LoginViewModel(IAuthService authService, IUserContext userContext, IAppNavigator navigator)
    {
        _authService = authService;
        _userContext = userContext;
        _navigator = navigator;
    }

    public string LogoSource => "timeon_logo.png";

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private bool _isBusy;

    public override async Task OnAppearingAsync()
    {
        if (!await _authService.IsAuthenticatedAsync())
            return;

        await _userContext.LoadCachedAsync();

        if (_userContext.Current == null && !await _userContext.RefreshAsync())
        {
            await _authService.LogoutAsync();

            return;
        }

        await _navigator.GoToMainAsync();
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;

        try
        {
            var success = await _authService.LoginAsync(new LoginRequest
            {
                Username = Username.Trim(),
                Password = Password
            });

            if (!success)
            {
                ErrorMessage = "Login failed. Check your email address and password.";

                return;
            }

            if (!await _userContext.RefreshAsync())
            {
                await _authService.LogoutAsync();
                ErrorMessage = "Login succeeded, but user details could not be loaded.";

                return;
            }

            Password = string.Empty;
            await _navigator.GoToMainAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanLogin => !IsBusy;
}


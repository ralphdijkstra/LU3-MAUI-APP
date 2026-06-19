namespace ClockApp.Maui.Services;

public interface IAppNavigator
{
    Task GoToMainAsync();

    Task RefreshMainAsync();

    Task GoToLoginAsync();
}

namespace ClockApp.Maui;

public partial class App : Microsoft.Maui.Controls.Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var loginPage = IPlatformApplication.Current!.Services.GetRequiredService<Pages.LoginPage>();

		return new Window(new NavigationPage(loginPage));
	}
}

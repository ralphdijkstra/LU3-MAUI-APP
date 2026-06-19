namespace ClockApp.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		builder
			.AddPresentation()
			.AddApplication()
			.AddInfrastructure();

		return builder.Build();
	}
}

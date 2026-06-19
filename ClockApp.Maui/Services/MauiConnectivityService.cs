using ClockApp.Application.Interfaces;

namespace ClockApp.Maui.Services;

public class MauiConnectivityService : IConnectivityService
{
    public bool IsOnline => Connectivity.NetworkAccess == NetworkAccess.Internet;

    public event EventHandler? ConnectivityChanged;

    public MauiConnectivityService()
    {
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        ConnectivityChanged?.Invoke(this, EventArgs.Empty);
    }
}

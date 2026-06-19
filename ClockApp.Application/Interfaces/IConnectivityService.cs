namespace ClockApp.Application.Interfaces;

public interface IConnectivityService
{
    bool IsOnline { get; }

    event EventHandler? ConnectivityChanged;
}

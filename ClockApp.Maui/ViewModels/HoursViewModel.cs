using ClockApp.Application.Interfaces;
using ClockApp.Application.UseCases.Hours;
using ClockApp.Application.UseCases.Manager;
using ClockApp.Maui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ClockApp.Maui.ViewModels;

public partial class HoursViewModel : ViewModelBase
{
    private readonly GetAllDayHoursUseCase _getAllDayHoursUseCase;
    private readonly GetClockedInUsersUseCase _getClockedInUsersUseCase;
    private readonly UpdateHourUseCase _updateHourUseCase;
    private readonly IAuthService _authService;
    private readonly IAppNavigator _navigator;
    private readonly IConnectivityService _connectivity;

    public HoursViewModel(GetAllDayHoursUseCase getAllDayHoursUseCase, GetClockedInUsersUseCase getClockedInUsersUseCase, UpdateHourUseCase updateHourUseCase, IAuthService authService, IAppNavigator navigator, IConnectivityService connectivity)
    {
        _getAllDayHoursUseCase = getAllDayHoursUseCase;
        _getClockedInUsersUseCase = getClockedInUsersUseCase;
        _updateHourUseCase = updateHourUseCase;
        _authService = authService;
        _navigator = navigator;
        _connectivity = connectivity;

        _connectivity.ConnectivityChanged += async (_, _) => await OnConnectivityChangedAsync();
    }

    public string LogoSource => "timeon_logo.png";

    public string LogoutSource => "logout.png";

    public bool IsOffline => !IsOnline;

    public bool HasHours => DayHourCount > 0;

    public bool HasNoHours => !IsLoadingHours && DayHourCount == 0;

    public bool HasHoursError => !string.IsNullOrWhiteSpace(HoursErrorMessage);

    public bool HasEditError => !string.IsNullOrWhiteSpace(EditErrorMessage);

    public bool HasTotalDuration => !string.IsNullOrWhiteSpace(TotalDuration);

    public bool HasClockedInUsers => ClockedInUserCount > 0;

    public bool HasNoClockedInUsers => !IsLoadingClockedInUsers && ClockedInUserCount == 0;

    public ObservableCollection<ClockedInUserItemViewModel> ClockedInUsers { get; } = new();

    public ObservableCollection<DayHourItemViewModel> DayHours { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasClockedInUsers), nameof(HasNoClockedInUsers))]
    private int _clockedInUserCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasClockedInUsers), nameof(HasNoClockedInUsers))]
    private bool _isLoadingClockedInUsers;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasHours), nameof(HasNoHours))]
    private int _dayHourCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOffline))]
    private bool _isOnline;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasHours), nameof(HasNoHours))]
    [NotifyCanExecuteChangedFor(nameof(SaveEditCommand), nameof(LogoutCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasHours), nameof(HasNoHours))]
    private bool _isLoadingHours;

    [ObservableProperty]
    private string _dayTitle = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTotalDuration))]
    private string _totalDuration = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasHoursError))]
    private string _hoursErrorMessage = string.Empty;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private DayHourItemViewModel? _listSelectedHour;

    [ObservableProperty]
    private DayHourItemViewModel? _selectedHour;

    [ObservableProperty]
    private string _editStartTime = string.Empty;

    [ObservableProperty]
    private string _editDuration = string.Empty;

    [ObservableProperty]
    private string _editBreak = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEditError))]
    private string _editErrorMessage = string.Empty;

    public override async Task OnAppearingAsync()
    {
        if (!await _authService.IsAuthenticatedAsync())
        {
            await _navigator.GoToLoginAsync();

            return;
        }

        UpdateConnectivity();
        await LoadClockedInUsersAsync();
        await LoadHoursAsync();
    }

    partial void OnListSelectedHourChanged(DayHourItemViewModel? value)
    {
        if (value == null)
            return;

        SelectedHour = value;
        EditStartTime = HourFormatting.FormatTimeOfDay(value.FromSeconds);
        EditDuration = HourFormatting.FormatDuration(value.Seconds);
        EditBreak = HourFormatting.FormatDuration(value.BreakSeconds);
        EditErrorMessage = string.Empty;
        IsEditing = true;
        ListSelectedHour = null;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        SelectedHour = null;
        ListSelectedHour = null;
        EditErrorMessage = string.Empty;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteBusyCommands))]
    private async Task SaveEditAsync()
    {
        if (SelectedHour == null)
            return;

        if (!HourFormatting.TryParseTimeOfDay(EditStartTime, out var fromSeconds))
        {
            EditErrorMessage = "Voer een geldige starttijd in (uu:mm).";

            return;
        }

        if (!HourFormatting.TryParseDuration(EditDuration, out var seconds))
        {
            EditErrorMessage = "Voer een geldige duur in (uu:mm).";

            return;
        }

        if (!HourFormatting.TryParseBreakDuration(EditBreak, out var breakSeconds))
        {
            EditErrorMessage = "Voer een geldige pauze in (uu:mm).";

            return;
        }

        IsBusy = true;
        EditErrorMessage = string.Empty;

        try
        {
            var result = await _updateHourUseCase.ExecuteAsync(new UpdateHourRequest
            {
                HourId = SelectedHour.HourId,
                Date = SelectedHour.Date,
                FromSeconds = fromSeconds,
                Seconds = seconds,
                BreakSeconds = breakSeconds
            });

            if (!result.Success)
            {
                EditErrorMessage = result.ErrorMessage ?? "Opslaan mislukt.";

                return;
            }

            IsEditing = false;
            SelectedHour = null;
            await LoadHoursAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteBusyCommands))]
    private async Task RefreshAppAsync()
    {
        IsBusy = true;

        try
        {
            await _navigator.RefreshMainAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteBusyCommands))]
    private async Task LogoutAsync()
    {
        IsBusy = true;

        try
        {
            await _authService.LogoutAsync();
            await _navigator.GoToLoginAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanExecuteBusyCommands => !IsBusy;

    private async Task OnConnectivityChangedAsync()
    {
        UpdateConnectivity();

        if (IsOnline)
        {
            await LoadClockedInUsersAsync();
            await LoadHoursAsync();
        }
    }

    private async Task LoadClockedInUsersAsync()
    {
        IsLoadingClockedInUsers = true;

        try
        {
            var users = await _getClockedInUsersUseCase.ExecuteAsync();

            ClockedInUsers.Clear();

            foreach (var user in users)
                ClockedInUsers.Add(new ClockedInUserItemViewModel(user));

            ClockedInUserCount = users.Count;
        }
        finally
        {
            IsLoadingClockedInUsers = false;
        }
    }

    private async Task LoadHoursAsync()
    {
        if (!IsOnline)
        {
            HoursErrorMessage = "Geen verbinding. Uren kunnen niet worden geladen.";

            return;
        }

        IsLoadingHours = true;
        HoursErrorMessage = string.Empty;

        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var hours = await _getAllDayHoursUseCase.ExecuteAsync(today);

            DayHours.Clear();

            foreach (var hour in hours)
                DayHours.Add(new DayHourItemViewModel(hour));

            DayHourCount = hours.Count;
            DayTitle = $"Uren vandaag — {today:dd MMM}";
            TotalDuration = hours.Count > 0 ? HourFormatting.FormatDuration(hours.Sum(h => h.Seconds)) : string.Empty;
        }
        catch (Exception ex)
        {
            HoursErrorMessage = $"Uren laden mislukt: {ex.Message}";
        }
        finally
        {
            IsLoadingHours = false;
        }
    }

    private void UpdateConnectivity()
    {
        IsOnline = _connectivity.IsOnline;
    }
}

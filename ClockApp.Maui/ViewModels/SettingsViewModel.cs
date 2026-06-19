using ClockApp.Application.Interfaces;
using ClockApp.Domain.Enums;
using ClockApp.Domain.Repositories;
using ClockApp.Maui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ClockApp.Maui.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IUserContext _userContext;
    private readonly IAppNavigator _navigator;
    private readonly IConnectivityService _connectivity;
    private readonly IQrCodeRepository _qrCodeRepository;
    private readonly IWorkLocationRepository _workLocationRepository;
    private readonly IClockInModeCache _modeCache;

    private IDispatcherTimer? _qrRefreshTimer;

    public SettingsViewModel(IAuthService authService, IUserContext userContext, IAppNavigator navigator, IConnectivityService connectivity, IQrCodeRepository qrCodeRepository, IWorkLocationRepository workLocationRepository, IClockInModeCache modeCache)
    {
        _authService = authService;
        _userContext = userContext;
        _navigator = navigator;
        _connectivity = connectivity;
        _qrCodeRepository = qrCodeRepository;
        _workLocationRepository = workLocationRepository;
        _modeCache = modeCache;

        _connectivity.ConnectivityChanged += (_, _) => UpdateConnectivity();
    }

    public string LogoSource => "timeon_logo.png";

    public string LogoutSource => "logout.png";

    public bool IsOffline => !IsOnline;

    public bool IsSelfModeSelected => CheckInMode == CheckInMode.Self;

    public bool IsQrModeSelected => CheckInMode == CheckInMode.Qr;

    public bool IsGpsModeSelected => CheckInMode == CheckInMode.Gps;

    public bool HasCheckInModeError => !string.IsNullOrWhiteSpace(CheckInModeError);

    public bool ShowQrSection => IsManager && CheckInMode == CheckInMode.Qr;

    public bool ShowGpsSection => IsManager && CheckInMode == CheckInMode.Gps;

    public bool HasWorkLocations => WorkLocations.Count > 0;

    public ObservableCollection<WorkLocationItemViewModel> WorkLocations { get; } = new();

    public bool HasQrCodeImage => QrCodeImage != null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowQrSection))]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _organisationId = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOffline))]
    private bool _isOnline;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowQrSection))]
    private bool _isManager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSelfModeSelected))]
    [NotifyPropertyChangedFor(nameof(IsQrModeSelected))]
    [NotifyPropertyChangedFor(nameof(IsGpsModeSelected))]
    [NotifyPropertyChangedFor(nameof(ShowQrSection))]
    [NotifyPropertyChangedFor(nameof(ShowGpsSection))]
    private CheckInMode _checkInMode = CheckInMode.Self;

    partial void OnCheckInModeChanged(CheckInMode value)
    {
        if (!IsManager || value != CheckInMode.Gps)
            return;

        _ = LoadWorkLocationsForGpsAsync();
    }

    [ObservableProperty]
    private WorkLocationItemViewModel? _selectedWorkLocation;

    [ObservableProperty]
    private string _checkInModeError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasQrCodeImage))]
    private ImageSource? _qrCodeImage;

    [ObservableProperty]
    private string _pairingCode = string.Empty;

    [ObservableProperty]
    private string _qrValidUntilText = string.Empty;

    public override async Task OnAppearingAsync()
    {
        if (!await _authService.IsAuthenticatedAsync())
        {
            await _navigator.GoToLoginAsync();

            return;
        }

        UpdateConnectivity();
        DisplayName = _userContext.Current?.DisplayName ?? string.Empty;
        Email = _userContext.Current?.Email ?? string.Empty;
        IsManager = _userContext.IsManager;

        var organisationId = await ResolveOrganisationIdAsync();

        OrganisationId = organisationId > 0 ? organisationId.ToString() : "—";

        if (!IsManager)
            return;

        CheckInModeError = string.Empty;
        CheckInMode = organisationId > 0 ? _qrCodeRepository.GetMode(organisationId) : CheckInMode.Self;
        _modeCache.Mode = CheckInMode;
        _modeCache.LocationId = organisationId > 0 ? _qrCodeRepository.GetLocationId(organisationId) : null;

        if (CheckInMode == CheckInMode.Qr)
            await LoadQrCodeAsync(organisationId);
        else
            ClearQrDisplay();

        if (CheckInMode == CheckInMode.Gps)
            await LoadWorkLocationsForGpsAsync();
    }

    partial void OnSelectedWorkLocationChanged(WorkLocationItemViewModel? value)
    {
        foreach (var location in WorkLocations)
            location.IsSelected = location.Id == value?.Id;

        if (!IsManager || value == null || CheckInMode != CheckInMode.Gps)
            return;

        var organisationId = _userContext.Current?.OrganisationId ?? 0;

        if (organisationId <= 0)
            return;

        _qrCodeRepository.SetMode(organisationId, CheckInMode.Gps, value.Id);
        _modeCache.LocationId = value.Id;
    }

    [RelayCommand]
    private void SelectWorkLocation(WorkLocationItemViewModel location)
    {
        if (CheckInMode != CheckInMode.Gps)
            return;

        SelectedWorkLocation = location;
    }

    [RelayCommand(CanExecute = nameof(CanChangeCheckInMode))]
    private Task SelectSelfModeAsync() => UpdateCheckInModeAsync(CheckInMode.Self);

    [RelayCommand(CanExecute = nameof(CanChangeCheckInMode))]
    private Task SelectQrModeAsync() => UpdateCheckInModeAsync(CheckInMode.Qr);

    [RelayCommand(CanExecute = nameof(CanChangeCheckInMode))]
    private Task SelectGpsModeAsync() => UpdateCheckInModeAsync(CheckInMode.Gps);

    [RelayCommand]
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

    [RelayCommand]
    private async Task LogoutAsync()
    {
        IsBusy = true;

        try
        {
            StopQrRefreshTimer();
            await _authService.LogoutAsync();
            await _navigator.GoToLoginAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanChangeCheckInMode => IsManager && !IsBusy;

    private async Task UpdateCheckInModeAsync(CheckInMode mode)
    {
        if (!IsManager || IsBusy)
            return;

        if (mode == CheckInMode)
        {
            if (mode == CheckInMode.Qr)
                await LoadQrCodeAsync(_userContext.Current?.OrganisationId ?? 0);
            else if (mode == CheckInMode.Gps)
                await LoadWorkLocationsForGpsAsync();

            return;
        }

        CheckInModeError = string.Empty;
        IsBusy = true;
        SelectSelfModeCommand.NotifyCanExecuteChanged();
        SelectQrModeCommand.NotifyCanExecuteChanged();
        SelectGpsModeCommand.NotifyCanExecuteChanged();

        try
        {
            var organisationId = await ResolveOrganisationIdAsync();

            if (organisationId <= 0)
            {
                CheckInModeError = "Clock-in method could not be saved.";

                return;
            }

            _qrCodeRepository.SetMode(organisationId, mode, mode == CheckInMode.Gps ? ResolveGpsLocationId(organisationId) : null);
            CheckInMode = mode;
            _modeCache.Mode = mode;
            _modeCache.LocationId = _qrCodeRepository.GetLocationId(organisationId);

            if (mode == CheckInMode.Qr)
                await LoadQrCodeAsync(organisationId);
            else
                ClearQrDisplay();

            if (mode == CheckInMode.Gps)
                await LoadWorkLocationsForGpsAsync();
        }
        finally
        {
            IsBusy = false;
            SelectSelfModeCommand.NotifyCanExecuteChanged();
            SelectQrModeCommand.NotifyCanExecuteChanged();
            SelectGpsModeCommand.NotifyCanExecuteChanged();
        }
    }

    private async Task LoadWorkLocationsForGpsAsync()
    {
        var organisationId = await ResolveOrganisationIdAsync();
        LoadWorkLocations(organisationId);
    }

    private void LoadWorkLocations(int organisationId)
    {
        WorkLocations.Clear();

        foreach (var location in _workLocationRepository.ListForOrganisation(organisationId))
            WorkLocations.Add(new WorkLocationItemViewModel(location.Id, location.Name));

        OnPropertyChanged(nameof(HasWorkLocations));

        if (WorkLocations.Count == 0)
        {
            SelectedWorkLocation = null;

            return;
        }

        var currentId = _qrCodeRepository.GetLocationId(organisationId);
        SelectedWorkLocation = WorkLocations.FirstOrDefault(l => string.Equals(l.Id, currentId, StringComparison.OrdinalIgnoreCase)) ?? WorkLocations[0];
    }

    private string? ResolveGpsLocationId(int organisationId)
    {
        var currentId = _qrCodeRepository.GetLocationId(organisationId);
        var locations = _workLocationRepository.ListForOrganisation(organisationId);

        if (locations.Any(l => string.Equals(l.Id, currentId, StringComparison.OrdinalIgnoreCase)))
            return currentId;

        return locations.FirstOrDefault()?.Id;
    }

    private async Task LoadQrCodeAsync(int organisationId)
    {
        if (organisationId <= 0)
        {
            ClearQrDisplay();

            return;
        }

        var snapshot = _qrCodeRepository.CreatePairingCode(organisationId);

        if (snapshot == null)
        {
            ClearQrDisplay();

            return;
        }

        PairingCode = snapshot.Code;
        QrValidUntilText = $"Valid until {snapshot.ValidUntilUtc.ToLocalTime():HH:mm}";
        QrCodeImage = ImageSource.FromStream(() => new MemoryStream(snapshot.QrImagePng));
        ScheduleQrRefresh(snapshot.ValidUntilUtc);
    }

    private void ScheduleQrRefresh(DateTime validUntilUtc)
    {
        StopQrRefreshTimer();

        var delay = validUntilUtc - DateTime.UtcNow;

        if (delay <= TimeSpan.Zero)
        {
            _ = RefreshQrCodeAsync();

            return;
        }

        var dispatcher = Microsoft.Maui.Controls.Application.Current?.Dispatcher;

        if (dispatcher == null)
            return;

        _qrRefreshTimer = dispatcher.CreateTimer();
        _qrRefreshTimer.Interval = delay;
        _qrRefreshTimer.IsRepeating = false;
        _qrRefreshTimer.Tick += (_, _) => _ = RefreshQrCodeAsync();
        _qrRefreshTimer.Start();
    }

    private async Task RefreshQrCodeAsync()
    {
        if (!IsManager || CheckInMode != CheckInMode.Qr)
            return;

        var organisationId = await ResolveOrganisationIdAsync();
        await LoadQrCodeAsync(organisationId);
    }

    private void ClearQrDisplay()
    {
        StopQrRefreshTimer();
        PairingCode = string.Empty;
        QrValidUntilText = string.Empty;
        QrCodeImage = null;
    }

    private void StopQrRefreshTimer()
    {
        if (_qrRefreshTimer == null)
            return;

        _qrRefreshTimer.Stop();
        _qrRefreshTimer = null;
    }

    private void UpdateConnectivity() => IsOnline = _connectivity.IsOnline;

    private async Task<int> ResolveOrganisationIdAsync()
    {
        var organisationId = _userContext.Current?.OrganisationId ?? 0;

        if (organisationId > 0)
            return organisationId;

        await _userContext.RefreshAsync();

        return _userContext.Current?.OrganisationId ?? 0;
    }
}

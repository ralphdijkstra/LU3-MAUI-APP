using ClockApp.Application.Interfaces;
using ClockApp.Application.UseCases.Break;
using ClockApp.Application.UseCases.ClockIn;
using ClockApp.Application.UseCases.ClockOut;
using ClockApp.Application.UseCases.Hours;
using ClockApp.Application.UseCases.Location;
using ClockApp.Application.UseCases.Sync;
using ClockApp.Domain.Enums;
using ClockApp.Maui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ZXing.Net.Maui;

namespace ClockApp.Maui.ViewModels;

public partial class ClockViewModel : ViewModelBase
{
    private readonly ClockInUseCase _clockInUseCase;
    private readonly ClockOutUseCase _clockOutUseCase;
    private readonly StartBreakUseCase _startBreakUseCase;
    private readonly EndBreakUseCase _endBreakUseCase;
    private readonly GetClockStatusUseCase _getClockStatusUseCase;
    private readonly SyncPendingEntriesUseCase _syncPendingEntriesUseCase;
    private readonly GetUserDayHoursUseCase _getUserDayHoursUseCase;
    private readonly IConnectivityService _connectivity;
    private readonly IAuthService _authService;
    private readonly IAppNavigator _navigator;
    private readonly IConfiguration _configuration;
    private readonly IClockInModeCache _modeCache;
    private readonly CheckWorkLocationUseCase _checkWorkLocation;
    private readonly InMemoryQrSessionStore _qrSession;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private IDispatcherTimer? _elapsedTimer;
    private IDispatcherTimer? _locationTimer;
    private DateTime? _checkedInAt;
    private DateTime? _breakStartedAt;
    private TimeSpan _totalBreakTime;

    public ClockViewModel(ClockInUseCase clockInUseCase, ClockOutUseCase clockOutUseCase, StartBreakUseCase startBreakUseCase, EndBreakUseCase endBreakUseCase, GetClockStatusUseCase getClockStatusUseCase, SyncPendingEntriesUseCase syncPendingEntriesUseCase, GetUserDayHoursUseCase getUserDayHoursUseCase, IConnectivityService connectivity, IAuthService authService, IAppNavigator navigator, IConfiguration configuration, IClockInModeCache modeCache, CheckWorkLocationUseCase checkWorkLocation, InMemoryQrSessionStore qrSession)
    {
        _clockInUseCase = clockInUseCase;
        _clockOutUseCase = clockOutUseCase;
        _startBreakUseCase = startBreakUseCase;
        _endBreakUseCase = endBreakUseCase;
        _getClockStatusUseCase = getClockStatusUseCase;
        _syncPendingEntriesUseCase = syncPendingEntriesUseCase;
        _getUserDayHoursUseCase = getUserDayHoursUseCase;
        _connectivity = connectivity;
        _authService = authService;
        _navigator = navigator;
        _configuration = configuration;
        _modeCache = modeCache;
        _checkWorkLocation = checkWorkLocation;
        _qrSession = qrSession;

        _connectivity.ConnectivityChanged += async (_, _) => await OnConnectivityChangedAsync();
    }

    public bool IsQrMode => _modeCache.Mode == CheckInMode.Qr;

    public bool IsGpsMode => _modeCache.Mode == CheckInMode.Gps;

    public bool ShowQrEntry => IsQrMode;

    public bool ShowManualClockButtons => !IsQrMode;

    public bool ShowGpsLocationStatus => IsGpsMode;

    public bool CanClockInAtLocation => !IsGpsMode || IsAtLocation;

    public bool CanClockOutAtLocation => !IsGpsMode || IsAtLocation;

    public bool HasWorkLocationName => !string.IsNullOrWhiteSpace(WorkLocationName);

    public string LogoSource => "timeon_logo.png";

    public string LogoutSource => "logout.png";

    public bool ShowClockIn => !IsSessionActive && ShowManualClockButtons;

    public bool ShowStartBreak => IsWorking;

    public bool ShowEndBreak => IsOnBreak;

    public bool ShowClockOut => IsSessionActive && ShowManualClockButtons;

    public bool ShowActionDivider => IsQrMode ? ShowStartBreak || ShowEndBreak : ShowClockOut && (ShowClockIn || ShowStartBreak || ShowEndBreak);

    public string QrScannerHint => IsSessionActive
        ? "Scan de QR-code op locatie of vul de koppelcode in om uit te klokken."
        : "Scan de QR-code op locatie of vul de 5-letterige koppelcode in.";

    public string QrSubmitButtonText => IsSessionActive ? "Uitklokken" : "Inklokken";

    public bool CanSubmitCode => !IsBusy && PairingCode.Length == 5;

    public bool EndBreakEnabled => IsOnBreak && CanEndBreak && !IsBusy;

    public bool IsOffline => !IsOnline;

    public bool HasInfoMessage => !string.IsNullOrWhiteSpace(InfoMessage);

    public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasSessionLabel => !string.IsNullOrWhiteSpace(SessionLabel);

    public bool HasBreakLabel => !string.IsNullOrWhiteSpace(BreakLabel);

    public bool HasDayHours => DayHourCount > 0;

    public bool HasNoDayHours => !IsLoadingDayHours && DayHourCount == 0;

    public bool HasDayHoursError => !string.IsNullOrWhiteSpace(DayHoursErrorMessage);

    public bool HasDayTotalDuration => !string.IsNullOrWhiteSpace(DayTotalDuration);

    public ObservableCollection<DayHourItemViewModel> DayHours { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDayHours), nameof(HasNoDayHours))]
    private int _dayHourCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDayHours), nameof(HasNoDayHours))]
    private bool _isLoadingDayHours;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDayHoursError))]
    private string _dayHoursErrorMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDayTotalDuration))]
    private string _dayTotalDuration = string.Empty;

    [ObservableProperty]
    private string _dayHoursTitle = "Uren vandaag";

    public bool ShowElapsedTimer => IsSessionActive;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowElapsedTimer))]
    private string _elapsedDisplay = string.Empty;

    [ObservableProperty]
    private string _timerCaption = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowClockIn), nameof(ShowClockOut), nameof(ShowActionDivider), nameof(ShowElapsedTimer), nameof(QrScannerHint), nameof(QrSubmitButtonText))]
    private bool _isSessionActive;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowStartBreak), nameof(ShowActionDivider))]
    private bool _isWorking;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEndBreak), nameof(ShowActionDivider), nameof(EndBreakEnabled))]
    [NotifyCanExecuteChangedFor(nameof(EndBreakCommand))]
    private bool _isOnBreak;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EndBreakEnabled))]
    [NotifyCanExecuteChangedFor(nameof(EndBreakCommand))]
    private bool _canEndBreak;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOffline))]
    private bool _isOnline;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EndBreakEnabled), nameof(CanSubmitCode))]
    [NotifyCanExecuteChangedFor(nameof(ClockInCommand), nameof(StartBreakCommand), nameof(ClockOutCommand), nameof(LogoutCommand), nameof(RefreshAppCommand), nameof(EndBreakCommand), nameof(SubmitCodeCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasInfoMessage))]
    private string _infoMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSessionLabel))]
    private string _sessionLabel = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasBreakLabel))]
    private string _breakLabel = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanClockInAtLocation), nameof(CanClockOutAtLocation))]
    [NotifyCanExecuteChangedFor(nameof(ClockInCommand), nameof(ClockOutCommand))]
    private bool _isAtLocation;

    [ObservableProperty]
    private bool _isCheckingLocation;

    [ObservableProperty]
    private string _locationStatusText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasWorkLocationName))]
    private string _workLocationName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmitCode))]
    [NotifyCanExecuteChangedFor(nameof(SubmitCodeCommand))]
    private string _pairingCode = string.Empty;

    [ObservableProperty]
    private bool _isScanning = true;

    public override async Task OnAppearingAsync()
    {
        if (!await _authService.IsAuthenticatedAsync())
        {
            await _navigator.GoToLoginAsync();

            return;
        }

        UpdateConnectivity();
        NotifyCheckInModeChanged();
        await RefreshStatusAsync();
        await TrySyncPendingAsync();
        await LoadDayHoursAsync();

        if (IsGpsMode)
        {
            await RefreshLocationStatusAsync();
            StartLocationTimer();
        }
        else
            StopLocationTimer();

        if (IsQrMode)
            IsScanning = !IsBusy;
    }

    private void NotifyCheckInModeChanged()
    {
        OnPropertyChanged(nameof(IsQrMode));
        OnPropertyChanged(nameof(IsGpsMode));
        OnPropertyChanged(nameof(ShowQrEntry));
        OnPropertyChanged(nameof(ShowManualClockButtons));
        OnPropertyChanged(nameof(ShowClockIn));
        OnPropertyChanged(nameof(ShowClockOut));
        OnPropertyChanged(nameof(ShowActionDivider));
        OnPropertyChanged(nameof(ShowGpsLocationStatus));
        OnPropertyChanged(nameof(CanClockInAtLocation));
        OnPropertyChanged(nameof(CanClockOutAtLocation));
        OnPropertyChanged(nameof(QrScannerHint));
        OnPropertyChanged(nameof(QrSubmitButtonText));
    }

    public void OnBarcodeDetected(BarcodeResult[] results)
    {
        if (MainThread.IsMainThread)
        {
            HandleBarcodeDetected(results);

            return;
        }

        MainThread.BeginInvokeOnMainThread(() => HandleBarcodeDetected(results));
    }

    private void HandleBarcodeDetected(BarcodeResult[] results)
    {
        if (!IsQrMode || !IsScanning || IsBusy || results.Length == 0)
            return;

        var value = results[0].Value?.Trim().ToUpperInvariant() ?? string.Empty;
        var code = ExtractCode(value);

        if (code.Length != 5 || code == PairingCode)
            return;

        PairingCode = code;
    }

    [RelayCommand(CanExecute = nameof(CanSubmitCode))]
    private async Task SubmitCodeAsync()
    {
        if (IsSessionActive)
            await ClockOutWithCodeAsync(PairingCode);
        else
            await ClockInWithCodeAsync(PairingCode);
    }

    [RelayCommand(CanExecute = nameof(CanClockIn))]
    private async Task ClockInAsync()
    {
        IsBusy = true;

        try
        {
            var result = await _clockInUseCase.ExecuteAsync(new ClockInRequest { LocationId = _modeCache.LocationId });

            await RefreshStatusAsync();
            ApplyClockInMessage(result);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Inklokken mislukt: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteBusyCommands))]
    private async Task StartBreakAsync()
    {
        IsBusy = true;

        try
        {
            var result = await _startBreakUseCase.ExecuteAsync();

            await RefreshStatusAsync();
            ApplyStartBreakMessage(result);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Pauze starten mislukt: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(EndBreakEnabled))]
    private async Task EndBreakAsync()
    {
        IsBusy = true;

        try
        {
            var result = await _endBreakUseCase.ExecuteAsync();

            await RefreshStatusAsync();
            ApplyEndBreakMessage(result);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Pauze beëindigen mislukt: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanClockOut))]
    private async Task ClockOutAsync()
    {
        IsBusy = true;

        try
        {
            var result = await _clockOutUseCase.ExecuteAsync(new ClockOutRequest { LocationId = _modeCache.LocationId });

            await RefreshStatusAsync();
            ApplyClockOutMessage(result);

            if (result.Status == ClockOutStatus.PendingSync)
                await TrySyncPendingAsync();

            await LoadDayHoursAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Uitklokken mislukt: {ex.Message}";
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
            StopElapsedTimer();
            StopLocationTimer();
            await _authService.LogoutAsync();
            await _navigator.GoToLoginAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanExecuteBusyCommands => !IsBusy;

    private bool CanClockIn => CanExecuteBusyCommands && CanClockInAtLocation;

    private bool CanClockOut => CanExecuteBusyCommands && CanClockOutAtLocation;

    private async Task OnConnectivityChangedAsync()
    {
        UpdateConnectivity();
        await TrySyncPendingAsync();
        await RefreshStatusAsync();
        await LoadDayHoursAsync();
    }

    private async Task TrySyncPendingAsync()
    {
        if (!IsOnline)
            return;

        var synced = await _syncPendingEntriesUseCase.ExecuteAsync();

        if (synced > 0)
        {
            await RefreshStatusAsync();
            await LoadDayHoursAsync();
        }
    }

    private async Task RefreshStatusAsync()
    {
        await _refreshLock.WaitAsync();

        try
        {
            await RefreshStatusCoreAsync();
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task RefreshStatusCoreAsync()
    {
        var status = await _getClockStatusUseCase.ExecuteAsync();

        _checkedInAt = status.CheckedInAt;
        _breakStartedAt = status.BreakStartedAt;
        _totalBreakTime = status.TotalBreakTime;
        IsSessionActive = status.IsSessionActive;
        IsWorking = status.IsWorking;
        IsOnBreak = status.IsOnBreak;
        CanEndBreak = status.CanEndBreak;

        if (IsQrMode)
            IsScanning = !IsBusy;

        UpdateElapsedDisplay();

        if (status.IsOnBreak && status.BreakStartedAt.HasValue)
        {
            SessionLabel = string.Empty;
            UpdateBreakSubLabel(status.BreakStartedAt.Value);
        }
        else if (status.IsSessionActive && status.CheckedInAt.HasValue)
        {
            var localTime = status.CheckedInAt.Value.ToLocalTime();
            SessionLabel = $"Ingeklokt sinds {localTime:HH:mm}";
            BreakLabel = status.TotalBreakTime > TimeSpan.Zero
                ? $"Pauze genomen — {FormatBreakDuration(status.TotalBreakTime)}"
                : string.Empty;
        }
        else
        {
            SessionLabel = string.Empty;
            BreakLabel = string.Empty;
            ElapsedDisplay = string.Empty;
            TimerCaption = string.Empty;
        }

        UpdateInfoMessage(status);
    }

    private void UpdateBreakSubLabel(DateTime breakStartedAt)
    {
        if (_configuration.CanEndBreak(breakStartedAt))
            BreakLabel = string.Empty;
        else
        {
            var remaining = Math.Ceiling(_configuration.RemainingBreakTime(breakStartedAt).TotalMinutes);
            BreakLabel = $"nog {remaining} min. verplicht";
        }
    }

    private TimeSpan CalculateElapsed()
    {
        if (IsOnBreak && _breakStartedAt.HasValue)
            return DateTime.UtcNow - _breakStartedAt.Value;

        if (_checkedInAt.HasValue)
            return DateTime.UtcNow - _checkedInAt.Value - _totalBreakTime;

        return TimeSpan.Zero;
    }

    private void UpdateElapsedDisplay()
    {
        if (!IsSessionActive)
        {
            ElapsedDisplay = string.Empty;
            TimerCaption = string.Empty;

            return;
        }

        ElapsedDisplay = FormatElapsed(CalculateElapsed());
        TimerCaption = IsOnBreak ? "Pauze" : "Aan het werk";
    }

    private void OnElapsedTick()
    {
        if (!IsSessionActive)
        {
            StopElapsedTimer();

            return;
        }

        UpdateElapsedDisplay();

        if (!IsOnBreak || !_breakStartedAt.HasValue)
            return;

        var canEndBreak = _configuration.CanEndBreak(_breakStartedAt.Value);

        if (canEndBreak != CanEndBreak)
            CanEndBreak = canEndBreak;

        UpdateBreakSubLabel(_breakStartedAt.Value);
    }

    partial void OnIsSessionActiveChanged(bool value)
    {
        if (value)
        {
            StartElapsedTimer();

            return;
        }

        StopElapsedTimer();
        ElapsedDisplay = string.Empty;
        TimerCaption = string.Empty;

        if (IsQrMode)
            IsScanning = true;
    }

    private void StartElapsedTimer()
    {
        StopElapsedTimer();
        _elapsedTimer = Microsoft.Maui.Controls.Application.Current!.Dispatcher.CreateTimer();
        _elapsedTimer.Interval = TimeSpan.FromSeconds(1);
        _elapsedTimer.Tick += (_, _) => OnElapsedTick();
        _elapsedTimer.Start();
        OnElapsedTick();
    }

    private void StopElapsedTimer()
    {
        if (_elapsedTimer == null)
            return;

        _elapsedTimer.Stop();
        _elapsedTimer = null;
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        var totalHours = (int)elapsed.TotalHours;

        if (totalHours > 0)
            return $"{totalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";

        return $"{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
    }

    private void ApplyClockInMessage(ClockInResult result)
    {
        switch (result.Status)
        {
            case ClockInStatus.AlreadyCheckedIn:
                ErrorMessage = result.Message ?? "Je bent al ingeklokt.";
                break;
            case ClockInStatus.Failed:
                ErrorMessage = result.Message ?? "Inklokken is mislukt.";
                break;
            case ClockInStatus.Recorded or ClockInStatus.SavedOffline or ClockInStatus.Synced:
                if (IsSessionActive)
                    ErrorMessage = string.Empty;
                else
                    ErrorMessage = "Inklokken is opgeslagen, maar de status kon niet worden geladen. Wis lokale data of herstart de app.";
                break;
        }
    }

    private void ApplyStartBreakMessage(StartBreakResult result)
    {
        switch (result.Status)
        {
            case StartBreakStatus.NotWorking:
            case StartBreakStatus.Failed:
                ErrorMessage = result.Message ?? "Pauze starten is mislukt.";
                break;
            case StartBreakStatus.Recorded or StartBreakStatus.SavedOffline or StartBreakStatus.Synced:
                if (IsOnBreak)
                    ErrorMessage = string.Empty;
                else
                    ErrorMessage = result.Message ?? "Pauze kon niet worden gestart.";
                break;
        }
    }

    private void ApplyEndBreakMessage(EndBreakResult result)
    {
        switch (result.Status)
        {
            case EndBreakStatus.NotOnBreak:
            case EndBreakStatus.MinimumDurationNotMet:
            case EndBreakStatus.Failed:
                ErrorMessage = result.Message ?? "Pauze beëindigen is mislukt.";
                break;
            case EndBreakStatus.Recorded or EndBreakStatus.SavedOffline or EndBreakStatus.Synced:
                if (IsWorking)
                    ErrorMessage = string.Empty;
                else
                    ErrorMessage = result.Message ?? "Pauze kon niet worden beëindigd.";
                break;
        }
    }

    private void ApplyClockOutMessage(ClockOutResult result)
    {
        switch (result.Status)
        {
            case ClockOutStatus.NotCheckedIn:
            case ClockOutStatus.Failed:
            case ClockOutStatus.PendingSync:
                ErrorMessage = result.Message ?? "Uitklokken is mislukt.";
                break;
            case ClockOutStatus.Synced or ClockOutStatus.SavedOffline:
                if (!IsSessionActive)
                    ErrorMessage = string.Empty;
                else
                    ErrorMessage = result.Message ?? "Uitklokken is mislukt.";
                break;
        }
    }

    private void UpdateInfoMessage(ClockStatus status)
    {
        if (status.PendingSyncCount > 0 && !status.IsSessionActive)
        {
            InfoMessage = $"{status.PendingSyncCount} sessie(s) wachten op synchronisatie.";

            return;
        }

        if (InfoMessage.Contains("wachten op synchronisatie", StringComparison.Ordinal))
            InfoMessage = string.Empty;
    }

    private void UpdateConnectivity()
    {
        IsOnline = _connectivity.IsOnline;
    }

    private async Task LoadDayHoursAsync()
    {
        if (!IsOnline)
            return;

        IsLoadingDayHours = true;
        DayHoursErrorMessage = string.Empty;

        try
        {
            var hours = await _getUserDayHoursUseCase.ExecuteAsync();

            DayHours.Clear();

            foreach (var hour in hours)
                DayHours.Add(new DayHourItemViewModel(hour));

            DayHourCount = hours.Count;
            DayTotalDuration = hours.Count > 0 ? HourFormatting.FormatDuration(hours.Sum(h => h.Seconds)) : string.Empty;
        }
        catch (Exception ex)
        {
            DayHoursErrorMessage = $"Uren laden mislukt: {ex.Message}";
        }
        finally
        {
            IsLoadingDayHours = false;
        }
    }

    private static string FormatBreakDuration(TimeSpan duration)
    {
        if (duration.TotalMinutes < 1)
            return "minder dan 1 min";

        if (duration.TotalHours < 1)
            return $"{(int)duration.TotalMinutes} min";

        var hours = (int)duration.TotalHours;
        var minutes = duration.Minutes;

        if (minutes == 0)
            return $"{hours} uur";

        return $"{hours} uur {minutes} min";
    }

    private async Task RefreshLocationStatusAsync()
    {
        if (!IsGpsMode)
            return;

        IsCheckingLocation = true;

        try
        {
            var result = await _checkWorkLocation.ExecuteAsync();

            WorkLocationName = result.LocationName ?? string.Empty;

            switch (result.Status)
            {
                case WorkLocationCheckStatus.AtLocation:
                    LocationStatusText = "Op locatie";
                    IsAtLocation = true;
                    break;
                case WorkLocationCheckStatus.NotAtLocation:
                    LocationStatusText = "Niet op locatie";
                    IsAtLocation = false;
                    break;
                case WorkLocationCheckStatus.NoLocationConfigured:
                    LocationStatusText = "Geen locatie ingesteld";
                    IsAtLocation = false;
                    break;
                default:
                    LocationStatusText = "Locatie niet beschikbaar";
                    IsAtLocation = false;
                    break;
            }
        }
        finally
        {
            IsCheckingLocation = false;
        }
    }

    private void StartLocationTimer()
    {
        StopLocationTimer();

        var dispatcher = Microsoft.Maui.Controls.Application.Current?.Dispatcher;

        if (dispatcher == null)
            return;

        _locationTimer = dispatcher.CreateTimer();
        _locationTimer.Interval = TimeSpan.FromSeconds(8);
        _locationTimer.IsRepeating = true;
        _locationTimer.Tick += (_, _) => _ = RefreshLocationStatusAsync();
        _locationTimer.Start();
    }

    private void StopLocationTimer()
    {
        if (_locationTimer == null)
            return;

        _locationTimer.Stop();
        _locationTimer = null;
    }

    private async Task ClockInWithCodeAsync(string code)
    {
        IsBusy = true;
        IsScanning = false;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _clockInUseCase.ExecuteAsync(new ClockInRequest { QrCode = code, LocationId = _modeCache.LocationId });

            if (result.Status is ClockInStatus.Recorded or ClockInStatus.SavedOffline or ClockInStatus.Synced)
            {
                _qrSession.ValidatedCode = code;
                _qrSession.LocationId = _modeCache.LocationId;
                _qrSession.ValidatedAtUtc = DateTime.UtcNow;
                PairingCode = string.Empty;
            }

            await RefreshStatusAsync();
            ApplyClockInMessage(result);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Inklokken mislukt: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            IsScanning = true;
        }
    }

    private async Task ClockOutWithCodeAsync(string code)
    {
        IsBusy = true;
        IsScanning = false;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _clockOutUseCase.ExecuteAsync(new ClockOutRequest { QrCode = code, LocationId = _modeCache.LocationId });

            if (result.Status is ClockOutStatus.Synced or ClockOutStatus.SavedOffline or ClockOutStatus.PendingSync)
            {
                PairingCode = string.Empty;
                _qrSession.Clear();
            }

            await RefreshStatusAsync();
            ApplyClockOutMessage(result);

            if (result.Status == ClockOutStatus.PendingSync)
                await TrySyncPendingAsync();

            await LoadDayHoursAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Uitklokken mislukt: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            IsScanning = true;
        }
    }

    private static string ExtractCode(string value)
    {
        if (value.Length == 5)
            return value;

        var parts = value.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return parts.Length > 0 ? parts[^1].ToUpperInvariant() : value;
    }
}

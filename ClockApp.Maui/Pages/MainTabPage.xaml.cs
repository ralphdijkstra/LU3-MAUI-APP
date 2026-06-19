using ClockApp.Application.Interfaces;

namespace ClockApp.Maui.Pages;

public partial class MainTabPage : ContentPage
{
    private readonly ClockPage _clockPage;
    private readonly View _clockContent;
    private readonly HoursPage? _hoursPage;
    private readonly View? _hoursContent;
    private readonly SettingsPage? _settingsPage;
    private readonly View? _settingsContent;

    public MainTabPage(IServiceProvider services, IUserContext userContext)
    {
        InitializeComponent();

        _clockPage = services.GetRequiredService<ClockPage>();
        _clockContent = ExtractContent(_clockPage);
        ContentHost.Children.Add(_clockContent);

        if (userContext.IsManager)
        {
            _hoursPage = services.GetRequiredService<HoursPage>();
            _hoursContent = ExtractContent(_hoursPage);
            _hoursContent.IsVisible = false;
            ContentHost.Children.Add(_hoursContent);
            HoursTabLabel.IsVisible = true;

            _settingsPage = services.GetRequiredService<SettingsPage>();
            _settingsContent = ExtractContent(_settingsPage);
            _settingsContent.IsVisible = false;
            ContentHost.Children.Add(_settingsContent);
            SettingsTabLabel.IsVisible = true;
            TabBarGrid.ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) };
        }
        else
        {
            TabBarGrid.ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star) };
        }

        SelectClockTab();
    }

    private void OnClockTabTapped(object? sender, TappedEventArgs e) => SelectClockTab();

    private void OnHoursTabTapped(object? sender, TappedEventArgs e)
    {
        if (_hoursContent == null)
            return;

        SelectHoursTab();
    }

    private void OnSettingsTabTapped(object? sender, TappedEventArgs e)
    {
        if (_settingsContent == null)
            return;

        SelectSettingsTab();
    }

    private void SelectClockTab()
    {
        _clockContent.IsVisible = true;

        if (_hoursContent != null)
            _hoursContent.IsVisible = false;

        if (_settingsContent != null)
            _settingsContent.IsVisible = false;

        ClockTabLabel.Style = GetStyle("TimeOnBottomTabSelected");
        HoursTabLabel.Style = GetStyle("TimeOnBottomTabUnselected");
        SettingsTabLabel.Style = GetStyle("TimeOnBottomTabUnselected");

        _ = _clockPage.AppearAsync();
    }

    private void SelectHoursTab()
    {
        if (_hoursPage == null || _hoursContent == null)
            return;

        _clockContent.IsVisible = false;
        _hoursContent.IsVisible = true;

        if (_settingsContent != null)
            _settingsContent.IsVisible = false;

        ClockTabLabel.Style = GetStyle("TimeOnBottomTabUnselected");
        HoursTabLabel.Style = GetStyle("TimeOnBottomTabSelected");
        SettingsTabLabel.Style = GetStyle("TimeOnBottomTabUnselected");

        _ = _hoursPage.AppearAsync();
    }

    private void SelectSettingsTab()
    {
        if (_settingsPage == null || _settingsContent == null)
            return;

        _clockContent.IsVisible = false;

        if (_hoursContent != null)
            _hoursContent.IsVisible = false;

        _settingsContent.IsVisible = true;

        ClockTabLabel.Style = GetStyle("TimeOnBottomTabUnselected");
        HoursTabLabel.Style = GetStyle("TimeOnBottomTabUnselected");
        SettingsTabLabel.Style = GetStyle("TimeOnBottomTabSelected");

        _ = _settingsPage.AppearAsync();
    }

    private static View ExtractContent(ContentPage page)
    {
        var content = page.Content ?? throw new InvalidOperationException($"Page {page.GetType().Name} has no content.");

        page.Content = null;
        content.BindingContext = page.BindingContext;

        return content;
    }

    private static Style GetStyle(string key) => (Style)Microsoft.Maui.Controls.Application.Current!.Resources[key]!;
}

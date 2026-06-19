using ClockApp.Maui.ViewModels;

namespace ClockApp.Maui.Pages;

public class MvvmContentPage<TViewModel> : ContentPage where TViewModel : ViewModelBase
{
    protected TViewModel ViewModel { get; }

    protected MvvmContentPage(TViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = ViewModel;
        NavigationPage.SetHasNavigationBar(this, false);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.OnAppearingAsync();
    }

    public Task AppearAsync() => ViewModel.OnAppearingAsync();
}

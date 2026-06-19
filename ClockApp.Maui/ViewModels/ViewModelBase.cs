using CommunityToolkit.Mvvm.ComponentModel;

namespace ClockApp.Maui.ViewModels;

public abstract class ViewModelBase : ObservableObject, IPageLifecycle
{
    public virtual Task OnAppearingAsync() => Task.CompletedTask;
}

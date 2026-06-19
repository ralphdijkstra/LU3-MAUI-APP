using ClockApp.Maui.ViewModels;
using ZXing.Net.Maui;

namespace ClockApp.Maui.Pages;

public partial class ClockPage : MvvmContentPage<ClockViewModel>
{
    public ClockPage(ClockViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        ViewModel.OnBarcodeDetected(e.Results);
    }
}

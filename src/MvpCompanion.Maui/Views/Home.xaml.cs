using MvpCompanion.Maui.ViewModels;

namespace MvpCompanion.Maui.Views;

public partial class Home : ContentPage
{
    private HomeViewModel _viewModel;

	public Home()
	{
		InitializeComponent();
        _viewModel = new HomeViewModel();
        this.BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        await _viewModel.RefreshContributionsAsync();
    }
}
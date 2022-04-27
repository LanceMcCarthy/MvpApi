namespace MvpCompanion.Maui.Views;

public partial class Detail : ContentPage
{
	public Detail()
	{
		InitializeComponent();
	}

    private async void GoBack()
    {
        await Shell.Current.GoToAsync("..");
	}
}
using MvpCompanion.UI.ViewModels;

namespace MvpCompanion.UI.Views
{
    public class PageBase : Microsoft.UI.Xaml.Controls.Page
    {
        protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            (this.DataContext as PageViewModelBase).OnNavigatedTo(e);

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            (this.DataContext as PageViewModelBase).OnNavigatedFrom(e);
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatingFrom(Microsoft.UI.Xaml.Navigation.NavigatingCancelEventArgs e)
        {
            (this.DataContext as PageViewModelBase).OnNavigatingFrom(e);
            base.OnNavigatingFrom(e);
        }
    }
}

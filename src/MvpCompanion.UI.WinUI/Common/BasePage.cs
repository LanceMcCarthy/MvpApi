using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace MvpCompanion.UI.WinUI.Common
{
    public class BasePage : Page
    {
        public PageViewModelBase PageViewModel;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            PageViewModel.OnPageNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            PageViewModel.OnPageNavigatingFrom(e);
            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            PageViewModel.OnPageNavigatedFrom(e);
            base.OnNavigatedFrom(e);
        }
    }
}

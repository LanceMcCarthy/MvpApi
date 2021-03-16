using CommonHelpers.Common;
using Microsoft.UI.Xaml.Navigation;

namespace MvpCompanion.UI.WinUI.Common
{
    public class PageViewModelBase : ViewModelBase
    {
        public virtual void OnPageNavigatedTo(NavigationEventArgs e) {}

        public virtual void OnPageNavigatingFrom(NavigatingCancelEventArgs e) {}

        public virtual void OnPageNavigatedFrom(NavigationEventArgs e) {}
    }
}

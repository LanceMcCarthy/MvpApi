using CommonHelpers.Common;
using System.Threading.Tasks;

namespace MvpCompanion.UI.ViewModels
{
    public class PageViewModelBase : ViewModelBase
    {

        public virtual void OnNavigatingFrom(Microsoft.UI.Xaml.Navigation.NavigatingCancelEventArgs e) { /* empty */ }

        public virtual void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e) { /* empty */ }

        public virtual void OnNavigatedFrom(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e) { /* empty */ }
    }
}

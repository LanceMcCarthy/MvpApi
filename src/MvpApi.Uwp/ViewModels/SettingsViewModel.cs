using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;
using MvpApi.Uwp.Views;

namespace MvpApi.Uwp.ViewModels
{
    public class SettingsViewModel : PageViewModelBase
    {
        private bool _useBetaEditor;

        public SettingsViewModel()
        {
            if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
            {

            }
        }

        public bool UseBetaEditor
        {
            get
            {
                if (ApplicationData.Current.RoamingSettings.Values.TryGetValue("UseBetaEditor", out object rawValue))
                {
                    _useBetaEditor = (bool)rawValue;
                }
                else
                {
                    ApplicationData.Current.RoamingSettings.Values["UseBetaEditor"] = _useBetaEditor;
                }

                return _useBetaEditor;
            }
            set
            {
                if (Set(ref _useBetaEditor, value))
                {
                    ApplicationData.Current.RoamingSettings.Values["UseBetaEditor"] = _useBetaEditor;
                }
            }
        }
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (ShellPage.Instance.DataContext is ShellViewModel shellVm)
            {
                if (!shellVm.IsLoggedIn)
                {
                    await ShellPage.Instance.SignInAsync();
                }

                if (shellVm.IsLoggedIn)
                {

                }


                if (IsBusy)
                {
                    IsBusy = false;
                    IsBusyMessage = "";
                }
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using MvpApi.Uwp.Views;
using Template10.Common;
using Template10.Mvvm;

namespace MvpApi.Uwp.ViewModels
{
    public class AboutViewModel : PageViewModelBase
    {
        public AboutViewModel()
        {
            EmailCommand = new DelegateCommand(async () => await CreateEmailAsync());
        }

        public string AppVersion
        {
            get
            {
                if (DesignMode.DesignModeEnabled)
                    return "1.0";

                var nameHelper = Package.Current.Id;
                return nameHelper.Version.Major + "." + nameHelper.Version.Minor + "." + nameHelper.Version.Build;
            }
        }

        public DelegateCommand EmailCommand { get; }
        
        private async Task CreateEmailAsync()
        {
            try
            {
                IsBusy = true;
                IsBusyMessage = "opening email...";

                var email = new EmailMessage();
                email.To.Add(new EmailRecipient("awesome.apps@outlook.com", "Lancelot Software"));
                email.Subject = $"MVP Companion {AppVersion}";
                email.Body = "[write your message here]\r\n\n";

                await EmailManager.ShowComposeNewEmailAsync(email);
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Something went wrong trying to open your email application automatically. You can still manually send an email to awesome.apps@outlook.com. /r/n/nError: {ex.Message}")
                    .ShowAsync();
            }
            finally
            {
                IsBusy = false;
                IsBusyMessage = "";
            }
        }

        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (App.ShellPage.DataContext is ShellPageViewModel shellVm && shellVm.IsLoggedIn)
            {
                
            }
            else
            {
                await BootStrapper.Current.NavigationService.NavigateAsync(typeof(LoginPage));
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        #endregion
    }
}

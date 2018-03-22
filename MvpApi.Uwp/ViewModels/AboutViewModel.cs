using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Microsoft.Services.Store.Engagement;
using MvpApi.Uwp.Views;
using Template10.Common;

namespace MvpApi.Uwp.ViewModels
{
    public class AboutViewModel : PageViewModelBase
    {
        private Visibility feedbackHubButtonVisibility;

        public AboutViewModel()
        {

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

        public Visibility FeedbackHubButtonVisibility
        {
            get => feedbackHubButtonVisibility;
            set => Set(ref feedbackHubButtonVisibility, value);
        }

        public async void EmailButton_Click(object sender, RoutedEventArgs e)
        {
            await CreateEmailAsync();
        }

        public async void FeedbackButton_Click(object sender, RoutedEventArgs e)
        {
            var launcher = StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();
        }

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

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            FeedbackHubButtonVisibility = StoreServicesFeedbackLauncher.IsSupported() 
                ? Visibility.Visible 
                : Visibility.Collapsed;

            return base.OnNavigatedToAsync(parameter, mode, state);
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        #endregion
    }
}

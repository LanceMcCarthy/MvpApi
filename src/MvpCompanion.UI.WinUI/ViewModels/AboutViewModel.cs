using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.Storage;
using Windows.UI.Popups;
using MvpCompanion.UI.WinUI.Common;

namespace MvpCompanion.UI.WinUI.ViewModels
{
    public class AboutViewModel : PageViewModelBase
    {
        private string appVersion;
        private readonly ApplicationDataContainer roamingSettings;
        private Visibility feedbackHubButtonVisibility = Visibility.Collapsed;
        private int daysToKeepErrorLogs = 5;

        public AboutViewModel()
        {
            if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
            {
                AppVersion = "1.0.1";
            }
            else
            {
                roamingSettings = ApplicationData.Current.RoamingSettings;
            }
        }

        public string AppVersion
        {
            get
            {
                appVersion = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";

                return appVersion;
            }
            set => SetProperty(ref appVersion, value);
        }

        public int DaysToKeepErrorLogs
        {
            get
            {
                if (roamingSettings.Values.TryGetValue("DaysToKeepErrorLogs", out object rawValue))
                {
                    daysToKeepErrorLogs = Convert.ToInt32(rawValue);
                }
                else
                {
                    roamingSettings.Values["DaysToKeepErrorLogs"] = daysToKeepErrorLogs;
                }

                return daysToKeepErrorLogs;
            }
            set
            {
                SetProperty(ref daysToKeepErrorLogs, value);

                roamingSettings.Values["DaysToKeepErrorLogs"] = value;
            }
        }
        
        public Visibility FeedbackHubButtonVisibility
        {
            get => feedbackHubButtonVisibility;
            set => SetProperty(ref feedbackHubButtonVisibility, value);
        }

        //public bool UseBetaEditor
        //{
        //    get => (ShellPage.Instance.DataContext as ShellViewModel).UseBetaEditor;
        //    set => (ShellPage.Instance.DataContext as ShellViewModel).UseBetaEditor = value;
        //}

        // Methods

        public async void EmailButton_Click(object sender, RoutedEventArgs e)
        {
            await CreateEmailAsync();
        }

        public async void FeedbackButton_Click(object sender, RoutedEventArgs e)
        {
            //await StoreServicesFeedbackLauncher.GetDefault().LaunchAsync();
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
                await new MessageDialog(
                        $"Something went wrong trying to open your email application automatically. You can still manually send an email to awesome.apps@outlook.com. " +
                        $"/r/n/nError: {ex.Message}")
                    .ShowAsync();
            }
            finally
            {
                IsBusy = false;
                IsBusyMessage = "";
            }
        }

        #region Navigation
        
        public override void OnPageNavigatedTo(NavigationEventArgs e)
        {
            //FeedbackHubButtonVisibility = StoreServicesFeedbackLauncher.IsSupported() 
            //    ? Visibility.Visible 
            //    : Visibility.Collapsed;

            base.OnPageNavigatedTo(e);
        }

        public override void OnPageNavigatedFrom(NavigationEventArgs e)
        {
            base.OnPageNavigatedFrom(e);
        }

        public override void OnPageNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnPageNavigatingFrom(e);
        }

        #endregion
    }
}
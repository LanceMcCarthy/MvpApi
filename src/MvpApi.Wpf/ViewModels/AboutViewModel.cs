using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.Storage;
using Windows.UI.Popups;
using CommonHelpers.Common;

namespace MvpApi.Wpf.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        private readonly ApplicationDataContainer localSettings;
        private string appVersion;
        private Visibility feedbackHubButtonVisibility;
        private int daysToKeepErrorLogs = 5;

        public AboutViewModel()
        {
            //if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            //{
            //    AppVersion = "1.0.1";
            //}
            //else
            //{
            //    localSettings = ApplicationData.Current.LocalSettings;
            //}

            localSettings = ApplicationData.Current.LocalSettings;
        }

        public string AppVersion
        {
            get
            {
                appVersion = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";
                //appVersion = Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString();

                return appVersion;
            }
            set => SetProperty(ref appVersion, value);
        }

        public int DaysToKeepErrorLogs
        {
            get
            {
                if (localSettings.Values.TryGetValue("DaysToKeepErrorLogs", out object rawValue))
                {
                    daysToKeepErrorLogs = Convert.ToInt32(rawValue);
                }
                else
                {
                    localSettings.Values["DaysToKeepErrorLogs"] = daysToKeepErrorLogs;
                }

                return daysToKeepErrorLogs;
            }
            set
            {
                SetProperty(ref daysToKeepErrorLogs, value);

                localSettings.Values["DaysToKeepErrorLogs"] = value;
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

        public void OnLoadedAsync()
        {
            

        }

        public void OnUnloaded()
        {

        }

        #endregion
    }
}
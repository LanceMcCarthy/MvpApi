using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Packaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using CommonHelpers.Common;

namespace MvpApi.Wpf.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        private readonly ApplicationDataContainer _roamingSettings;
        private string _appVersion;
        private Visibility _feedbackHubButtonVisibility;
        private int _daysToKeepErrorLogs = 5;

        public AboutViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                AppVersion = "1.0.1";
            }
            else
            {
                _roamingSettings = ApplicationData.Current.RoamingSettings;
            }
        }

        public string AppVersion
        {
            get
            {
                _appVersion = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";

                return _appVersion;
            }
            set => SetProperty(ref _appVersion, value);
        }

        public int DaysToKeepErrorLogs
        {
            get
            {
                if (_roamingSettings.Values.TryGetValue("DaysToKeepErrorLogs", out object rawValue))
                {
                    _daysToKeepErrorLogs = Convert.ToInt32(rawValue);
                }
                else
                {
                    _roamingSettings.Values["DaysToKeepErrorLogs"] = _daysToKeepErrorLogs;
                }

                return _daysToKeepErrorLogs;
            }
            set
            {
                SetProperty(ref _daysToKeepErrorLogs, value);

                _roamingSettings.Values["DaysToKeepErrorLogs"] = value;
            }
        }
        
        public Visibility FeedbackHubButtonVisibility
        {
            get => _feedbackHubButtonVisibility;
            set => SetProperty(ref _feedbackHubButtonVisibility, value);
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
            await StoreServicesFeedbackLauncher.GetDefault().LaunchAsync();
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
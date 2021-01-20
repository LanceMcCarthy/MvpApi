using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.UI.Popups;
using CommonHelpers.Common;
using CommonHelpers.Mvvm;
using Microsoft.AppCenter.Crashes;

namespace MvpApi.Wpf.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private string selectedTheme = "Fluent";
        private string appVersion;
        private int daysToKeepErrorLogs = 5;

        public SettingsViewModel()
        {
            //if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            //{
            //}

            LoadSettingsValues();

            OpenUrlCommand = new DelegateCommand<string>(OpenUrl);

            SendEmailCommand = new DelegateCommand(async ()=> await CreateEmailAsync());
        }

        public DateTime SubmissionStartDate
        {
            get => ((ShellWindow)Application.Current.MainWindow).ViewModel.SubmissionStartDate;
            set => ((ShellWindow)Application.Current.MainWindow).ViewModel.SubmissionStartDate = value;
        }

        public DateTime SubmissionDeadline
        {
            get => ((ShellWindow)Application.Current.MainWindow).ViewModel.SubmissionDeadline;
            set => ((ShellWindow)Application.Current.MainWindow).ViewModel.SubmissionDeadline = value;
        }

        public string SelectedTheme
        {
            get => selectedTheme;
            set
            {
                if (SetProperty(ref selectedTheme, value))
                {
                    Properties.Settings.Default.PreferredTheme = selectedTheme;

                    if (Application.Current.MainWindow is ShellWindow sw)
                    {
                        sw.UpdateTheme(selectedTheme);
                    }
                }
            }
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
            get => daysToKeepErrorLogs;
            set
            {
                if(SetProperty(ref daysToKeepErrorLogs, value))
                {
                    Properties.Settings.Default.DaysToKeepErrorLogs = daysToKeepErrorLogs;
                }
            }
        }

        public DelegateCommand<string> OpenUrlCommand { get; set; }

        public DelegateCommand SendEmailCommand { get; set; }

        public List<string> Themes { get; } = new List<string>
        {
            "Crystal",
            "Expression_Dark",
            "Fluent",
            "Green",
            "Material",
            "Office_Black",
            "Office_Blue",
            "Office_Silver",
            "Office2013",
            "Office2016",
            "Office2016_Touch",
            "Summer",
            "Transparent",
            "Vista",
            "VisualStudio2013",
            "VisualStudio2019",
            "Windows7",
            "Windows8",
            "Windows8Touch"
        };

        private void OpenUrl(string url)
        {
            Process.Start(url);
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
                Crashes.TrackError(ex);

                await new MessageDialog($"Something went wrong trying to open your email application automatically. You can still manually send an email to awesome.apps@outlook.com. /r/n/nError: {ex.Message}")
                    .ShowAsync();
            }
            finally
            {
                IsBusy = false;
                IsBusyMessage = "";
            }
        }

        public async Task OnLoadedAsync()
        {
            if (!App.ApiService.IsLoggedIn)
            {
                IsBusy = true;
                IsBusyMessage = "signing in...";

                await App.MainLoginWindow.SignInAsync();
            }

            if (IsBusy)
            {
                IsBusy = false;
                IsBusyMessage = "";
            }
        }

        private void LoadSettingsValues()
        {
            selectedTheme = Properties.Settings.Default.PreferredTheme;
            daysToKeepErrorLogs = Properties.Settings.Default.DaysToKeepErrorLogs;
        }
    }
}

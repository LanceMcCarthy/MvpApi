using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;
using MvpApi.Services.Utilities;
using MvpApi.Uwp.Views;

namespace MvpApi.Uwp.ViewModels
{
    public class SettingsViewModel : PageViewModelBase
    {
        public SettingsViewModel()
        {
            if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
            {
                UseBetaEditor = true;
                SubmissionStartDate = ServiceConstants.SubmissionStartDate;
                SubmissionDeadline = ServiceConstants.SubmissionDeadline;
            }
        }

        public bool UseBetaEditor
        {
            get => (ShellPage.Instance.DataContext as ShellViewModel).UseBetaEditor;
            set => (ShellPage.Instance.DataContext as ShellViewModel).UseBetaEditor = value;
        }

        public DateTime SubmissionStartDate
        {
            get => ServiceConstants.SubmissionStartDate;
            set => ServiceConstants.SubmissionStartDate = value;
        }

        public DateTime SubmissionDeadline
        {
            get => ServiceConstants.SubmissionDeadline;
            set => ServiceConstants.SubmissionDeadline = value;
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (ShellPage.Instance.DataContext is ShellViewModel shellVm)
            {
                if (!shellVm.IsLoggedIn)
                {
                    await ShellPage.Instance.SignInAsync();
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

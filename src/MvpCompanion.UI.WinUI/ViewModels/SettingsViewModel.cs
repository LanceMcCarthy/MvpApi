using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Microsoft.UI.Xaml.Navigation;
using MvpCompanion.UI.WinUI.Views;
using CommonHelpers.Common;

namespace MvpCompanion.UI.WinUI.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel()
        {
            if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
            {
                UseBetaEditor = true;
            }
        }

        public bool UseBetaEditor
        {
            get => (ShellView.Instance.DataContext as ShellViewModel).UseBetaEditor;
            set => (ShellView.Instance.DataContext as ShellViewModel).UseBetaEditor = value;
        }

        public DateTime SubmissionStartDate
        {
            get => (ShellView.Instance.DataContext as ShellViewModel).SubmissionStartDate;
            set => (ShellView.Instance.DataContext as ShellViewModel).SubmissionStartDate = value;
        }

        public DateTime SubmissionDeadline
        {
            get => (ShellView.Instance.DataContext as ShellViewModel).SubmissionDeadline;
            set => (ShellView.Instance.DataContext as ShellViewModel).SubmissionDeadline = value;
        }

        public async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (ShellView.Instance.DataContext is ShellViewModel shellVm)
            {
                if (!shellVm.IsLoggedIn)
                {
                    await ShellView.Instance.SignInAsync();
                }

                if (IsBusy)
                {
                    IsBusy = false;
                    IsBusyMessage = "";
                }
            }
        }

        public async Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {

        }
    }
}

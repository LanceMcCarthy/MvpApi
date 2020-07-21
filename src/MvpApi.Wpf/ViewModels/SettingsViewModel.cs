using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using CommonHelpers.Common;

namespace MvpApi.Wpf.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                UseBetaEditor = true;
            }
        }

        public bool UseBetaEditor
        {
            get => (ShellPage.Instance.DataContext as ShellViewModel).UseBetaEditor;
            set => (ShellPage.Instance.DataContext as ShellViewModel).UseBetaEditor = value;
        }

        public DateTime SubmissionStartDate
        {
            get => (ShellPage.Instance.DataContext as ShellViewModel).SubmissionStartDate;
            set => (ShellPage.Instance.DataContext as ShellViewModel).SubmissionStartDate = value;
        }

        public DateTime SubmissionDeadline
        {
            get => (ShellPage.Instance.DataContext as ShellViewModel).SubmissionDeadline;
            set => (ShellPage.Instance.DataContext as ShellViewModel).SubmissionDeadline = value;
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
    }
}

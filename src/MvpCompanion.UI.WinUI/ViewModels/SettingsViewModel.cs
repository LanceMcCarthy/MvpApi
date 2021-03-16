using Microsoft.UI.Xaml.Navigation;
using MvpCompanion.UI.WinUI.Views;
using System;
using Windows.ApplicationModel;
using MvpCompanion.UI.WinUI.Common;

namespace MvpCompanion.UI.WinUI.ViewModels
{
    public class SettingsViewModel : PageViewModelBase
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
        
        public override async void OnPageNavigatedTo(NavigationEventArgs e)
        {
            base.OnPageNavigatedTo(e);

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

        public override void OnPageNavigatedFrom(NavigationEventArgs e)
        {
            base.OnPageNavigatedFrom(e);
        }

        public override void OnPageNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnPageNavigatingFrom(e);
        }
    }
}

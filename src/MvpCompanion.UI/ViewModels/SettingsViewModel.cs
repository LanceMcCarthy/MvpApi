using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Microsoft.UI.Xaml.Navigation;
using MvpCompanion.UI.Views;
using CommonHelpers.Common;

namespace MvpCompanion.UI.ViewModels
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

        //public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        //{
        //    if (ShellPage.Instance.DataContext is ShellViewModel shellVm)
        //    {
        //        if (!shellVm.IsLoggedIn)
        //        {
        //            await ShellPage.Instance.SignInAsync();
        //        }

        //        if (IsBusy)
        //        {
        //            IsBusy = false;
        //            IsBusyMessage = "";
        //        }
        //    }
        //}

        //public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        //{
        //    return base.OnNavigatedFromAsync(pageState, suspending);
        //}
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using CommonHelpers.Mvvm;
using MvpApi.Common.Models;
using MvpApi.Uwp.Views;
using MvpCompanion.UI.Common.Helpers;

namespace MvpApi.Uwp.ViewModels
{
    public class SettingsViewModel : PageViewModelBase
    {
        public SettingsViewModel()
        {
            if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
            {
                UseBetaEditor = true;
            }

            ExportContributionsJsonCommand = new DelegateCommand(async () => await ExportContributionsAsync());
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

        public DelegateCommand ExportContributionsJsonCommand { get; set; }

        private async Task ExportContributionsAsync()
        {

            try
            {
                IsBusy = true;
                IsBusyMessage = "loading contributions...";

                // Get the raw JSON from the API to help diagnose issues with deserialization problems
                var json = await App.ApiService.ExportContributionsAsync();

                var dataPackage = new DataPackage
                {
                    RequestedOperation = DataPackageOperation.Copy
                };

                dataPackage.SetText(json);

                Clipboard.SetContent(dataPackage);

                await new MessageDialog("The raw JSON is in your clipboard, you can paste it into any destination.", "Success").ShowAsync();

                IsBusyMessage = "";
                IsBusy = false;
            }
            catch (Exception ex)
            {

                await ex.LogExceptionWithUserMessage();
            }
            finally
            {
                IsBusyMessage = "";
                IsBusy = false;
            }
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

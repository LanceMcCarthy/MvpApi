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
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.Provider;

namespace MvpApi.Uwp.ViewModels
{
    public class SettingsViewModel : PageViewModelBase
    {
        private string selectedExportType;

        public SettingsViewModel()
        {
            if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
            {
                UseBetaEditor = true;
            }

            SelectedExportType = ContributionExportTypes[0];

            ExportContributionsCommand = new DelegateCommand(async () => await ExportContributionsAsync());

            ExportOnlineIdentitiesCommand = new DelegateCommand(async () => await ExportOnlineIdentitiesAsync());
        }

        public List<string> ContributionExportTypes => new List<string> { "All", "Current Cycle", "Historical" };

        public string SelectedExportType 
        { 
            get => selectedExportType;
            set => Set(ref selectedExportType, value); 
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

        public DelegateCommand ExportContributionsCommand { get; set; }

        public DelegateCommand ExportOnlineIdentitiesCommand { get; set; }

        private async Task ExportContributionsAsync()
        {
            try
            {
                IsBusy = true;
                IsBusyMessage = $"Exporting {SelectedExportType} contributions...";

                string json = string.Empty;

                switch (SelectedExportType)
                {
                    case "Current Cycle":
                        json = await App.ApiService.ExportCurrentContributionsAsync();
                        break;
                    case "Historical":
                        json = await App.ApiService.ExportHistoricalContributionsAsync();
                        break;
                    case "All":
                    default:
                        json = await App.ApiService.ExportAllContributionsAsync();
                        break;
                }

                await ChooseDataDestinationAsync(json, $"MVPCompanion_{SelectedExportType}_Activities.json");

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

        private async Task ExportOnlineIdentitiesAsync()
        {
            try
            {
                IsBusy = true;
                IsBusyMessage = $"Exporting online identities...";

                var json = await App.ApiService.ExportOnlineIdentitiesAsync();

                await ChooseDataDestinationAsync(json, "MVPCompanion_OnlineIdentities.json");

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

        private async Task ChooseDataDestinationAsync(string data, string defaultFileName)
        {
            string resultMessage = "";

            var md = new MessageDialog("Where do you want to export the data to?", "Select Desintation");

            md.Commands.Add(new UICommand("Cancel"));

            md.Commands.Add(new UICommand("Clipboard", (c) =>
            {
                var dataPackage = new DataPackage
                {
                    RequestedOperation = DataPackageOperation.Copy
                };

                dataPackage.SetText(data);

                Clipboard.SetContent(dataPackage);

                resultMessage = "The exported data is in your clipboard, you can paste it in your desired destination (NotePad, email, etc)";
            }));

            md.Commands.Add(new UICommand("File", async (c) =>
            {
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.Downloads;
                savePicker.FileTypeChoices.Add("MVP Companion Export", new List<string>() { ".json" });
                savePicker.SuggestedFileName = defaultFileName;

                StorageFile file = await savePicker.PickSaveFileAsync();

                if (file != null)
                {
                    CachedFileManager.DeferUpdates(file);

                    await FileIO.WriteTextAsync(file, data);

                    var status = await CachedFileManager.CompleteUpdatesAsync(file);

                    if (status == FileUpdateStatus.Complete)
                    {
                        resultMessage = $"{file.Name} was successfully saved.";
                    }
                }
            }));

            md.DefaultCommandIndex = 0;

            await md.ShowAsync();

            if (!string.IsNullOrEmpty(resultMessage))
            {
                await new MessageDialog(resultMessage).ShowAsync();
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

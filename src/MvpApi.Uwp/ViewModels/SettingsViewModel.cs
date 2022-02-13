using System;
using System.Collections.Generic;
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
using Newtonsoft.Json;

namespace MvpApi.Uwp.ViewModels
{
    public class SettingsViewModel : PageViewModelBase
    {
        private string selectedExportType = "All";

        public SettingsViewModel()
        {
            if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
            {
                UseBetaEditor = true;
            }
            
            ExportContributionsCommand = new DelegateCommand(async () => await ExportContributionsAsync());
            ExportOnlineIdentitiesCommand = new DelegateCommand(async () => await ExportOnlineIdentitiesAsync());
            ImportContributionsTestCommand = new DelegateCommand(async () => await ImportContributionsTestAsync());
        }

        public List<string> ContributionExportTypes => new List<string> { "All", "Current", "Historical" };

        public string SelectedExportType 
        { 
            get => selectedExportType;
            set
            {
                if (selectedExportType == value)
                    return;

                selectedExportType = value;
                RaisePropertyChanged(nameof(SelectedExportType));
            }
        }

        public bool UseBetaEditor
        {
            get => ((ShellViewModel)ShellPage.Instance.DataContext).UseBetaEditor;
            set => ((ShellViewModel)ShellPage.Instance.DataContext).UseBetaEditor = value;
        }

        public DateTime SubmissionStartDate
        {
            get => ((ShellViewModel)ShellPage.Instance.DataContext).SubmissionStartDate;
            set => ((ShellViewModel)ShellPage.Instance.DataContext).SubmissionStartDate = value;
        }

        public DateTime SubmissionDeadline
        {
            get => ((ShellViewModel)ShellPage.Instance.DataContext).SubmissionDeadline;
            set => ((ShellViewModel)ShellPage.Instance.DataContext).SubmissionDeadline = value;
        }

        public DelegateCommand ExportContributionsCommand { get; set; }

        public DelegateCommand ExportOnlineIdentitiesCommand { get; set; }

        public DelegateCommand ImportContributionsTestCommand { get; set; }

        private async Task ImportContributionsTestAsync()
        {
            try
            {
                var picker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.List,
                    SuggestedStartLocation = PickerLocationId.Downloads,
                    CommitButtonText = "Start Test"
                };
                picker.FileTypeFilter.Add(".json");
                
                var file = await picker.PickSingleFileAsync();

                if (file == null)
                    return;

                var jsonData = await FileIO.ReadTextAsync(file);

                var deserializedResult = JsonConvert.DeserializeObject<List<ContributionsModel>>(jsonData);
                
                await new MessageDialog($"The json data was successfully been deserialized, found ({deserializedResult.Count}) records.\r\n\nThe app shouldn't have any problem using that award cycle's data from the MVP API.", "Successful Deserialization").ShowAsync();

            }
            catch (Exception ex)
            {
               await new MessageDialog($"The json file was NOT able to be deserialized, contact awesome.apps@outlook.com and share this error sp we can fix it: \r\n{ex.Message}", "Unsuccessful").ShowAsync();
            }
        }

        private async Task ExportContributionsAsync()
        {
            try
            {
                IsBusy = true;
                IsBusyMessage = $"Exporting {SelectedExportType} contributions, please be patient this could take a while...";

                string json = string.Empty;

                switch (SelectedExportType)
                {
                    case "Current":
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
                
                var savePicker = new FileSavePicker
                {
                    SuggestedFileName = $"MVPCompanion_{SelectedExportType}_Activities {DateTime.Now:yyyy-dd-M--HH-mm-ss}.json",
                    SuggestedStartLocation = PickerLocationId.Downloads
                };
                savePicker.FileTypeChoices.Add("MVP Companion Export", new List<string>() { ".json" });

                StorageFile file = await savePicker.PickSaveFileAsync();

                if (file != null)
                {
                    CachedFileManager.DeferUpdates(file);

                    await FileIO.WriteTextAsync(file, json);

                    var status = await CachedFileManager.CompleteUpdatesAsync(file);

                    await ShowFileSaveResultAsync(status);
                }

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
                IsBusyMessage = $"Exporting online identities, this should be quick...";

                var json = await App.ApiService.ExportOnlineIdentitiesAsync();
                
                var savePicker = new FileSavePicker
                {
                    SuggestedFileName = $"MVPCompanion_OnlineIdentities {DateTime.Now:yyyy-dd-M--HH-mm-ss}.json",
                    SuggestedStartLocation = PickerLocationId.Downloads
                };
                savePicker.FileTypeChoices.Add("MVP Companion Export", new List<string>() { ".json" });

                StorageFile file = await savePicker.PickSaveFileAsync();

                if (file != null)
                {
                    CachedFileManager.DeferUpdates(file);

                    await FileIO.WriteTextAsync(file, json);

                    var status = await CachedFileManager.CompleteUpdatesAsync(file);

                    await ShowFileSaveResultAsync(status);
                }

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

        private static async Task ShowFileSaveResultAsync(FileUpdateStatus status)
        {
            if (status == FileUpdateStatus.Complete || status == FileUpdateStatus.CompleteAndRenamed)
            {
                var message = "If you want to open this in Excel (to save as xlsx or csv), take these steps:\r\n\n" +
                              "1. Click the 'Data' tab, then 'Get Data' > 'From File' > 'From JSON'. \n" +
                              "2. Browse to where you saved the json file, select it, and click 'Open'. \n" +
                              "3. Once the Query Editor has loaded your data, click 'Convert > Into Table', then 'Close & Load'.\n" +
                              "4. Now you can us 'Save As' to xlsx file or csv.";

                await new MessageDialog(message, "Export Saved!").ShowAsync();
            }
            else
            {
                await new MessageDialog(
                    $"Unfortunately, something went wrong saving the file (Status: '{status}').\r\n\n" +
                    $"If you got an Unavailable or Fail status, wait and try again or use a different file name.",
                    "Unsuccessful").ShowAsync();
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

using CommonHelpers.Mvvm;
using MvpApi.Common.Models;
using MvpApi.Uwp.Views;
using MvpCompanion.UI.Common.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MvpApi.Uwp.Dialogs;

namespace MvpApi.Uwp.ViewModels
{
    public class SettingsViewModel : PageViewModelBase
    {
        private string selectedExportType = "All";
        private string importContributionsStatus = "All";
        private string importOnlineIdentitiesStatus = "All";

        public SettingsViewModel()
        {
            ExportContributionsCommand = new DelegateCommand(async () => await ExportContributionsAsync());
            ExportOnlineIdentitiesCommand = new DelegateCommand(async () => await ExportOnlineIdentitiesAsync());
            ImportContributionsTestCommand = new DelegateCommand(async () => await ImportContributionsTestAsync());
            ImportOnlineIdentitiesTestCommand = new DelegateCommand(async () => await ImportOnlineIdentitiesTestAsync());
        }

        public List<string> ContributionExportTypes => new List<string> { "All", "Current", "Historical" };

        public string SelectedExportType 
        { 
            get => selectedExportType;
            set => Set(ref selectedExportType, value);
        }

        public string ImportContributionsStatus
        {
            get => importContributionsStatus;
            set
            {
                if (importContributionsStatus == value)
                    return;

                importContributionsStatus = value;
                RaisePropertyChanged(nameof(ImportContributionsStatus));
            }
        }
        
        public string ImportOnlineIdentitiesStatus
        {
            get => importOnlineIdentitiesStatus;
            set => Set(ref importOnlineIdentitiesStatus, value);
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

        public DelegateCommand ImportOnlineIdentitiesTestCommand { get; set; }

        private async Task ImportContributionsTestAsync()
        {
            try
            {
                ImportContributionsStatus = "waiting for file selection...";

                var picker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.List,
                    SuggestedStartLocation = PickerLocationId.Downloads,
                    CommitButtonText = "Start Test"
                };

                picker.FileTypeFilter.Add(".json");
                
                var file = await picker.PickSingleFileAsync();

                if (file != null)
                {
                    ImportContributionsStatus = "reading data file contents...";

                    var jsonData = await FileIO.ReadTextAsync(file);

                    ImportContributionsStatus = "deserializing json...";

                    var deserializedResult = JsonConvert.DeserializeObject<List<ContributionsModel>>(jsonData);
                    
                    ImportContributionsStatus = $"Test Successful, {deserializedResult.Count} contributions deserialized.";
                    
                    var testImportDialog = new ImportContributionsDialog(deserializedResult);
                    testImportDialog.IsPrimaryButtonEnabled = false;

                    await testImportDialog.ShowAsync();
                }
                else
                {
                    ImportContributionsStatus = "file selection cancelled.";
                }
            }
            catch (Exception ex)
            {
                ImportContributionsStatus = $"Error: {ex.Message}";

                await new MessageDialog("The json data could not be deserialized, make sure you have chosen the correct file.\r\n\n" +
                                        "If this keeps happening, it could be a sign of corrupt API data. Contact awesome.apps@outlook.com and share the file so we investigate.\r\n\n" +
                                        $"Error: {ex.Message}",
                    "Unsuccessful").ShowAsync();
            }
        }

        private async Task ImportOnlineIdentitiesTestAsync()
        {
            try
            {
                ImportOnlineIdentitiesStatus = "waiting for file selection...";

                var picker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.List,
                    SuggestedStartLocation = PickerLocationId.Downloads,
                    CommitButtonText = "Start Test"
                };

                picker.FileTypeFilter.Add(".json");

                var file = await picker.PickSingleFileAsync();

                if (file != null)
                {
                    ImportOnlineIdentitiesStatus = "reading data file contents...";

                    var jsonData = await FileIO.ReadTextAsync(file);

                    ImportOnlineIdentitiesStatus = "deserializing json...";

                    var deserializedResult = JsonConvert.DeserializeObject<List<OnlineIdentityViewModel>>(jsonData);

                    ImportOnlineIdentitiesStatus = $"Test Successful,{deserializedResult.Count} records deserialized.";
                }
                else
                {
                    ImportOnlineIdentitiesStatus = "file selection cancelled.";
                }
            }
            catch (Exception ex)
            {
                ImportOnlineIdentitiesStatus = $"Error: {ex.Message}";

                await new MessageDialog("The json data could not be deserialized, make sure you have chosen the correct file.\r\n\n" +
                                        "If this keeps happening, it could be a sign of corrupt API data. Contact awesome.apps@outlook.com and share the file so we investigate.\r\n\n" +
                                        $"Error: {ex.Message}", 
                    "Unsuccessful").ShowAsync();
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
                await new MessageDialog("The data has been successfully serialized and saved as a json data file.", "Export Saved!").ShowAsync();
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

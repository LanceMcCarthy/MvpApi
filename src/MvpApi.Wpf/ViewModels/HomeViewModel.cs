using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
//using Windows.Storage;
//using Windows.Storage.Pickers;
//using Windows.Storage.Provider;
using Windows.UI.Popups;
using CommonHelpers.Common;
using CommonHelpers.Mvvm;
using MvpApi.Common.Models;
using MvpApi.Wpf.Helpers;
using Telerik.Windows.Data;

namespace MvpApi.Wpf.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        #region Fields

        private DataGridSelectionMode gridSelectionMode = DataGridSelectionMode.Single;
        private bool isMultipleSelectionEnabled;
        private ObservableCollection<ContributionsModel> contributions;
        private bool areAppBarButtonsEnabled;
        private bool isInternetDisabled;

        #endregion

        public HomeViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var designItems = DesignTimeHelpers.GenerateContributions();

                Contributions = new ObservableCollection<ContributionsModel>();

                foreach (var contribution in designItems)
                {
                    Contributions.Add(contribution);
                }
            }

            RefreshAfterDisconnectCommand = new DelegateCommand(async () =>
            {
                IsInternetDisabled = !NetworkHelper.Current.CheckInternetConnection();

                if (IsInternetDisabled)
                {
                    await new MessageDialog("Internet is still not available, please check your connection and try again.", "No Internet").ShowAsync();
                }
                else
                {

                }
            });
        }

        #region Properties

        public ObservableCollection<ContributionsModel> Contributions
        {
            get => contributions;
            set => SetProperty(ref contributions, value);
        }

        public ObservableCollection<object> SelectedContributions { get; set; }

        public GroupDescriptorCollection GroupDescriptors { get; set; }

        public DelegateCommand RefreshAfterDisconnectCommand { get; }

        public bool IsMultipleSelectionEnabled
        {
            get => isMultipleSelectionEnabled;
            set
            {
                SetProperty(ref isMultipleSelectionEnabled, value);

                GridSelectionMode = value
                    ? DataGridSelectionMode.Extended
                    : DataGridSelectionMode.Single;
            }
        }

        public DataGridSelectionMode GridSelectionMode
        {
            get => gridSelectionMode;
            set => SetProperty(ref gridSelectionMode, value);
        }

        public bool AreAppBarButtonsEnabled
        {
            get => areAppBarButtonsEnabled;
            set => SetProperty(ref areAppBarButtonsEnabled, value);
        }

        public bool IsInternetDisabled
        {
            get => isInternetDisabled;
            set => SetProperty(ref isInternetDisabled, value);
        }

        #endregion

        #region Event Handlers

        public async void AddActivityButton_Click(object sender, RoutedEventArgs e)
        {
            //(App.Current.MainWindow as ShellWindow).RootNavigationView.Content = new AddContributionView();
        }

        public void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedContributions.Any())
                SelectedContributions.Clear();
        }

        public async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedContributions.Any())
                SelectedContributions.Clear();

            await LoadContributionsAsync();
        }

        public async void DeleteSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsBusy = true;
                IsBusyMessage = "preparing to delete contributions...";

                foreach (ContributionsModel contribution in SelectedContributions)
                {
                    IsBusyMessage = $"deleting {contribution.Title}...";

                    var success = await App.ApiService.DeleteContributionAsync(contribution);

                    // Quality assurance, only logs a successful or failed delete.
                    //if (ApiInformation.IsTypePresent("Microsoft.Services.Store.Engagement.StoreServicesCustomEventLogger"))
                    //    StoreServicesCustomEventLogger.GetDefault().Log(success == true ? "DeleteContributionSuccess" : "DeleteContributionFailure");
                }

                SelectedContributions.Clear();

                // After deleting contributions, we need to fetch updated list
                IsBusyMessage = "refreshing contributions...";
                await LoadContributionsAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteSelectedContributions Exception: {ex}");
            }
            finally
            {
                IsBusy = false;
                IsBusyMessage = "";
            }
        }

        public void GroupingToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (!(sender is RadioButton rb) || rb.Content == null || GroupDescriptors == null) return;

            GroupDescriptors.Clear();

            var groupName = rb.Content.ToString();

            switch (groupName)
            {
                case "None":
                    GroupDescriptors.Clear();
                    break;
                case "Date":
                    GroupDescriptors.Add(new GroupDescriptor { DisplayContent = "Start Date", Member = "StartDate.Date" });
                    break;
                case "Award Area":
                    GroupDescriptors.Add(new GroupDescriptor { DisplayContent = "Award Name", Member = "ContributionTechnology.Name",  });
                    break;
                case "Contribution Type":
                    GroupDescriptors.Add(new GroupDescriptor { DisplayContent = "Contribution Type", Member = "ContributionTypeName" });
                    break;
            }
        }

        public async void ExportButton_OnClick(object sender, RoutedEventArgs e)
        {
            IsBusy = true;
            IsBusyMessage = "exporting all contributions...";

            var jsonData = await App.ApiService.ExportContributionsAsync();

            if (string.IsNullOrEmpty(jsonData))
                return;

            //var savePicker = new FileSavePicker
            //{
            //    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            //    SuggestedFileName = $"MVP Contributions {DateTime.Now:yyyy-dd-M--HH-mm-ss}"
            //};

            //savePicker.FileTypeChoices.Add("JSON Data", new List<string> { ".json" });

            //var file = await savePicker.PickSaveFileAsync();

            //if (file != null)
            //{
            //    IsBusyMessage = "saving file...";

            //    // prevents file changes by syncing services like OneDrive
            //    CachedFileManager.DeferUpdates(file);

            //    await FileIO.WriteTextAsync(file, jsonData);

            //    // releases the hold on the file so syncing services can make changes
            //    var status = await CachedFileManager.CompleteUpdatesAsync(file);

            //    if (status == FileUpdateStatus.Complete)
            //    {
            //        var message = "If you want to open this in Excel (to save as xlsx or csv), take these steps:\r\n\n" +
            //                      "1. Click the 'Data' tab, then 'Get Data' > 'From File' > 'From JSON'. \n" +
            //                      "2. Browse to where you saved the json file, select it, and click 'Open'. \n" +
            //                      "3. Once the Query Editor has loaded your data, click 'Convert > Into Table', then 'Close & Load'.\n" +
            //                      "4. Now you can us 'Save As' to xlsx file or csv.";

            //        await new MessageDialog(message, "Export Saved").ShowAsync();
            //    }
            //}

            IsBusyMessage = "";
            IsBusy = false;
        }

        #endregion

        #region Methods

        //private async Task<IEnumerable<ContributionsModel>> LoadMoreItems(uint count)
        //{
        //    try
        //    {
        //        // Here we use a different flag when the view model is busy loading items because we don't want to cover the UI
        //        // The IsBusy flag is used for when deleting items, when we want to block the UI
        //        IsLoadingMoreItems = true;

        //        var result = await App.ApiService.GetContributionsAsync(_currentOffset, (int)count);

        //        Debug.WriteLine($"** LoadMoreItems **\nPagingIndex: {result.PagingIndex}, Count: {result.Contributions.Count}, TotalContributions: {result.TotalContributions}");

        //        _currentOffset = result.PagingIndex;

        //        DisplayTotal = $"{_currentOffset} of {result.TotalContributions}";

        //        // If we've received all the contributions, return null to stop automatic loading
        //        if (result.PagingIndex == result.TotalContributions)
        //            return null;

        //        return result.Contributions;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Only log this exception after the user is logged in
        //        if (App.ShellPage?.DataContext is ShellPageViewModel shellVm && shellVm.IsLoggedIn)
        //        {
        //            await ex.LogExceptionAsync();
        //            Debug.WriteLine($"LoadMoreItems Exception: {ex}");
        //        }

        //        return null;
        //    }
        //    finally
        //    {
        //        IsLoadingMoreItems = false;
        //    }
        //}

        private async Task LoadContributionsAsync()
        {
            try
            {
                IsBusy = true;
                IsBusyMessage = "loading contributions...";
                
                // Get all the contributions for the currently signed in MVP.
                var result = await App.ApiService.GetAllContributionsAsync();

                // Load the items into the DataGrid
                Contributions = new ObservableCollection<ContributionsModel>(result.Contributions);

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

        #endregion

        #region Navigation

        public async Task OnLoadedAsync()
        {
            IsInternetDisabled = !NetworkHelper.Current.CheckInternetConnection();

            if (IsInternetDisabled)
            {
                await new MessageDialog("This application requires an internet connection. Please check your connection and try again.", "No Internet").ShowAsync();
                return;
            }

            if ((App.Current.MainWindow as ShellWindow).DataContext is ShellViewModel shellVm)
            {
                if (!shellVm.IsLoggedIn)
                {
                    await (App.Current.MainWindow as ShellWindow).SignInAsync();
                }

                // Although user should be logged in at this point, still check
                // TODO Use NeedsHomePageRefresh property to determine to reload the contributions
                if (shellVm.IsLoggedIn)
                {
                    await LoadContributionsAsync();
                }

                //if (!(ApplicationData.Current.LocalSettings.Values["HomePageTutorialShown"] is bool tutorialShown) || !tutorialShown)
                //{
                    //var td = new TutorialDialog
                    //{
                    //    SettingsKey = "HomePageTutorialShown",
                    //    MessageTitle = "Home Page",
                    //    Message = "Welcome MVP! This page lists your contributions, which are automatically loaded on-demand as you scroll down.\r\n\n" +
                    //              "- Group or sort the contributions by any column.\r\n" +
                    //              "- Select a contribution to view its details or edit it.\r\n" +
                    //              "- Select the 'Add' button to upload new contributions (single or in bulk).\r\n" +
                    //              "- Select the 'Multi-Select' button to enter multi-select mode (for item deletion)."
                    //};

                    //await td.ShowAsync();
                //}

                if (IsBusy)
                {
                    IsBusy = false;
                    IsBusyMessage = "";
                }
            }
        }

        #endregion
    }
}
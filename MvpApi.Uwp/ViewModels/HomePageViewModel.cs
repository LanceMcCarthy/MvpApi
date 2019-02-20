using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Services.Store.Engagement;
using Microsoft.Toolkit.Uwp.Connectivity;
using MvpApi.Common.Models;
using MvpApi.Uwp.Common;
using MvpApi.Uwp.Dialogs;
using MvpApi.Uwp.Helpers;
using MvpApi.Uwp.Views;
using Telerik.Data.Core;
using Telerik.UI.Xaml.Controls.Grid;
using Template10.Common;
using Template10.Mvvm;

namespace MvpApi.Uwp.ViewModels
{
    public class HomePageViewModel : PageViewModelBase
    {
        #region Fields
        
        private DataGridSelectionMode _gridSelectionMode = DataGridSelectionMode.Single;
        private bool _isMultipleSelectionEnabled;
        private ObservableCollection<ContributionsModel> _contributions;
        private bool _areAppBarButtonsEnabled;
        private bool _isInternetDisabled;

        #endregion

        public HomePageViewModel()
        {
            if (DesignMode.DesignModeEnabled)
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
                IsInternetDisabled = !NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable;

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
            get => _contributions;
            set => Set(ref _contributions, value);
        }

        public ObservableCollection<object> SelectedContributions { get; set; }

        public GroupDescriptorCollection GroupDescriptors { get; set; }
        
        public DelegateCommand RefreshAfterDisconnectCommand { get; }
        
        public bool IsMultipleSelectionEnabled
        {
            get => _isMultipleSelectionEnabled;
            set
            {
                Set(ref _isMultipleSelectionEnabled, value);

                GridSelectionMode = value
                    ? DataGridSelectionMode.Multiple
                    : DataGridSelectionMode.Single;
            }
        }

        public DataGridSelectionMode GridSelectionMode
        {
            get => _gridSelectionMode;
            set => Set(ref _gridSelectionMode, value);
        }

        public bool AreAppBarButtonsEnabled
        {
            get => _areAppBarButtonsEnabled;
            set => Set(ref _areAppBarButtonsEnabled, value);
        }
        
        public bool IsInternetDisabled
        {
            get => _isInternetDisabled;
            set => Set(ref _isInternetDisabled, value);
        }
        
        #endregion

        #region Event Handlers
        
        public async void AddActivityButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO Remove temporary navigation blocker
            await new MessageDialog(
                    "The MVP API has made breaking changes and the app cannot make any edits or add new contributions at this time. " +
                    "\n\nI expect to release an update within a day or two with a fix, thank you for your patience - Lance", "Temporarily Disabled")
                .ShowAsync();

            //await BootStrapper.Current.NavigationService.NavigateAsync(typeof(AddContributionsPage));
        }

        public void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedContributions.Any())
                SelectedContributions.Clear();
        }

        public async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedContributions.Any())
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
                    if (ApiInformation.IsTypePresent("Microsoft.Services.Store.Engagement.StoreServicesCustomEventLogger"))
                        StoreServicesCustomEventLogger.GetDefault().Log(success == true ? "DeleteContributionSuccess" : "DeleteContributionFailure");
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

        public async void RadDataGrid_OnSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            // When in multiple selection mode, enable/disable delete instead of navigating to details page
            if (GridSelectionMode == DataGridSelectionMode.Multiple)
            {
                AreAppBarButtonsEnabled = e?.AddedItems.Any() == true;
                return;
            }

            // When in single selection mode, go to the selected item's details page
            if (GridSelectionMode == DataGridSelectionMode.Single && e?.AddedItems?.FirstOrDefault() is ContributionsModel contribution)
            {
                await BootStrapper.Current.NavigationService.NavigateAsync(typeof(ContributionDetailPage), contribution);
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
                    // do nothing because we've already cleared the GroupDescriptors
                    break;
                case "Date":
                    // Custom group descriptor to group by DateTime.Date  
                    GroupDescriptors.Add(new DelegateGroupDescriptor { DisplayContent = "Start Date", KeyLookup = new DateTimeMonthKeyLookup() });
                    break;
                case "Award Area":
                    // Need a custom descriptor to group by ContributionTechnology.Name
                    GroupDescriptors.Add(new DelegateGroupDescriptor { DisplayContent = "Award Name", KeyLookup = new TechnologyKeyLookup() });
                    break;
                case "Contribution Type":
                    GroupDescriptors.Add(new PropertyGroupDescriptor { DisplayContent = "Contribution Type", PropertyName = nameof(ContributionsModel.ContributionTypeName) });
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

            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = $"MVP Contributions {DateTime.Now:yyyy-dd-M--HH-mm-ss}"
            };

            savePicker.FileTypeChoices.Add("JSON Data", new List<string> { ".json" });

            var file = await savePicker.PickSaveFileAsync();
            
            if (file != null)
            {
                IsBusyMessage = "saving file...";

                // prevents file changes by syncing services like OneDrive
                CachedFileManager.DeferUpdates(file);

                await FileIO.WriteTextAsync(file, jsonData);

                // releases the hold on the file so syncing services can make changes
                var status = await CachedFileManager.CompleteUpdatesAsync(file);

                if (status == FileUpdateStatus.Complete)
                {
                    var message = "If you want to open this in Excel (to save as xlsx or csv), take these steps:\r\n\n" +
                                  "1. Click the 'Data' tab, then 'Get Data' > 'From File' > 'From JSON'. \n" +
                                  "2. Browse to where you saved the json file, select it, and click 'Open'. \n" +
                                  "3. Once the Query Editor has loaded your data, click 'Convert > Into Table', then 'Close & Load'.\n" +
                                  "4. Now you can us 'Save As' to xlsx file or csv.";

                    await new MessageDialog(message, "Export Saved").ShowAsync();
                }
            }

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

                // Just load one item from the API so we can get the total number of items
                var pingResult = await App.ApiService.GetContributionsAsync(0, 1);

                // Read the total number of items
                var itemsToFetch = Convert.ToInt32(pingResult.TotalContributions);
                
                // Do the complete fetch now
                var result = await App.ApiService.GetContributionsAsync(0, itemsToFetch);
                
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

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            IsInternetDisabled = !NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable;

            if (IsInternetDisabled)
            {
                await new MessageDialog("This application requires an internet connection. Please check your connection and try again.", "No Internet").ShowAsync();
                return;
            }

            if (ShellPage.Instance.DataContext is ShellPageViewModel shellVm)
            {
                if (!shellVm.IsLoggedIn)
                {
                    await ShellPage.Instance.SignInAsync();
                }

                // Although user should be logged in at this point, still check
                if (shellVm.IsLoggedIn)
                {
                    await LoadContributionsAsync();
                }
                
                if (!(ApplicationData.Current.LocalSettings.Values["HomePageTutorialShown"] is bool tutorialShown) || !tutorialShown)
                {
                    var td = new TutorialDialog
                    {
                        SettingsKey = "HomePageTutorialShown",
                        MessageTitle = "Home Page",
                        Message = "Welcome MVP! This page lists your contributions, which are automatically loaded on-demand as you scroll down.\r\n\n" +
                                  "- Group or sort the contributions by any column.\r\n" +
                                  "- Select a contribution to view its details or edit it.\r\n" +
                                  "- Select the 'Add' button to upload new contributions (single or in bulk).\r\n" +
                                  "- Select the 'Multi-Select' button to enter multi-select mode (for item deletion)."
                    };

                    await td.ShowAsync();
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

        #endregion
    }
}
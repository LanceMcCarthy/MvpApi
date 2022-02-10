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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Microsoft.Services.Store.Engagement;
using Microsoft.Toolkit.Uwp.Connectivity;
using MvpApi.Common.Interfaces;
using MvpApi.Common.Models;
using MvpApi.Uwp.Common;
using MvpApi.Uwp.Dialogs;
using MvpApi.Uwp.Views;
using MvpCompanion.UI.Common.Helpers;
using Telerik.Core.Data;
using Telerik.Data.Core;
using Telerik.UI.Xaml.Controls.Grid;
using Template10.Common;
using Template10.Mvvm;

namespace MvpApi.Uwp.ViewModels
{
    public class HomeViewModel : PageViewModelBase
    {
        #region Fields

        private IncrementalLoadingCollection<ContributionsModel> _contributions;
        private DataGridSelectionMode _gridSelectionMode = DataGridSelectionMode.Single;
        private bool _isMultipleSelectionEnabled;
        private bool _areAppBarButtonsEnabled;
        private bool _isInternetDisabled;
        private bool _isLoadingMoreItems;
        private string _loadingMoreItemsMessage;
        private string _preferredAwardDataCycle;

        private int? _currentOffset = 0;
        private string _displayTotal = "0 Items";

        #endregion

        public HomeViewModel()
        {
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

            if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
            {
                var designItems = DesignTimeHelpers.GenerateContributions();

                Contributions = new IncrementalLoadingCollection<ContributionsModel>(null);

                foreach (var contribution in designItems)
                {
                    Contributions.Add(contribution);
                }

                PreferredAwardDataDataCycle = AwardDataCycles[0];
            }
        }

        #region Properties
        
        public IncrementalLoadingCollection<ContributionsModel> Contributions
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

        public bool IsLoadingMoreItems
        {
            get => _isLoadingMoreItems;
            set => Set(ref _isLoadingMoreItems, value);
        }

        public string LoadingMoreItemsMessage
        {
            get => _loadingMoreItemsMessage;
            set => Set(ref _loadingMoreItemsMessage, value);
        }

        public string DisplayTotal
        {
            get => _displayTotal;
            set => Set(ref _displayTotal, value);
        }

        public List<string> AwardDataCycles => new List<string> { "All", "Current", "Historical" };

        public string PreferredAwardDataDataCycle
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(nameof(PreferredAwardDataDataCycle), out var rawValue))
                {
                    _preferredAwardDataCycle = (string)rawValue;
                }
                else
                {
                    _preferredAwardDataCycle = "Current";
                    ApplicationData.Current.LocalSettings.Values[nameof(PreferredAwardDataDataCycle)] = _preferredAwardDataCycle;
                }

                return _preferredAwardDataCycle;
            }
            set
            {
                Debug.WriteLine($"PreferredAwardDataDataCycle SET: Before ${_preferredAwardDataCycle}");

                if (_preferredAwardDataCycle == value)
                    return;

                _preferredAwardDataCycle = value;

                ApplicationData.Current.LocalSettings.Values[nameof(PreferredAwardDataDataCycle)] = _preferredAwardDataCycle;

                RaisePropertyChanged(nameof(PreferredAwardDataDataCycle));

                FlyoutView?.CloseFlyout();

                Contributions = new IncrementalLoadingCollection<ContributionsModel>(LoadMoreItems);
            }
        }

        public IFlyoutView FlyoutView { get; set; }

        public IScrollableView ScrollableView { get; set; }

        #endregion

        #region Event Handlers

        public async void AddActivityButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShellPage.Instance.DataContext is ShellViewModel vm && vm.UseBetaEditor)
            {
                var editDialog = new ContributionEditorDialog();

                await editDialog.ShowAsync();

                if (editDialog.ContributionResult != null)
                {
                    Debug.WriteLine($"Created {editDialog.ContributionResult.ContributionTypeName}");
                }
            }
            else
            {
                await BootStrapper.Current.NavigationService.NavigateAsync(typeof(AddContributionsPage), null, new SuppressNavigationTransitionInfo());
            }
        }

        public void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedContributions.Any())
                SelectedContributions.Clear();
        }

        public void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedContributions.Any())
                SelectedContributions.Clear();
            
            Contributions = new IncrementalLoadingCollection<ContributionsModel>(LoadMoreItems);
        }

        public async void DeleteSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsBusy = true;
                IsBusyMessage = "preparing to delete contributions...";
                
                int? indexToReturnTo = null;

                foreach (ContributionsModel contribution in SelectedContributions)
                {
                    // Try to grab an index in the overall list so that we can scroll back to it after deletion
                    if (indexToReturnTo == null)
                    {
                        indexToReturnTo = Contributions.IndexOf(contribution);
                    }

                    IsBusyMessage = $"deleting {contribution.Title}...";

                    var success = await App.ApiService.DeleteContributionAsync(contribution);

                    // Quality assurance, only logs a successful or failed delete.
                    if (ApiInformation.IsTypePresent("Microsoft.Services.Store.Engagement.StoreServicesCustomEventLogger"))
                        StoreServicesCustomEventLogger.GetDefault().Log(success == true ? "DeleteContributionSuccess" : "DeleteContributionFailure");
                }
                
                SelectedContributions.Clear();

                // After deleting contributions, we need to fetch updated list
                IsBusyMessage = "refreshing contributions...";
                
                // TODO - IMPORTANT: decide if we need a full refresh or if this custom refresh with scrolling position works
                await RefreshAndReturnToPositionAsync(Convert.ToUInt32(indexToReturnTo));
                // or start them at the beginning because we cant programmatically get the right number of items in one fetch
                //Contributions = new IncrementalLoadingCollection<ContributionsModel>(LoadMoreItems);
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
                if(ShellPage.Instance.DataContext is ShellViewModel vm && vm.UseBetaEditor)
                {
                    var editDialog = new ContributionEditorDialog(contribution);

                    await editDialog.ShowAsync();

                    if (editDialog.ContributionResult != null)
                    {
                        Debug.WriteLine($"Created {editDialog.ContributionResult.ContributionTypeName}");
                    }
                }
                else
                {
                    await BootStrapper.Current.NavigationService.NavigateAsync(typeof(ContributionDetailPage), contribution, new SuppressNavigationTransitionInfo());
                }
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

            var jsonData = await App.ApiService.ExportAllContributionsAsync();

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

        private async Task<IEnumerable<ContributionsModel>> LoadMoreItems(uint count)
        {
            try
            {
                // Here we use a different flag when the view model is busy loading items because we don't want to cover the UI
                // The IsBusy flag is used for when deleting items, when we want to block the UI
                IsLoadingMoreItems = true;
                LoadingMoreItemsMessage = $"fetching the next {count} items...";

                if (_currentOffset == null)
                {
                    _currentOffset = 0;
                }
                
                ContributionViewModel fetchResult;

                switch (PreferredAwardDataDataCycle)
                {
                    case "All":
                        fetchResult = await App.ApiService.GetContributionsAsync(_currentOffset, (int)count);
                        break;
                    case "Historical":
                        fetchResult = await App.ApiService.GetHistoricalContributionsAsync(_currentOffset, (int)count);
                        break;
                    case "Current":
                    default:
                        fetchResult = await App.ApiService.GetCurrentCycleContributionsAsync(_currentOffset, (int)count);
                        break;
                }
                
                Debug.WriteLine($"** LoadMoreItems **\nPagingIndex: {fetchResult.PagingIndex}, Count: {fetchResult.Contributions.Count}, TotalContributions: {fetchResult.TotalContributions}");

                // Current offset is the number of items we've already fetched
                _currentOffset = fetchResult.PagingIndex;

                DisplayTotal = $"{fetchResult.PagingIndex} of {fetchResult.TotalContributions} items";

                // If we've received all the contributions, return null to stop automatic loading because we've retrived all the available items
                if (fetchResult.PagingIndex + fetchResult.Contributions.Count == fetchResult.TotalContributions)
                {
                    return null;
                }

                return fetchResult.Contributions;
            }
            catch (Exception ex)
            {
                // Only log this exception after the user is logged in, unauthorized users will get an error.
                if (ShellPage.Instance.DataContext is ShellViewModel shellVm && shellVm.IsLoggedIn)
                {
                    await ex.LogExceptionAsync();
                    Debug.WriteLine($"LoadMoreItems Exception: {ex}");
                }

                return null;
            }
            finally
            {
                IsLoadingMoreItems = false;
                LoadingMoreItemsMessage = "";
            }
        }

        private async Task RefreshAndReturnToPositionAsync(uint rowIndexToReturnTo)
        {
            Contributions = new IncrementalLoadingCollection<ContributionsModel>(LoadMoreItems);

            if (Contributions.Count < rowIndexToReturnTo)
            {
                await Contributions.LoadMoreItemsAsync(rowIndexToReturnTo);
            }
            
            ScrollableView.ScrollTo(rowIndexToReturnTo);
        }

        //private async Task LoadContributionsAsync()
        //{
        //    try
        //    {
        //        IsBusy = true;
        //        IsBusyMessage = $"Fetching {PreferredAwardDataDataCycle} award cycle contributions...";
                
        //        ContributionViewModel result;

        //        switch (PreferredAwardDataDataCycle)
        //        {
        //            case "All":
        //                // Get the entire list of contributions for the user (current and historical)
        //                //result = await App.ApiService.GetAllContributionsAsync();

        //                result = await App.ApiService.GetAllContributionsAsync();
        //                break;
        //            case "Historical":
        //                // Get only the historical contributions from previous cycles
        //                result = await App.ApiService.GetAllHistoricalContributionsAsync();
        //                break;
        //            case "Current":
        //            default:
        //                // Get only the current cycle's contributions
        //                result = await App.ApiService.GetAllCurrentCycleContributionsAsync();
        //                break;
        //        }

        //        Contributions = new ObservableCollection<ContributionsModel>();

        //        foreach (var cont in result.Contributions)
        //        {
        //            Contributions.Add(cont);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await ex.LogExceptionWithUserMessage();
        //    }
        //    finally
        //    {
        //        IsBusyMessage = "";
        //        IsBusy = false;
        //    }
        //}

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

            if (ShellPage.Instance.DataContext is ShellViewModel shellVm)
            {
                if (!shellVm.IsLoggedIn)
                {
                    await ShellPage.Instance.SignInAsync();
                }

                // Although user should be logged in at this point, still check
                // TODO Use NeedsHomePageRefresh property to determine to reload the contributions
                if (shellVm.IsLoggedIn)
                {
                    Contributions = new IncrementalLoadingCollection<ContributionsModel>(LoadMoreItems);
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
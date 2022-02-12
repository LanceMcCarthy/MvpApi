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
using MvpApi.Common.Extensions;

namespace MvpApi.Uwp.ViewModels
{
    public class HomeViewModel : PageViewModelBase
    {
        #region Fields

        private IncrementalLoadingCollection<ContributionsModel> _contributions;
        //private DataGridSelectionMode _gridSelectionMode = DataGridSelectionMode.Single;
        //private bool _isMultipleSelectionEnabled;
        private bool _areAppBarButtonsEnabled;
        private bool _isInternetDisabled;
        private bool _isLoadingMoreItems;
        private string _loadingMoreItemsMessage;

        private int? _currentOffset = 0;
        private string _displayTotal = "0 Items";
        private bool _skipAutomaticRefresh;

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
                var sampleDataTask = Task.FromResult<IEnumerable<ContributionsModel>>(DesignTimeHelpers.GenerateContributions());

                Contributions = new IncrementalLoadingCollection<ContributionsModel>((c)=>sampleDataTask) { BatchSize = 0 };
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

        //public bool IsMultipleSelectionEnabled
        //{
        //    get => _isMultipleSelectionEnabled;
        //    set
        //    {
        //        Set(ref _isMultipleSelectionEnabled, value);

        //        GridSelectionMode = value
        //            ? DataGridSelectionMode.Multiple
        //            : DataGridSelectionMode.Single;
        //    }
        //}

        //public DataGridSelectionMode GridSelectionMode
        //{
        //    get => _gridSelectionMode;
        //    set => Set(ref _gridSelectionMode, value);
        //}

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

        public bool SkipAutomaticRefresh
        {
            get => _skipAutomaticRefresh;
            set => Set(ref _skipAutomaticRefresh, value);
        }

        #endregion

        #region Interfaces

        public IFlyoutView FlyoutView { get; set; }

        public IScrollableView ScrollableView { get; set; }

        public IExpandableItemView ExpandableItemView { get; set; }

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
            
            Contributions = new IncrementalLoadingCollection<ContributionsModel>(LoadMoreItems) { BatchSize = 50 };
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
                    if (contribution.IsEditable == true)
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
                }
                
                var md = new MessageDialog(
                    "The contribution(s) was successfully deleted, do you want to refresh your local data?\r\n\n" +
                    "Skipping refresh will keep your position in the list, but your fetch numbers will be inaccurate until you do a refresh.",
                    "Success - Refresh Data?");

                md.Commands.Add(new UICommand("Yes, refresh data now"));
                md.Commands.Add(new UICommand("No, I'll refresh later"));
                md.DefaultCommandIndex = 0;

                var mdResult = await md.ShowAsync();

                if (mdResult.Label == "No, I'll refresh later")
                {
                    var itemsToRemove = SelectedContributions.ToArray();

                    foreach (var item in itemsToRemove)
                    {
                        Contributions.Remove((ContributionsModel)item);
                    }
                    
                    return;
                }
                else
                {
                    // After deleting contributions, we need to fetch updated list
                    IsBusyMessage = "refreshing contributions...";

                    // TODO - IMPORTANT: decide if we need a full refresh or if this custom refresh with scrolling position works
                    await RefreshAndReturnToPositionAsync(Convert.ToUInt32(indexToReturnTo));

                    // or start them at the beginning because we cant programmatically get the right number of items in one fetch
                    //Contributions = new IncrementalLoadingCollection<ContributionsModel>(LoadMoreItems);
                }
                
                SelectedContributions.Clear();
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

        //public async void RadDataGrid_OnSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        //{
        //    // When in multiple selection mode, enable/disable delete instead of navigating to details page
        //    if (GridSelectionMode == DataGridSelectionMode.Multiple)
        //    {
        //        AreAppBarButtonsEnabled = e?.AddedItems.Any() == true;
        //        return;
        //    }

        //    // When in single selection mode, go to the selected item's details page
        //    if (GridSelectionMode == DataGridSelectionMode.Single && e?.AddedItems?.FirstOrDefault() is ContributionsModel contribution)
        //    {
        //        if (contribution.IsEditable == false)
        //        {
        //            await new MessageDialog("This contribution is marked as non-editable by Microsoft. If you feel this is an error, contact your CPM.", "Readonly Contribution").ShowAsync();
        //            return;
        //        }

        //        if(ShellPage.Instance.DataContext is ShellViewModel vm && vm.UseBetaEditor)
        //        {
        //            var editDialog = new ContributionEditorDialog(contribution, false);

        //            await editDialog.ShowAsync();

        //            if (editDialog.ContributionResult != null)
        //            {
        //                Debug.WriteLine($"Created {editDialog.ContributionResult.ContributionTypeName}");
        //            }
        //        }
        //        else
        //        {
        //            await BootStrapper.Current.NavigationService.NavigateAsync(typeof(ContributionDetailPage), contribution, new SuppressNavigationTransitionInfo());
        //        }
        //    }
        //}

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
        
        [Deprecated("This will be removed in a future update.", DeprecationType.Deprecate, 1)]
        public async void ExportButton_OnClick(object sender, RoutedEventArgs e)
        {
            await new MessageDialog("The new Export feature is located on the Settings page.\r\n\n" +
                                    "You can now choose which award cycle to export your data from: Current, Historical or All.", 
                "Export has Moved").ShowAsync();
        }

        public async void CloneButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ContributionsModel originalContribution)
            {
                var editDialog = new ContributionEditorDialog(originalContribution, true);

                await editDialog.ShowAsync();

                if (editDialog.ContributionResult != null)
                {
                    Debug.WriteLine($"Cloned item uploaded {editDialog.ContributionResult.ContributionTypeName}");

                    var refreshData = await AskForDataRefreshAsync();

                    if (refreshData)
                    {
                        IsBusyMessage = "refreshing contributions...";

                        var itemIndex = Contributions.IndexOf(originalContribution);

                        await RefreshAndReturnToPositionAsync(Convert.ToUInt32(itemIndex));
                    }
                    else
                    {
                        Contributions.Insert(0, editDialog.ContributionResult);
                    }
                }
            }
        }

        public async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ContributionsModel contribution)
            {
                if (contribution.IsEditable == false)
                {
                    await new MessageDialog("This contribution is marked as non-editable by Microsoft. If you feel this is an error, contact your CPM.", "Readonly Contribution").ShowAsync();
                    return;
                }

                var editDialog = new ContributionEditorDialog(contribution, false);

                await editDialog.ShowAsync();

                if (editDialog.ContributionResult != null)
                {
                    var itemIndex = Contributions.IndexOf(contribution);

                    Debug.WriteLine($"Edited {editDialog.ContributionResult.ContributionTypeName}");

                    if (SkipAutomaticRefresh)
                    {
                        Contributions.ReplaceContribution(itemIndex, editDialog.ContributionResult);
                        return;
                    }

                    // If the user didn't mute

                    var refreshData = await AskForDataRefreshAsync();

                    if (refreshData)
                    {
                        IsBusyMessage = "refreshing contributions...";

                        await RefreshAndReturnToPositionAsync(Convert.ToUInt32(itemIndex));
                    }
                    else
                    {
                        Contributions.ReplaceContribution(itemIndex, editDialog.ContributionResult);
                    }
                }
            }
        }

        public async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ContributionsModel contribution)
            {
                if (contribution.IsEditable != true)
                    return;

                var md = new MessageDialog(
                    "Are you sure you want to delete this? there is no way to recover it.",
                    "Delete?");

                md.Commands.Add(new UICommand("DELETE"));
                md.Commands.Add(new UICommand("cancel"));
                md.DefaultCommandIndex = 1;

                var mdResult = await md.ShowAsync();

                if (mdResult.Label != "DELETE")
                    return;

                try
                {
                    IsBusy = false;
                    IsBusyMessage = $"deleting {contribution.Title}...";

                    var indexToReturnTo = Contributions.IndexOf(contribution);

                    var success = await App.ApiService.DeleteContributionAsync(contribution);

                    if (success == true)
                    {
                        // IF the user has already seen the popup and selected Skip All
                        if (SkipAutomaticRefresh)
                        {
                            Contributions.Remove(contribution);
                            return;
                        }

                        // Ask for local data refresh 
                        var refreshData = await AskForDataRefreshAsync();

                        if (refreshData)
                        {
                            IsBusyMessage = "refreshing contributions...";

                            // TODO - IMPORTANT: decide if we need a full refresh or if this custom refresh with scrolling position works
                            await RefreshAndReturnToPositionAsync(Convert.ToUInt32(indexToReturnTo));
                        }
                        else
                        {
                            Contributions.Remove(contribution);
                        }
                    }
                    else
                    {
                        await new MessageDialog("The API refused the request to delete this item. It may be readonly or might not be allowed via API.\r\n\n" +
                            "Try deleting it on the MVP website if this continues to happen."
                            , "Not Deleted").ShowAsync();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Delete Contribution Exception: {ex}");
                }
                finally
                {
                    IsBusy = false;
                    IsBusyMessage = "";
                }
            }
        }

        public void ExpandButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ContributionsModel contribution)
            {
                ExpandableItemView.ToggleRowDetail(contribution);
            }
        }

        #endregion

        #region Methods

        private async Task<IEnumerable<ContributionsModel>> LoadMoreItems(uint count)
        {
            try
            {
                // Here we use a different flag for busy status so we don't block the entire UI
                IsLoadingMoreItems = true;
                LoadingMoreItemsMessage = $"Loading {count} items from the server...";
                
                if (_currentOffset == null)
                {
                    _currentOffset = 0;
                }

                var fetchResult = await App.ApiService.GetContributionsAsync(_currentOffset, (int)count);

                Debug.WriteLine($"** LoadMoreItems **\nPagingIndex: {fetchResult.PagingIndex}, Count: {fetchResult.Contributions.Count}, TotalContributions: {fetchResult.TotalContributions}");

                // Current offset is the number of items we've already fetched
                _currentOffset = fetchResult.PagingIndex;

                DisplayTotal = $"{fetchResult.PagingIndex} of {fetchResult.TotalContributions} items";


                // If we've received all the contributions, return null to stop automatic loading because we've retrieved all the available items
                // To check if we have all the items downloaded, simply add the pagingIndex (the total downloaded) to the current result's total
                // // Then see if it equal to (or greater than) the total amount
                if (fetchResult.PagingIndex + fetchResult.Contributions.Count >= fetchResult.TotalContributions)
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

        public async Task RefreshAndReturnToPositionAsync(uint rowIndexToReturnTo)
        {
            Contributions = new IncrementalLoadingCollection<ContributionsModel>(LoadMoreItems) { BatchSize = 50 };

            if (Contributions.Count < rowIndexToReturnTo)
            {
                await Contributions.LoadMoreItemsAsync(rowIndexToReturnTo);
            }
            
            ScrollableView.ScrollTo(rowIndexToReturnTo);
        }

        public async Task<bool> AskForDataRefreshAsync()
        {
            var refreshDataConfirmation = new MessageDialog(
                "Operation was successful, do you want to refresh your local data?\r\n\n" +
                "- Refresh: Will pull the latest data from the API, but you might lose your scrolling position.\r\n" +
                "- Skip Once: Skip now, but ask next time.\r\n" +
                "- Skip All: Skip and don't ask again for remainder of session.\r\n\n" +
                "Note: If you skip, you'll keep the scroll position, but fetch count may be inaccurate until a refresh.",
                "Success! Refresh Data Now?");

            refreshDataConfirmation.Commands.Add(new UICommand("Refresh"));
            refreshDataConfirmation.Commands.Add(new UICommand("No (skip once)"));
            refreshDataConfirmation.Commands.Add(new UICommand("No (skip all)"));
            refreshDataConfirmation.DefaultCommandIndex = 0;

            var md2Result = await refreshDataConfirmation.ShowAsync();

            if (md2Result.Label == "No (skip all)")
            {
                SkipAutomaticRefresh = true;

                return false;
            }
            else if (md2Result.Label == "No (skip once)")
            {
                SkipAutomaticRefresh = false;

                return false;
            }
            else
            {
                return true;
            }
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
                    Contributions = new IncrementalLoadingCollection<ContributionsModel>(LoadMoreItems) { BatchSize = 50 };
                }

                if (!(ApplicationData.Current.LocalSettings.Values["HomePageTutorialShown"] is bool tutorialShown) || !tutorialShown)
                {
                    var td = new TutorialDialog
                    {
                        SettingsKey = "HomePageTutorialShown",
                        MessageTitle = "Home Page",
                        Message = "Welcome MVP! This page lists your contributions, which are automatically loaded on-demand as you scroll down.\r\n\n" +
                                  "- Group or sort the contributions by any column.\r\n" +
                                  "- Use the Options column to edit, clone or delete a contribution.\r\n" +
                                  "- Use the 'Add' button to upload new contributions (single or in bulk).\r\n" +
                                  "- Expand any row to see all the details for that contribution."
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
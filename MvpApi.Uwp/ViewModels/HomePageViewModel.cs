using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.Storage;
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
using Telerik.Core.Data;
using Telerik.Data.Core;
using Telerik.UI.Xaml.Controls.Grid;
using Template10.Common;
using Template10.Mvvm;

namespace MvpApi.Uwp.ViewModels
{
    public class HomePageViewModel : PageViewModelBase
    {
        #region Fields

        private int? _currentOffset = 0;
        private string _displayTotal;
        private DataGridSelectionMode _gridSelectionMode = DataGridSelectionMode.Single;
        private bool _isMultipleSelectionEnabled;
        private bool _isLoadingMoreItems;
        private IncrementalLoadingCollection<ContributionsModel> _contributions;
        private bool _areAppBarButtonsEnabled;
        private bool _isInternetDisabled;
        private uint _batchSize = 50;

        #endregion

        public HomePageViewModel()
        {
            Contributions = new IncrementalLoadingCollection<ContributionsModel>(LoadMoreItems) { BatchSize = SelectedBatchSize };
            
            if (DesignMode.DesignModeEnabled)
            {
                var designItems = DesignTimeHelpers.GenerateContributions();

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

        public IncrementalLoadingCollection<ContributionsModel> Contributions
        {
            get => _contributions;
            set => Set(ref _contributions, value);
        }
        
        public ObservableCollection<object> SelectedContributions { get; set; }

        public GroupDescriptorCollection GroupDescriptors { get; set; }
        
        public DelegateCommand RefreshAfterDisconnectCommand { get; }

        public string DisplayTotal
        {
            get => _displayTotal;
            set => Set(ref _displayTotal, value);
        }

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

        public bool AreAppbarButtonsEnabled
        {
            get => _areAppBarButtonsEnabled;
            set => Set(ref _areAppBarButtonsEnabled, value);
        }

        public bool IsLoadingMoreItems
        {
            get => _isLoadingMoreItems;
            set => Set(ref _isLoadingMoreItems, value);
        }

        public bool IsInternetDisabled
        {
            get => _isInternetDisabled;
            set => Set(ref _isInternetDisabled, value);
        }

        public uint SelectedBatchSize
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue("BatchSize", out object rawValue))
                {
                    _batchSize = Convert.ToUInt32(rawValue);
                }
                else
                {
                    ApplicationData.Current.LocalSettings.Values["BatchSize"] = _batchSize;
                }

                return _batchSize;
            }
            set
            {
                Set(ref _batchSize, value);

                ApplicationData.Current.LocalSettings.Values["BatchSize"] = value;

                ResetData();
            }
        }

        #endregion

        #region Event Handlers
        
        public async void AddActivityButton_Click(object sender, RoutedEventArgs e)
        {
            await BootStrapper.Current.NavigationService.NavigateAsync(typeof(AddContributionsPage));
        }

        public void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedContributions.Clear();
        }

        public void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ResetData();
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

                // After deleting contributions from the server, we want to start fresh.
                // We can do this by resetting the offset and starting over
                IsBusyMessage = "refreshing contributions...";

                _currentOffset = 0;

                Contributions = new IncrementalLoadingCollection<ContributionsModel>(LoadMoreItems) { BatchSize = 50 };

                await Contributions.LoadMoreItemsAsync(50);
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
            // When in multiple selection mode, enable/diable delete instead of navigating to details page
            if (GridSelectionMode == DataGridSelectionMode.Multiple)
            {
                AreAppbarButtonsEnabled = e?.AddedItems.Any() == true;
                return;
            }

            // when in single selectin mode, go to the selected item's details page
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

        #endregion

        #region Methods

        private async Task<IEnumerable<ContributionsModel>> LoadMoreItems(uint count)
        {
            try
            {
                // Here we use a different flag when the view model is busy loading items because we dont want to cover the UI
                // The IsBusy flag is used for when deleting items, when we want to block the UI
                IsLoadingMoreItems = true;

                var result = await App.ApiService.GetContributionsAsync(_currentOffset, (int)count);

                if (result == null)
                    return null;

                _currentOffset = result.PagingIndex;

                DisplayTotal = $"{_currentOffset} of {result.TotalContributions}";

                // If we've recieved all the contributions, return null to stop automatic loading
                if (result.PagingIndex == result.TotalContributions)
                    return null;

                return result.Contributions;
            }
            catch (Exception ex)
            {
                // Only log this exception after the user is logged in
                if (App.ShellPage?.DataContext is ShellPageViewModel shellVm && shellVm.IsLoggedIn)
                {
                    await ex.LogExceptionAsync();
                    Debug.WriteLine($"LoadMoreItems Exception: {ex}");
                }

                return null;
            }
            finally
            {
                IsLoadingMoreItems = false;
            }
        }

        private void ResetData()
        {
            if (SelectedContributions.Any())
            {
                SelectedContributions.Clear();
            }

            IsMultipleSelectionEnabled = false;
            _currentOffset = 0;

            Contributions = new IncrementalLoadingCollection<ContributionsModel>(LoadMoreItems) { BatchSize = SelectedBatchSize };
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
                    IsBusy = true;
                    IsBusyMessage = "logging in...";
                    
                    await ShellPage.Instance.SignInAsync();

                    if (shellVm.IsLoggedIn)
                    {
                        IsBusyMessage = "loading contributions...";
                        ResetData();
                    }

                    IsBusyMessage = "";
                    IsBusy = false;
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
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        #endregion
    }
}
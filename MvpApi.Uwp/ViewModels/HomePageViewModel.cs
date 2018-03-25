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

        private int? currentOffset = 0;
        private string displayTotal;
        private DataGridSelectionMode gridSelectionMode = DataGridSelectionMode.Single;
        private bool isMultipleSelectionEnabled;
        private bool isLoadingMoreItems;
        private IncrementalLoadingCollection<ContributionsModel> activities;
        private bool areAppbarButtonsEnabled;
        private bool isInternetDisabled;
        private uint batchSize = 50;

        #endregion

        public HomePageViewModel()
        {
            Activities = new IncrementalLoadingCollection<ContributionsModel>(LoadMoreItems) { BatchSize = SelectedBatchSize };

            if (DesignMode.DesignModeEnabled)
            {
                var designItems = DesignTimeHelpers.GenerateContributions();

                foreach (var contribution in designItems)
                {
                    Activities.Add(contribution);
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
                    await BootStrapper.Current.NavigationService.NavigateAsync(typeof(LoginPage));
                }
            });
        }

        private async Task<IEnumerable<ContributionsModel>> LoadMoreItems(uint count)
        {
            try
            {
                // Here we use a different flag when the view model is busy loading items because we dont want to cover the UI
                // The IsBusy flag is used for when deleting items, when we want to block the UI
                IsLoadingMoreItems = true;

                var result = await App.ApiService.GetContributionsAsync(currentOffset, (int)count);

                if (result == null)
                    return null;

                currentOffset = result.PagingIndex;

                DisplayTotal = $"{currentOffset} of {result.TotalContributions}";

                // If we've recieved all the contributions, return null to stop automatic loading
                if (result.PagingIndex == result.TotalContributions)
                    return null;

                return result.Contributions;
            }
            catch (Exception ex)
            {
                // TODO Figure out a way to skip the first exception, we dont want to log that one
                //await ex.LogExceptionAsync();
                Debug.WriteLine($"LoadMoreItems Exception: {ex}");
                return null;
            }
            finally
            {
                IsLoadingMoreItems = false;
            }
        }

        #region Properties

        public IncrementalLoadingCollection<ContributionsModel> Activities
        {
            get => activities;
            set => Set(ref activities, value);
        }

        public ObservableCollection<object> SelectedContributions { get; set; }

        public GroupDescriptorCollection GroupDescriptors { get; set; }
        
        public DelegateCommand RefreshAfterDisconnectCommand { get; }

        public string DisplayTotal
        {
            get => displayTotal;
            set => Set(ref displayTotal, value);
        }

        public bool IsMultipleSelectionEnabled
        {
            get => isMultipleSelectionEnabled;
            set
            {
                Set(ref isMultipleSelectionEnabled, value);

                GridSelectionMode = value
                    ? DataGridSelectionMode.Multiple
                    : DataGridSelectionMode.Single;
            }
        }

        public DataGridSelectionMode GridSelectionMode
        {
            get => gridSelectionMode;
            set => Set(ref gridSelectionMode, value);
        }

        public bool AreAppbarButtonsEnabled
        {
            get => areAppbarButtonsEnabled;
            set => Set(ref areAppbarButtonsEnabled, value);
        }

        public bool IsLoadingMoreItems
        {
            get => isLoadingMoreItems;
            set => Set(ref isLoadingMoreItems, value);
        }

        public bool IsInternetDisabled
        {
            get => isInternetDisabled;
            set => Set(ref isInternetDisabled, value);
        }

        public uint SelectedBatchSize
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue("BatchSize", out object rawValue))
                {
                    batchSize = Convert.ToUInt32(rawValue);
                }
                else
                {
                    ApplicationData.Current.LocalSettings.Values["BatchSize"] = batchSize;
                }

                return batchSize;
            }
            set
            {
                Set(ref batchSize, value);

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

                currentOffset = 0;

                Activities = new IncrementalLoadingCollection<ContributionsModel>(LoadMoreItems) { BatchSize = 50 };

                await Activities.LoadMoreItemsAsync(50);
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
        
        private void ResetData()
        {
            if (SelectedContributions.Any())
            {
                SelectedContributions.Clear();
                GridSelectionMode = DataGridSelectionMode.Single;
            }

            currentOffset = 0;

            Activities = new IncrementalLoadingCollection<ContributionsModel>(LoadMoreItems) { BatchSize = SelectedBatchSize };
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

            if (App.ShellPage.DataContext is ShellPageViewModel shellVm && shellVm.IsLoggedIn)
            {
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
            else
            {
                await BootStrapper.Current.NavigationService.NavigateAsync(typeof(LoginPage));
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        #endregion
    }
}
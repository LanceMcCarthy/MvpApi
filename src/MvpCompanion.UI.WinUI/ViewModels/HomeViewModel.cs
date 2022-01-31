using CommonHelpers.Mvvm;
using Microsoft.UI.Xaml;
using MvpApi.Common.Models;
using MvpCompanion.UI.WinUI.Common;
using MvpCompanion.UI.WinUI.Dialogs;
using MvpCompanion.UI.WinUI.Helpers;
using MvpCompanion.UI.WinUI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Telerik.Core.Data;
using Telerik.Data.Core;
using Telerik.UI.Xaml.Controls.Grid;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Microsoft.UI.Xaml.Controls;
using Telerik.Core;

//using Microsoft.AppCenter.Analytics;

namespace MvpCompanion.UI.WinUI.ViewModels;

public class HomeViewModel : TabViewModelBase
{
    #region Fields

    private DataGridSelectionMode gridSelectionMode = DataGridSelectionMode.Single;
    private bool isMultipleSelectionEnabled;
    private IncrementalLoadingCollection<ContributionsModel> contributions;
    private bool areAppBarButtonsEnabled;
    private bool isInternetDisabled;

    // LoD features
    private int currentItemsOffset;
    private string displayTotal;

    #endregion

    public HomeViewModel()
    {
        //if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
        //{
        //    var designItems = DesignTimeHelpers.GenerateContributions();

        //    Contributions = new IncrementalLoadingCollection<ContributionsModel>(LoadMoreItems) { BatchSize = 50 };

        //    foreach (var contribution in designItems)
        //    {
        //        Contributions.Add(contribution);
        //    }
        //}

        //RefreshAfterDisconnectCommand = new DelegateCommand(async () =>
        //{
        //    IsInternetDisabled = !NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable;

        //    if (IsInternetDisabled)
        //    {
        //        //await new MessageDialog("Internet is still not available, please check your connection and try again.", "No Internet").ShowAsync();
        //        await App.ShowMessageAsync("Internet is still not available, please check your connection and try again.", "No Internet");
        //    }
        //    else
        //    {

        //    }
        //});

        GroupingOptionCommand = new DelegateCommand<string>(SelectGrouping);
    }

    #region Properties

    public IncrementalLoadingCollection<ContributionsModel> Contributions
    {
        get => contributions;
        set => SetProperty(ref contributions, value);
    }

    public BindableCollection<object> SelectedContributions { get; set; }

    public GroupDescriptorCollection GroupDescriptors { get; set; }

    public DelegateCommand RefreshAfterDisconnectCommand { get; }

    public DelegateCommand<string> GroupingOptionCommand { get; }

    public bool IsMultipleSelectionEnabled
    {
        get => isMultipleSelectionEnabled;
        set => SetProperty(ref isMultipleSelectionEnabled, value, onChanged: IsMultipleSelectionEnabledChanged);
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

    public string DisplayTotal
    {
        get => displayTotal;
        set => SetProperty(ref displayTotal, value);
    }

    #endregion

    #region Methods

    private async Task<IEnumerable<ContributionsModel>> LoadMoreItems(uint count)
    {
        try
        {
            // Here we use a different flag when the view model is busy loading items because we don't want to cover the UI
            // The IsBusy flag is used for when deleting items, when we want to block the UI
            IsBusy = true;
            IsBusyMessage = "[Load on Demand] fetching items from API...";

            var result = await App.ApiService.GetContributionsAsync(currentItemsOffset, (int)count);

            Debug.WriteLine($"** LoadMoreItems ** PagingIndex: {result.PagingIndex}, Count: {result.Contributions.Count}, TotalContributions: {result.TotalContributions}");

            currentItemsOffset = result.PagingIndex ?? 0;

            DisplayTotal = $"{currentItemsOffset} of {result.TotalContributions}";

            // Compare the PagingIndex (how many items have been delivered) with the TotalContributions
            return result.PagingIndex == result.TotalContributions 
                ? null // return null to turn off load on demand
                : result.Contributions; // return the current set of items
        }
        catch (Exception ex)
        {
            // Only log this exception after the user is logged in
            if (App.ApiService.IsLoggedIn)
            {
                await ex.LogExceptionAsync();
                Debug.WriteLine($"LoadMoreItems Exception: {ex}");
            }

            return null;
        }
        finally
        {
            IsBusy = false;
            IsBusyMessage = $"";
        }
    }

    //private async Task LoadContributionsAsync()
    //{
    //    try
    //    {
    //        IsBusy = true;
    //        IsBusyMessage = "loading contributions...";
            
    //        // Get all the contributions for the currently signed in MVP.
    //        var result = await App.ApiService.GetAllContributionsAsync();

    //        Contributions.Clear();

    //        foreach (var cont in result.Contributions)
    //        {
    //            Contributions.Add(cont);
    //        }
            
    //        IsBusyMessage = "";
    //        IsBusy = false;
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

    private void ClearSelections()
    {
        if (SelectedContributions != null && SelectedContributions.Count > 0)
        {
            SelectedContributions.Clear();
        }
    }

    private void SelectGrouping(string groupBy)
    {
        if (GroupDescriptors == null)
            return;

        GroupDescriptors.Clear();

        switch (groupBy)
        {
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
            case "None":
            default:
                // do nothing because we've already cleared the GroupDescriptors
                break;
        }
    }

    private void IsMultipleSelectionEnabledChanged()
    {
        GridSelectionMode = IsMultipleSelectionEnabled
            ? DataGridSelectionMode.Multiple
            : DataGridSelectionMode.None;
    }

    #endregion

    #region Event Handlers

    public async void AddActivityButton_Click(object sender, RoutedEventArgs e)
    {
        //if (ShellView.Instance.DataContext is ShellViewModel vm && vm.UseBetaEditor)
        //{
        //    //var editDialog = new ContributionEditorDialog();

        //    //await editDialog.ShowAsync();

        //    //if (editDialog.ContributionResult != null)
        //    //{
        //    //    Debug.WriteLine($"Created {editDialog.ContributionResult.ContributionTypeName}");
        //    //}
        //}
        //else
        //{
        //    // TODO navigation
        //    //await BootStrapper.Current.NavigationService.NavigateAsync(typeof(AddContributionsPage), null, new SuppressNavigationTransitionInfo());
        //}
    }

    public void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
    {
        ClearSelections();
    }

    public async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        ClearSelections();
        
        currentItemsOffset = 0;
        Contributions = new IncrementalLoadingCollection<ContributionsModel>(LoadMoreItems) { BatchSize = 50 };
        await Contributions.LoadMoreItemsAsync(50);
    }

    public async void DeleteSelectionButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            IsBusy = true;
            IsBusyMessage = "preparing to delete selected contributions...";

            var successfullyDeletedContributions = new List<ContributionsModel>();

            foreach (ContributionsModel contribution in SelectedContributions)
            {
                try
                {
                    IsBusyMessage = $"deleting {contribution.Title}...";

                    var success = await App.ApiService.DeleteContributionAsync(contribution);

                    // Here, I've switched to tracking individual items that hve been deleted 
                    // This allows to locally cache the items that need to be removed forom the collection. 
                    if (success == true)
                    {
                        successfullyDeletedContributions.Add(contribution);
                    }

                    // Quality assurance, only logs a successful or failed delete and the type of contribution.
                    //Analytics.TrackEvent(success == true ? "DeleteContribution" : "DeleteContribution Failed", new Dictionary<string, string>
                    //{
                    //    { "ContributionTypeName", contribution.ContributionTypeName }
                    //});
                }
                catch (Exception ex)
                {
                    //Analytics.TrackEvent("DeleteContribution Exception", new Dictionary<string, string>
                    //{
                    //    { "Exception", ex.Message },
                    //    { "ContributionTypeName", contribution.ContributionTypeName}
                    //});

                    Debug.WriteLine($"Delete Item Exception: {ex}");
                }
            }

            SelectedContributions.Clear();

            foreach (var contribution in successfullyDeletedContributions)
            {
                this.Contributions.Remove(contribution);
            }

            // No longer needed after keeping a local cache of successfully deleted items and removing them locally. This old approach was really slow
            // After deleting contributions, we need to fetch updated list
            //IsBusyMessage = "refreshing contributions...";
            //await LoadContributionsAsync();
        }
        catch (Exception ex)
        {
            await ex.LogExceptionAsync();
        }
        finally
        {
            IsBusy = false;
            IsBusyMessage = "";
        }
    }
    
    public void GroupingToggleButton_OnChecked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton rb
            || rb.Content == null
            || GroupDescriptors == null)
            return;
        
        GroupDescriptors.Clear();

        var groupName = rb.Content.ToString();

        switch (groupName)
        {
            default:
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
        try
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

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.CurrentWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

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

                    await App.ShowMessageAsync(message, "Export Saved");
                }
            }

            IsBusyMessage = "";
            IsBusy = false;
        }
        catch (Exception ex)
        {
            await ex.LogExceptionAsync();
            await App.ShowMessageAsync("There was an issue saving the exported data to a file. If this continues, please contact support awesome.apps@outlook.com", "Export Error");
        }
    }
    
    //public async void RadDataGrid_OnSelectionChanged(object? sender, DataGridSelectionChangedEventArgs e)
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
    //        ShellView.Instance.AddDetailTab(contribution);

    //        //if (ShellView.Instance.DataContext is ShellViewModel vm && vm.UseBetaEditor)
    //        //{
    //        //var editDialog = new ContributionEditorDialog(contribution);
    //        //await editDialog.ShowAsync();

    //        //if (editDialog.ContributionResult != null)
    //        //{
    //        //    Debug.WriteLine($"Created {editDialog.ContributionResult.ContributionTypeName}");
    //        //}
    //        //}
    //        //else
    //        //{
    //        // TODO navigation
    //        //await BootStrapper.Current.NavigationService.NavigateAsync(typeof(ContributionDetailPage), contribution, new SuppressNavigationTransitionInfo());
    //        //}
    //    }
    //}

    #endregion

    #region Navigation

    public override async Task OnLoadedAsync()
    {
        //if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
        //{
        //    await App.ShowMessageAsync("This application requires an internet connection. Please check your connection and try again.", "No Internet");
        //    return;
        //}

        // If this is true, it means the app is on first launch.
        // The signin process will fire completed event that re-calls this OnLoadedasync method
        if (App.ApiService == null)
            return;

        if (App.ApiService.IsLoggedIn == false)
        {
            await ShellView.Instance.LoginDialog.SignInAsync();
        }

        // If this is the first time the page loads, the contributions property will be null
        if (Contributions == null)
        {
            Contributions = new IncrementalLoadingCollection<ContributionsModel>(LoadMoreItems) { BatchSize = 50 };
            await Contributions.LoadMoreItemsAsync(50);
        }
        
        if (ApplicationData.Current.LocalSettings.Values["HomePageTutorialShown"] is not true)
        {
            var td = new TutorialDialog
            {
                XamlRoot = App.CurrentWindow.Content.XamlRoot,
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

        IsBusy = false;
        IsBusyMessage = "";

        await base.OnLoadedAsync();
    }

    #endregion
}
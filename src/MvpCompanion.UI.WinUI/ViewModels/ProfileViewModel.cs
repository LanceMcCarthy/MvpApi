using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Popups;
using CommonHelpers.Common;
using CommunityToolkit.WinUI.Connectivity;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MvpApi.Common.Models;
using MvpCompanion.UI.WinUI.Dialogs;
using MvpCompanion.UI.WinUI.Extensions;
using MvpCompanion.UI.WinUI.Helpers;
using MvpCompanion.UI.WinUI.Views;

namespace MvpCompanion.UI.WinUI.ViewModels;

public class ProfileViewModel : TabViewModelBase
{
    private MvpApi.Common.Models.ProfileViewModel mvp;
    private string profileImagePath;
    private ObservableCollection<OnlineIdentityViewModel> onlineIdentities;
    private ListViewSelectionMode listViewSelectionMode = ListViewSelectionMode.Single;
    private bool isMultipleSelectionEnabled;
    private bool areAppBarButtonsEnabled;
    private ObservableCollection<OnlineIdentityViewModel> selectedOnlineIdentities;

    public ProfileViewModel()
    {
        if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
        {
            Mvp = DesignTimeHelpers.GenerateSampleMvp();
            OnlineIdentities = DesignTimeHelpers.GenerateOnlineIdentities();
        }
    }

    public MvpApi.Common.Models.ProfileViewModel Mvp
    {
        get => mvp;
        set => SetProperty(ref mvp, value);
    }

    public ObservableCollection<OnlineIdentityViewModel> OnlineIdentities
    {
        get => onlineIdentities ??= new ObservableCollection<OnlineIdentityViewModel>();
        set => SetProperty(ref onlineIdentities, value);
    }

    public ObservableCollection<OnlineIdentityViewModel> SelectedOnlineIdentities
    {
        get => selectedOnlineIdentities ??= new ObservableCollection<OnlineIdentityViewModel>();
        set => SetProperty(ref selectedOnlineIdentities, value);
    }

    //public ObservableCollection<VisibilityViewModel> Visibilities { get; } = new ObservableCollection<VisibilityViewModel>();

    //public OnlineIdentityViewModel DraftOnlineIdentity { get; set; } = new OnlineIdentityViewModel();

    public string ProfileImagePath
    {
        get => profileImagePath;
        set => SetProperty(ref profileImagePath, value);
    }

    public bool IsMultipleSelectionEnabled
    {
        get => isMultipleSelectionEnabled;
        set
        {
            SetProperty(ref isMultipleSelectionEnabled, value);
                
            ListViewSelectionMode = value
                ? ListViewSelectionMode.Multiple
                : ListViewSelectionMode.Single;
        }
    }

    public ListViewSelectionMode ListViewSelectionMode
    {
        get => listViewSelectionMode;
        set => SetProperty(ref listViewSelectionMode, value);
    }

    public bool AreAppBarButtonsEnabled
    {
        get => areAppBarButtonsEnabled;
        set => SetProperty(ref areAppBarButtonsEnabled, value);
    }

    // Methods

    private async Task FetchOnlineIdentitiesAsync()
    {
        IsBusy = true;
        IsBusyMessage = "loading Online Identities...";

        var identities = await App.ApiService.GetOnlineIdentitiesAsync();

        if (identities != null & identities?.Count > 0)
        {
            OnlineIdentities.Clear();

            foreach (var onlineIdentity in identities)
            {
                OnlineIdentities.Add(onlineIdentity);
            }
        }
            
        IsBusyMessage = "";
        IsBusy = false;
    }

    public void OnlineIdentitiesListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Add any selected items to the SelectedItems collection
        if (e.AddedItems != null)
        {
            foreach (OnlineIdentityViewModel identity in e.AddedItems)
            {
                SelectedOnlineIdentities.Add(identity);
            }
        }

        // Remove any selected items from the SelectedItems collection
        if (e.RemovedItems != null)
        {
            foreach (OnlineIdentityViewModel identity in e.RemovedItems)
            {
                SelectedOnlineIdentities.Remove(identity);
            }
        }

        // Enable or Disable the ClearSelection and Delete buttons according to the selected items collection's count
        AreAppBarButtonsEnabled = SelectedOnlineIdentities.Any();
    }

    public void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
    {
        SelectedOnlineIdentities.Clear();
    }

    public async void RefreshOnlineIdentitiesButton_Click(object sender, RoutedEventArgs e)
    {
        await FetchOnlineIdentitiesAsync();
    }

    public async void ShowQuestionnaireButton_Click(object sender, RoutedEventArgs e)
    {
        await new AwardQuestionsDialog(App.ApiService).ShowAsync();
    }

    public async void DeleteOnlineIdentityButton_Click(object sender, RoutedEventArgs e)
    {
        IsBusy = true;
        IsBusyMessage = "requesting permission to delete Online Identities...";

        var md = new MessageDialog("Are you sure you want to delete this Online Identity? \r\n\nIf you want to add it again, you'll need to use the MVP portal. The MVP API doesn't allow adding new Online Identities yet.", "Confirm Delete!");

        md.Commands.Add(new UICommand("DELETE"));
        md.Commands.Add(new UICommand("cancel"));

        var result = await md.ShowAsync();

        if (result.Label == "DELETE")
        {
            // iterate over the selected items
            foreach (var onlineIdentity in SelectedOnlineIdentities)
            {
                IsBusyMessage = $"deleting {onlineIdentity.Url}...";

                // Call the API to delete the item
                await App.ApiService.DeleteOnlineIdentityAsync(onlineIdentity);
            }

            // Clear selected items
            SelectedOnlineIdentities.Clear();

            // Disable the Multiple  selection (this will also clear the LV selected items)
            IsMultipleSelectionEnabled = false;
                
            // Handle Visual State
            AreAppBarButtonsEnabled = false;

            // Refresh the list
            await FetchOnlineIdentitiesAsync();
        }

        IsBusyMessage = "";
        IsBusy = false;
    }

    public async void ExportButton_OnClick(object sender, RoutedEventArgs e)
    {
        IsBusy = true;
        IsBusyMessage = "exporting all Online Identities...";

        var jsonData = await App.ApiService.ExportOnlineIdentitiesAsync();

        if (string.IsNullOrEmpty(jsonData))
            return;

        var savePicker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = $"MVP OnlineIdentities {DateTime.Now:yyyy-dd-M--HH-mm-ss}"
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

                await new MessageDialog(message, "Export Saved").ShowAsync();
            }
        }
            
        IsBusyMessage = "";
        IsBusy = false;
    }

    // TODO API does not have API endpoint to add an identity yet.
    //public async void AddOnlineIdentityButton_Click(object sender, RoutedEventArgs e)
    //{
    //    var md = new MessageDialog("What type of online identity would you like to add?\r\n'Linked Identity' is an MSDN property (e.g. MSDN or Microsoft Community Forum), these can be used to automatically create contributions based on your activity.\r\n'Other Identity' for everything else, like social networks, GitHub and StackOverflow. ", "Add Online Identity");

    //    md.Commands.Add(new UICommand("Linked Identity"));
    //    md.Commands.Add(new UICommand("Other Identity"));

    //    var result = await md.ShowAsync();

    //    if (result.Label == "Linked")
    //    {

    //    }

    //    if (result.Label == "Other")
    //    {

    //    }
    //}

    #region Navigation

    public override async Task OnLoadedAsync()
    {
        //if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
        //{
        //    await new MessageDialog("This application requires an internet connection. Please check your connection and try again.", "No Internet").ShowAsync();
        //    return;
        //}

        if (!App.ApiService.IsLoggedIn)
        {
            IsBusy = true;
            IsBusyMessage = "signing in...";

            await ShellView.Instance.LoginDialog.SignInAsync();

            IsBusy = false;
            IsBusyMessage = "";
        }

        this.Mvp = App.ApiService.Mvp;
        this.ProfileImagePath = App.ApiService.ProfileImagePath;

        if (!OnlineIdentities.Any())
        {
            await FetchOnlineIdentitiesAsync();
        }

        //IsBusyMessage = "loading visibility options...";

        //var visibilities = await App.ApiService.GetVisibilitiesAsync();

        //visibilities.ForEach(visibility =>
        //{
        //    Visibilities.Add(visibility);
        //});

        IsBusyMessage = "";
        IsBusy = false;

        await base.OnLoadedAsync();
    }
    
    #endregion
}
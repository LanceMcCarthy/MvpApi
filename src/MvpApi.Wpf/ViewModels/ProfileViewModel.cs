using CommonHelpers.Common;
using CommonHelpers.Mvvm;
using MvpApi.Common.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Popups;
using MvpApi.Wpf.Helpers;

namespace MvpApi.Wpf.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private ObservableCollection<OnlineIdentityViewModel> _onlineIdentities;
        private SelectionMode _listViewSelectionMode = SelectionMode.Single;
        private bool _isMultipleSelectionEnabled;
        private bool _areAppBarButtonsEnabled;
        private ObservableCollection<OnlineIdentityViewModel> _selectedOnlineIdentities;

        public ProfileViewModel()
        {
            ClearSelectionCommand = new DelegateCommand(ClearSelection);
            RefreshOnlineIdentitiesCommand = new DelegateCommand(async () => await RefreshOnlineIdentitiesAsync());
            ShowQuestionnaireCommand = new DelegateCommand(ShowQuestionnaire);
            DeleteOnlineIdentityCommand = new DelegateCommand(DeleteOnlineIdentity);
            ExportOnlineIdentitiesCommand = new DelegateCommand(ExportOnlineIdentities);

            // Design time
            //App.ApiService.Mvp = DesignTimeHelpers.GenerateSampleMvp();
            //App.ApiService.ProfileImagePath = "../Images/MvpIcon.png";
            //OnlineIdentities = DesignTimeHelpers.GenerateOnlineIdentities();
        }

        public ObservableCollection<OnlineIdentityViewModel> OnlineIdentities
        {
            get => _onlineIdentities ??= new ObservableCollection<OnlineIdentityViewModel>();
            set => SetProperty(ref _onlineIdentities, value);
        }

        public ObservableCollection<OnlineIdentityViewModel> SelectedOnlineIdentities
        {
            get => _selectedOnlineIdentities ??= new ObservableCollection<OnlineIdentityViewModel>();
            set => SetProperty(ref _selectedOnlineIdentities, value);
        }

        public ObservableCollection<VisibilityViewModel> Visibilities { get; } = new ObservableCollection<VisibilityViewModel>();

        public OnlineIdentityViewModel DraftOnlineIdentity { get; set; } = new OnlineIdentityViewModel();

        public string ProfileImagePath => App.ApiService.ProfileImagePath;

        public Common.Models.ProfileViewModel Mvp => App.ApiService.Mvp;

        public bool IsMultipleSelectionEnabled
        {
            get => _isMultipleSelectionEnabled;
            set
            {
                SetProperty(ref _isMultipleSelectionEnabled, value);

                ListViewSelectionMode = value
                    ? SelectionMode.Multiple
                    : SelectionMode.Single;
            }
        }

        public SelectionMode ListViewSelectionMode
        {
            get => _listViewSelectionMode;
            set => SetProperty(ref _listViewSelectionMode, value);
        }

        public bool AreAppBarButtonsEnabled
        {
            get => _areAppBarButtonsEnabled;
            set => SetProperty(ref _areAppBarButtonsEnabled, value);
        }

        public DelegateCommand ClearSelectionCommand { get; set; }

        public DelegateCommand RefreshOnlineIdentitiesCommand { get; set; }

        public DelegateCommand ShowQuestionnaireCommand { get; set; }

        public DelegateCommand DeleteOnlineIdentityCommand { get; set; }

        public DelegateCommand ExportOnlineIdentitiesCommand { get; set; }

        // Methods

        private async Task RefreshOnlineIdentitiesAsync()
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

        public void ClearSelection()
        {
            SelectedOnlineIdentities.Clear();
        }

        public async void ShowQuestionnaire()
        {
            //await new AwardQuestionsDialog().ShowAsync();
        }

        public async void DeleteOnlineIdentity()
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
                foreach (OnlineIdentityViewModel onlineIdentity in SelectedOnlineIdentities)
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
                await RefreshOnlineIdentitiesAsync();
            }

            IsBusyMessage = "";
            IsBusy = false;
        }

        public async void ExportOnlineIdentities()
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

        public async Task OnLoadedAsync()
        {
            if (!App.ApiService.IsLoggedIn)
            {
                IsBusy = true;
                IsBusyMessage = "signing in...";

                await App.MainLoginWindow.SignInAsync();
            }

            

            await RefreshOnlineIdentitiesAsync();

            // TODO - future support for Visibilities
            //IsBusyMessage = "loading visibility options...";

            //var visibilities = await App.ApiService.GetVisibilitiesAsync();

            //visibilities.ForEach(visibility =>
            //{
            //    Visibilities.Add(visibility);
            //});

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
    }
}

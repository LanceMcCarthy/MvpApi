using MvpApi.Common.Models;
using MvpApi.Uwp.Dialogs;
using MvpApi.Uwp.Views;
using MvpCompanion.UI.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MvpApi.Uwp.ViewModels
{
    public class ProfileViewModel : PageViewModelBase
    {
        private MvpApi.Common.Models.ProfileViewModel _mvp;
        private ObservableCollection<OnlineIdentityViewModel> _onlineIdentities;
        private ObservableCollection<OnlineIdentityViewModel> _selectedOnlineIdentities;
        private ListViewSelectionMode _listViewSelectionMode = ListViewSelectionMode.Single;
        private string _profileImagePath;
        private bool _isMultipleSelectionEnabled;
        private bool _areAppBarButtonsEnabled;
        private bool _showIdentityOverlay;

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
            get => _mvp;
            set => Set(ref _mvp, value);
        }

        public ObservableCollection<OnlineIdentityViewModel> OnlineIdentities
        {
            get => _onlineIdentities ?? (_onlineIdentities = new ObservableCollection<OnlineIdentityViewModel>());
            set => Set(ref _onlineIdentities, value);
        }

        public ObservableCollection<OnlineIdentityViewModel> SelectedOnlineIdentities
        {
            get => _selectedOnlineIdentities ?? (_selectedOnlineIdentities = new ObservableCollection<OnlineIdentityViewModel>());
            set => Set(ref _selectedOnlineIdentities, value);
        }

        //public ObservableCollection<VisibilityViewModel> Visibilities { get; } = new ObservableCollection<VisibilityViewModel>();

        //private OnlineIdentityViewModel _draftOnlineIdentityViewModel;
        //public OnlineIdentityViewModel DraftOnlineIdentity
        //{
        //    get => _draftOnlineIdentityViewModel ?? (_draftOnlineIdentityViewModel = new OnlineIdentityViewModel());
        //    set => Set(ref _draftOnlineIdentityViewModel, value);
        //}

        public string ProfileImagePath
        {
            get => _profileImagePath;
            set => Set(ref _profileImagePath, value);
        }

        public bool IsMultipleSelectionEnabled
        {
            get => _isMultipleSelectionEnabled;
            set
            {
                Set(ref _isMultipleSelectionEnabled, value);

                ListViewSelectionMode = value
                    ? ListViewSelectionMode.Multiple
                    : ListViewSelectionMode.Single;
            }
        }

        public ListViewSelectionMode ListViewSelectionMode
        {
            get => _listViewSelectionMode;
            set => Set(ref _listViewSelectionMode, value);
        }

        public bool AreAppBarButtonsEnabled
        {
            get => _areAppBarButtonsEnabled;
            set => Set(ref _areAppBarButtonsEnabled, value);
        }

        //public bool ShowIdentityEditorOverlay
        //{
        //    get => _showIdentityOverlay;
        //    set => Set(ref _showIdentityOverlay, value);
        //}

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
            await RefreshOnlineIdentitiesAsync();
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
        
        // API just recently added this capability, will add shortly
        //public async void AddOnlineIdentityButton_Click(object sender, RoutedEventArgs e)
        //{
        //    var md = new MessageDialog("What type of online identity would you like to add?\r\n'Linked Identity' is an MSDN property (e.g. MSDN or Microsoft Community Forum), these can be used to automatically create contributions based on your activity.\r\n'Other Identity' for everything else, like social networks, GitHub and StackOverflow. ", "Add Online Identity");

        //    md.Commands.Add(new UICommand("Linked Identity"));
        //    md.Commands.Add(new UICommand("Other Identity"));

        //    // VALIDATION Requirements
        //    // URL Required, and SocialNetwork Required
        //    // Max lenth 490
        //    // MinLenth 0

        //    var regexPatternToValidateUrl = @"^((https?|ftp):\/\/)?(((([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&amp;'\(\)\*\+,;=]|:)*@)?(((\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]))|((([a-zA-Z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-zA-Z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-zA-Z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-zA-Z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-zA-Z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-zA-Z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)(:\d*)?)(\/((([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&amp;'\(\)\*\+,;=]|:|@)+(\/(([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&amp;'\(\)\*\+,;=]|:|@)*)*)?)?(\?((([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&amp;'\(\)\*\+,;=]|:|@)|[\uE000-\uF8FF]|\/|\?)*)?(\#((([a-zA-Z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-fA-F]{2})|[!\$&amp;'\(\)\*\+,;=]|:|@)|\/|\?)*)?$";

        //    var result = await md.ShowAsync();

        //    if (result.Label == "Linked")
        //    {

        //    }

        //    if (result.Label == "Other")
        //    {

        //    }

        //    await App.ApiService.SubmitOnlineIdentityAsync(DraftOnlineIdentity);
        //}

        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (ShellPage.Instance.DataContext is ShellViewModel shellVm)
            {
                if (!shellVm.IsLoggedIn)
                {
                    IsBusy = true;
                    IsBusyMessage = "signing in...";

                    await ShellPage.Instance.SignInAsync();
                }

                this.Mvp = shellVm.Mvp;
                this.ProfileImagePath = shellVm.ProfileImagePath;

                await RefreshOnlineIdentitiesAsync();

                //IsBusyMessage = "loading visibility options...";

                //var visibilities = await App.ApiService.GetVisibilitiesAsync();

                //visibilities.ForEach(visibility =>
                //{
                //    Visibilities.Add(visibility);
                //});

                IsBusyMessage = "";
                IsBusy = false;
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        #endregion
    }
}

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommonHelpers.Common;
using MvpApi.Common.Models;
using MvpApi.Forms.Portable.Common;
using MvpApi.Forms.Portable.Models;
using Xamarin.Forms;

namespace MvpApi.Forms.Portable.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private ProfileViewModel _mvp;
        private ObservableCollection<ContributionsModel> _contributions;
        private ObservableCollection<OnlineIdentityViewModel> _onlineIdentities;
        private ContributionsModel _selectedContribution;

        private string _profileImagePath;
        private bool _isLoggedIn;

        private bool _isDrawerOpen;
        private string _status;

        public MainPageViewModel()
        {
            GoToViewCommand = new Command(LoadView);
            ToggleDrawerCommand = new Command(ToggleDrawer);
        }

        #region MVP Profile Properties

        /// <summary>
        /// File path to locally saved MVP profile image
        /// </summary>
        public string ProfileImagePath
        {
            get => _profileImagePath;
            set
            {
                //enforcing propChanged because path will be the same, but image is different
                _profileImagePath = value;
                OnPropertyChanged();
            }
        }
        
        public ProfileViewModel Mvp
        {
            get => _mvp;
            set => SetProperty(ref _mvp, value);
        }

        public ObservableCollection<ContributionsModel> Contributions
        {
            get => _contributions ?? (_contributions = new ObservableCollection<ContributionsModel>());
            set => SetProperty(ref _contributions, value);
        }

        public ObservableCollection<OnlineIdentityViewModel> OnlineIdentities
        {
            get => _onlineIdentities ?? (_onlineIdentities = new ObservableCollection<OnlineIdentityViewModel>());
            set => SetProperty(ref _onlineIdentities, value);
        }

        public ContributionsModel SelectedContribution
        {
            get => _selectedContribution;
            set
            {
                if (SetProperty(ref _selectedContribution, value))
                {
                    LoadView(_selectedContribution == null ? ViewType.Home : ViewType.Detail);
                }
            }
        }
        
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }

        #endregion

        public bool IsDrawerOpen
        {
            get => _isDrawerOpen;
            set => SetProperty(ref _isDrawerOpen, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
        
        public Command GoToViewCommand { get; set; }

        public Command ToggleDrawerCommand { get; set; }

        public INavigationHandler NavigationHandler { private get; set; }

        public async void LoadView(object viewType)
        {
            // Work before view appears
            if ((ViewType)viewType == ViewType.Home)
            {
            }

            // Invoke View change in
            NavigationHandler.LoadView((ViewType)viewType);

            // Work after view appears
            if ((ViewType)viewType == ViewType.Home)
            {
                if (!IsBusy)
                {
                    IsBusy = true;
                }

                IsBusyMessage = "refreshing contributions...";
                
                await RefreshContributionsAsync();// TODO This is a temporary test, replace with incremental loading collection
            }

            if ((ViewType)viewType == ViewType.Profile)
            {
                if (!IsBusy)
                {
                    IsBusy = true;
                }
                
                IsBusyMessage = "loading Online Identities...";

                await RefreshOnlineIdentitiesAsync();
            }

            //Close drawer if it open
            if (IsDrawerOpen)
            {
                IsDrawerOpen = false;
            }

            if (IsBusy)
            {
                IsBusyMessage = "";
                IsBusy = false;
            }
        }

        private async Task RefreshContributionsAsync()
        {
            var contributionsResult = await App.ApiService.GetContributionsAsync(0, 30);

            if (contributionsResult != null & contributionsResult?.Contributions.Count > 0)
            {
                if (Contributions.Count > 1)
                    Contributions.Clear();

                foreach (var contribution in contributionsResult.Contributions)
                {
                    Contributions.Add(contribution);
                }
            }
        }

        private async Task RefreshOnlineIdentitiesAsync()
        {
            var identities = await App.ApiService.GetOnlineIdentitiesAsync();

            if (identities != null & identities?.Count > 0)
            {
                foreach (var onlineIdentity in identities)
                {
                    OnlineIdentities.Add(onlineIdentity);
                }
            }
        }
        
        private void ToggleDrawer()
        {
            IsDrawerOpen = !IsDrawerOpen;
        }
    }
}

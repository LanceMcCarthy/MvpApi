using System.Collections.ObjectModel;
using System.Linq;
using CommonHelpers.Common;
using MvpApi.Common.Models;
using MvpApi.Forms.Portable.Common;
using MvpApi.Forms.Portable.Models;
using MvpApi.Services.Apis;
using Telerik.XamarinForms.DataControls.ListView;
using Xamarin.Forms;

namespace MvpApi.Forms.Portable.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private ProfileViewModel _mvp;
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

        /// <summary>
        /// Currently signed in MVP profile
        /// </summary>
        public ProfileViewModel Mvp
        {
            get => _mvp;
            set => SetProperty(ref _mvp, value);
        }

        /// <summary>
        /// Denotes whether the user is currently logged in and able to make successful requests to the API
        /// </summary>
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

        public SelectionMode GridSelectionMode { get; set; }

        public ObservableCollection<ContributionsModel> Contributions { get; set; } = new ObservableCollection<ContributionsModel>();

        #region View state and navigation

        public Command GoToViewCommand { get; set; }

        public Command ToggleDrawerCommand { get; set; }
        
        public INavigationHandler NavigationHandler { private get; set; }

        public async void LoadView(object viewType)
        {
            // Work before view appears
            if ((ViewType)viewType == ViewType.Home)
            {
            }

            NavigationHandler.LoadView((ViewType)viewType);

            // Work after view appears
            if ((ViewType)viewType == ViewType.Home)
            {
                if(!Contributions.Any())
                {
                    // TODO quick test, replace with incremental loading collection
                    var result = await App.ApiService.GetContributionsAsync(0, 10);
                    foreach (var item in result.Contributions)
                    {
                        this.Contributions.Add(item);
                    }
                }
            }
        }

        private void ToggleDrawer()
        {
            IsDrawerOpen = !IsDrawerOpen;
        }

        #endregion

        
    }
}

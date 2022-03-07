using CommonHelpers.Common;
using MvpApi.Common.Interfaces;
using MvpApi.Services.Utilities;

namespace MvpCompanion.Maui.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        private MvpApi.Common.Models.ProfileViewModel _mvp;
        private string _profileImagePath;
        private bool _isLoggedIn;
        private bool isDarkMode;
        private DateTime _submissionStartDate = ServiceConstants.SubmissionStartDate;
        private DateTime _submissionDeadline = ServiceConstants.SubmissionDeadline;

        public ShellViewModel()
        {
            ChangeThemeMode = new Command<bool>(OnChangeThemeMode);
        }

        public string ProfileImagePath
        {
            get => _profileImagePath;
            set
            {
                // Manually invoke PropertyChanged to ensure image is reloaded, even if the file path is the same.
                _profileImagePath = value;
                OnPropertyChanged();
            }
        }

        public MvpApi.Common.Models.ProfileViewModel Mvp
        {
            get => _mvp;
            set => SetProperty(ref _mvp, value);
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }
        
        public bool IsDarkMode
        {
            get => isDarkMode = Preferences.Get(nameof(IsDarkMode), false);
            set => SetProperty(ref isDarkMode, value, onChanged: () =>
            {
                Preferences.Set(nameof(IsDarkMode), value);
                App.Current.UserAppTheme = value ? OSAppTheme.Dark : OSAppTheme.Light;
            });
        }

        public DateTime SubmissionStartDate
        {
            get => _submissionStartDate = Preferences.Get(nameof(SubmissionStartDate), _submissionStartDate);
            set => SetProperty(ref _submissionStartDate, value, onChanged: () => Preferences.Set(nameof(SubmissionStartDate), value));
        }

        public DateTime SubmissionDeadline
        {
            get => _submissionDeadline = Preferences.Get(nameof(SubmissionDeadline), _submissionDeadline);
            set => SetProperty(ref _submissionDeadline, value, onChanged: () => Preferences.Set(nameof(SubmissionDeadline), value));
        }

        public Command ChangeThemeMode { get; set; }

        public INavigationHandler NavigationHandler { get; set; }

        private void OnChangeThemeMode(bool dark) => IsDarkMode = dark;

        //public async void LoadView(object viewType)
        //{
            //// Pre-navigation work
            //if ((ViewType)viewType == ViewType.Home)
            //{
            //}
            //else if ((ViewType)viewType == ViewType.Upload)
            //{
            //    SelectedContribution = new ContributionsModel();
            //}

            //// Invoke View change
            //NavigationHandler.LoadView((ViewType)viewType);

            //// Post-navigation work
            //if ((ViewType)viewType == ViewType.Home)
            //{
            //    if (!IsBusy)
            //    {
            //        IsBusy = true;
            //    }

            //    IsBusyMessage = "refreshing contributions...";

            //    // TODO temporary, replace with incremental loading collection
            //    //await RefreshContributionsAsync();
            //}
            //else if ((ViewType)viewType == ViewType.Profile)
            //{
            //    if (!IsBusy)
            //    {
            //        IsBusy = true;
            //    }

            //    IsBusyMessage = "loading Online Identities...";

            //    //await RefreshOnlineIdentitiesAsync();
            //}
            
            //if (IsBusy)
            //{
            //    IsBusyMessage = "";
            //    IsBusy = false;
            //}
        //}

    }
}

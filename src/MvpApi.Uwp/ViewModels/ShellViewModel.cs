using Windows.ApplicationModel;
using Windows.Storage;
using MvpApi.Uwp.Helpers;

namespace MvpApi.Uwp.ViewModels
{
    public class ShellViewModel : PageViewModelBase
    {
        private MvpApi.Common.Models.ProfileViewModel _mvp;
        private string _profileImagePath;
        private bool _isLoggedIn;
        private bool _useBetaEditor;

        public ShellViewModel()
        {
            if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
            {
                Mvp = DesignTimeHelpers.GenerateSampleMvp();
                IsLoggedIn = true;
                ProfileImagePath = "/Images/MvpIcon.png";
            }
        }

        public string ProfileImagePath
        {
            get => _profileImagePath;
            set
            {
                _profileImagePath = value;

                // Manually invoke PropertyChanged to ensure image is reloaded, even if the file path is the same.
                RaisePropertyChanged();
            }
        }

        public MvpApi.Common.Models.ProfileViewModel Mvp
        {
            get => _mvp;
            set => Set(ref _mvp, value);
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => Set(ref _isLoggedIn, value);
        }
        
        public bool UseBetaEditor
        {
            get
            {
                if (ApplicationData.Current.RoamingSettings.Values.TryGetValue(nameof(UseBetaEditor), out object rawValue))
                {
                    _useBetaEditor = (bool)rawValue;
                }
                else
                {
                    ApplicationData.Current.RoamingSettings.Values[nameof(UseBetaEditor)] = _useBetaEditor;
                }
                
                return _useBetaEditor;
            }
            set
            {
                if (Set(ref _useBetaEditor, value))
                {
                    ApplicationData.Current.RoamingSettings.Values[nameof(UseBetaEditor)] = _useBetaEditor;
                }
            }
        }
    }
}

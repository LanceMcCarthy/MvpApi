using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.Storage;
using MvpApi.Common.Models;
using MvpApi.Uwp.Helpers;

namespace MvpApi.Uwp.ViewModels
{
    public class ShellPageViewModel : PageViewModelBase
    {
        private ProfileViewModel _mvp;
        private string _profileImagePath;
        private bool _isLoggedIn;
        private bool _needsHomePageRefresh = true;
        private bool _useBetaEditor;

        public ShellPageViewModel()
        {
            if (DesignMode.DesignModeEnabled)
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

                // The file path may be the same, but we still want the image to be reloaded whenever this is set
                RaisePropertyChanged();
            }
        }

        public ProfileViewModel Mvp
        {
            get => _mvp;
            set => Set(ref _mvp, value);
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => Set(ref _isLoggedIn, value);
        }

        public bool NeedsHomePageRefresh
        {
            get => _needsHomePageRefresh;
            set => Set(ref _needsHomePageRefresh, value);
        }
        
        public bool UseBetaEditor
        {
            get
            {
                if (ApplicationData.Current.RoamingSettings.Values.TryGetValue("UseBetaEditor", out object rawValue))
                {
                    _useBetaEditor = (bool)rawValue;
                }
                else
                {
                    ApplicationData.Current.RoamingSettings.Values["UseBetaEditor"] = _useBetaEditor;
                }
                
                return _useBetaEditor;
            }
            set
            {
                if (Set(ref _useBetaEditor, value))
                {
                    ApplicationData.Current.RoamingSettings.Values["UseBetaEditor"] = _useBetaEditor;
                }
            }
        }
    }
}

using System;
using Windows.ApplicationModel;
using Windows.Storage;
using MvpApi.Services.Utilities;
using MvpCompanion.UI.Common.Helpers;

namespace MvpApi.Uwp.ViewModels
{
    public class ShellViewModel : PageViewModelBase
    {
        private MvpApi.Common.Models.ProfileViewModel _mvp;
        private string _profileImagePath;
        private bool _isLoggedIn;
        private DateTime _submissionStartDate = ServiceConstants.SubmissionStartDate;
        private DateTime _submissionDeadline = ServiceConstants.SubmissionDeadline;

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
        
        public DateTime SubmissionStartDate
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(nameof(SubmissionStartDate), out object rawValue))
                {
                    _submissionStartDate = DateTime.Parse((string)rawValue);
                }
                else
                {
                    ApplicationData.Current.LocalSettings.Values[nameof(SubmissionStartDate)] = _submissionStartDate.ToLongDateString();
                }

                return _submissionStartDate;
            }
            set
            {
                if (Set(ref _submissionStartDate, value))
                {
                    ApplicationData.Current.LocalSettings.Values[nameof(SubmissionStartDate)] = _submissionStartDate.ToLongDateString();
                }
            }
        }

        public DateTime SubmissionDeadline
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(nameof(SubmissionDeadline), out object rawValue))
                {
                    _submissionDeadline = DateTime.Parse((string)rawValue);
                }
                else
                {
                    ApplicationData.Current.LocalSettings.Values[nameof(SubmissionDeadline)] = _submissionDeadline.ToLongDateString();
                }
                
                return _submissionDeadline;
            }
            set
            {
                if (Set(ref _submissionDeadline, value))
                {
                    ApplicationData.Current.LocalSettings.Values[nameof(SubmissionDeadline)] = _submissionDeadline.ToLongDateString();
                }
            }
        }
    }
}

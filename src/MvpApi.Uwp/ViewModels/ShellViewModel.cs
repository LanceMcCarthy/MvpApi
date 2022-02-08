using System;
using System.Collections.Generic;
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
        private bool _useBetaEditor;
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

        public bool UseBetaEditor
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(nameof(UseBetaEditor), out object rawValue))
                {
                    _useBetaEditor = (bool)rawValue;
                }
                else
                {
                    ApplicationData.Current.LocalSettings.Values[nameof(UseBetaEditor)] = _useBetaEditor;
                }

                return _useBetaEditor;
            }
            set
            {
                if (Set(ref _useBetaEditor, value))
                {
                    ApplicationData.Current.LocalSettings.Values[nameof(UseBetaEditor)] = _useBetaEditor;
                }
            }
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

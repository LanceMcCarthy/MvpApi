﻿using System;
using Windows.ApplicationModel;
using Windows.Storage;
using MvpApi.Services.Utilities;
using MvpCompanion.UI.WinUI.Helpers;
using CommonHelpers.Common;

namespace MvpCompanion.UI.WinUI.ViewModels
{
    public class ShellViewModel : ViewModelBase
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
                if (SetProperty(ref _useBetaEditor, value))
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
                if (SetProperty(ref _submissionStartDate, value))
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
                if (SetProperty(ref _submissionDeadline, value))
                {
                    ApplicationData.Current.LocalSettings.Values[nameof(SubmissionDeadline)] = _submissionDeadline.ToLongDateString();
                }
            }
        }
    }
}
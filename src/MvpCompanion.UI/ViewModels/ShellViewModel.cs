﻿using System;
using Windows.ApplicationModel;
using Windows.Storage;
using MvpApi.Services.Utilities;
using CommonHelpers.Common;
using MvpCompanion.UI.Wpf.Helpers;

namespace MvpCompanion.UI.Wpf.ViewModels
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
                if (SetProperty(ref _useBetaEditor, value))
                {
                    ApplicationData.Current.RoamingSettings.Values[nameof(UseBetaEditor)] = _useBetaEditor;
                }
            }
        }

        public DateTime SubmissionStartDate
        {
            get => _submissionStartDate;
            set => SetProperty(ref _submissionStartDate, value);
        }

        public DateTime SubmissionDeadline
        {
            get => _submissionDeadline;
            set => SetProperty(ref _submissionDeadline, value);
        }
    }
}

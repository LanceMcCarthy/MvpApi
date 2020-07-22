using System;
using System.Collections.Generic;
using System.Windows;
using Windows.Storage;
using CommonHelpers.Common;
using MvpApi.Services.Utilities;

namespace MvpApi.Wpf.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        private MvpApi.Common.Models.ProfileViewModel mvp;
        private string profileImagePath;
        private bool isLoggedIn;
        private bool useBetaEditor;
        private DateTime submissionStartDate = ServiceConstants.SubmissionStartDate;
        private DateTime submissionDeadline = ServiceConstants.SubmissionDeadline;

        public ShellViewModel()
        {
            //if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            //{
            //    Mvp = DesignTimeHelpers.GenerateSampleMvp();
            //    IsLoggedIn = true;
            //    ProfileImagePath = "/Images/MvpIcon.png";
            //}
        }

        public string ProfileImagePath
        {
            get => profileImagePath;
            set
            {
                profileImagePath = value;

                // Manually invoke PropertyChanged to ensure image is reloaded, even if the file path is the same.
                OnPropertyChanged();
            }
        }

        public MvpApi.Common.Models.ProfileViewModel Mvp
        {
            get => mvp;
            set => SetProperty(ref mvp, value);
        }

        public bool IsLoggedIn
        {
            get => isLoggedIn;
            set => SetProperty(ref isLoggedIn, value);
        }
        
        public bool UseBetaEditor
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(nameof(UseBetaEditor), out object rawValue))
                {
                    useBetaEditor = (bool)rawValue;
                }
                else
                {
                    ApplicationData.Current.LocalSettings.Values[nameof(UseBetaEditor)] = useBetaEditor;
                }
                
                return useBetaEditor;
            }
            set
            {
                if (SetProperty(ref useBetaEditor, value))
                {
                    ApplicationData.Current.LocalSettings.Values[nameof(UseBetaEditor)] = useBetaEditor;
                }
            }
        }

        public DateTime SubmissionStartDate
        {
            get => submissionStartDate;
            set => SetProperty(ref submissionStartDate, value);
        }

        public DateTime SubmissionDeadline
        {
            get => submissionDeadline;
            set => SetProperty(ref submissionDeadline, value);
        }

    }
}

using System;
using System.Collections.ObjectModel;
using CommonHelpers.Common;
using MvpApi.Services.Utilities;
using MvpApi.Wpf.Models;

namespace MvpApi.Wpf.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        private MvpApi.Common.Models.ProfileViewModel mvp;
        private string profileImagePath;
        private bool isLoggedIn;
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

            NavigationMenuItems = new ObservableCollection<NavItemModel>
            {
                new NavItemModel{Title = "Profile", Name = ViewName.Profile, IconGlyph = "&#xe801;"},
                new NavItemModel{Title = "Home", Name = ViewName.Home, IconGlyph = "&#xe022;"},
                new NavItemModel{Title = "Kudos", Name = ViewName.Kudos, IconGlyph = "&#xe301;"},
                new NavItemModel{Title = "Settings", Name = ViewName.Settings, IconGlyph = "&#xe13b;"},
                new NavItemModel{Title = "Authentication", Name = ViewName.Auth, IconGlyph = "&#xe130;"},
            };
        }

        public ObservableCollection<NavItemModel> NavigationMenuItems { get; set; }

        public string ProfileImagePath
        {
            get => profileImagePath;
            set
            {
                profileImagePath = value;

                // Always invoke PropertyChanged to ensure image is reloaded,
                // even if the file path is the same.
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

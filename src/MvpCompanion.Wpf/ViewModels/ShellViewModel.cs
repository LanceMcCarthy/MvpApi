using CommonHelpers.Common;
using MvpApi.Services.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Popups;
using CommonHelpers.Mvvm;
using MvpCompanion.Wpf.Helpers;
using MvpCompanion.Wpf.Models;

namespace MvpCompanion.Wpf.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        private DateTime submissionStartDate = ServiceConstants.SubmissionStartDate;
        private DateTime submissionDeadline = ServiceConstants.SubmissionDeadline;

        public ShellViewModel()
        {
            NavigationMenuItems = new ObservableCollection<NavItemModel>
            {
                new NavItemModel{Title = "Profile", Name = ViewName.Profile, IconGlyph = "&#xe801;"},
                new NavItemModel{Title = "Home", Name = ViewName.Home, IconGlyph = "&#xe022;"},
                new NavItemModel{Title = "Kudos", Name = ViewName.Kudos, IconGlyph = "&#xe301;"},
                new NavItemModel{Title = "Settings", Name = ViewName.Settings, IconGlyph = "&#xe13b;"},
                //new NavItemModel{Title = "Authentication", Name = ViewName.Auth, IconGlyph = "&#xe130;"},
            };
        }

        public ObservableCollection<NavItemModel> NavigationMenuItems { get; set; }

        public string ProfileImagePath => App.ApiService?.ProfileImagePath;

        public MvpApi.Common.Models.ProfileViewModel Mvp => App.ApiService?.Mvp;

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

        public DelegateCommand SignOutCommand { get; set; } = new DelegateCommand(async () => await SignOutAsync(), () => App.ApiService.IsLoggedIn);

        private static async Task SignOutAsync()
        {
            var md = new MessageDialog("Do you wish to sign out?");
            md.Commands.Add(new UICommand("Logout"));
            md.Commands.Add(new UICommand("Cancel"));

            var result = await md.ShowAsync();

            if (result.Label == "Logout")
            {
                await App.MainLoginWindow.SignOutAsync();
            }
        }

        public async Task OnLoadedAsync()
        {
            if (!NetworkHelper.Current.CheckInternetConnection())
            {
                await new MessageDialog("This application requires an internet connection. Please check your connection and restart the app.", "No Internet").ShowAsync();
            }

            if (!App.ApiService.IsLoggedIn)
            {
                IsBusy = true;
                IsBusyMessage = "signing in...";

                await App.MainLoginWindow.SignInAsync();
            }
        }
    }
}

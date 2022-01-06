using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Popups;
using MvpApi.Services.Utilities;
using MvpCompanion.UI.WinUI.Helpers;
using CommonHelpers.Common;
using CommunityToolkit.WinUI.Connectivity;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MvpCompanion.UI.WinUI.Views;

namespace MvpCompanion.UI.WinUI.ViewModels;

public class ShellViewModel : TabViewModelBase
{
    private string profileImagePath;
    private bool isLoggedIn;
    private MvpApi.Common.Models.ProfileViewModel mvp;
    private DateTime submissionStartDate = ServiceConstants.SubmissionStartDate;
    private DateTime submissionDeadline = ServiceConstants.SubmissionDeadline;

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
       get => App.ApiService != null ? App.ApiService.ProfileImagePath : "";
       private set => SetProperty(ref profileImagePath, value);
    }

    public MvpApi.Common.Models.ProfileViewModel Mvp
    {
        get => mvp;
        set => SetProperty(ref mvp, value);
    }

    public bool IsLoggedIn
    {
        get => App.ApiService != null && App.ApiService.IsLoggedIn;
        private set => SetProperty(ref isLoggedIn, value);
    }

    public DateTime SubmissionStartDate
    {
        get
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(nameof(SubmissionStartDate), out object rawValue))
            {
                submissionStartDate = DateTime.Parse((string)rawValue);
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values[nameof(SubmissionStartDate)] = submissionStartDate.ToLongDateString();
            }

            return submissionStartDate;
        }
        set
        {
            if (SetProperty(ref submissionStartDate, value))
            {
                ApplicationData.Current.LocalSettings.Values[nameof(SubmissionStartDate)] = submissionStartDate.ToLongDateString();
            }
        }
    }

    public DateTime SubmissionDeadline
    {
        get
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(nameof(SubmissionDeadline), out object rawValue))
            {
                submissionDeadline = DateTime.Parse((string)rawValue);
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values[nameof(SubmissionDeadline)] = submissionDeadline.ToLongDateString();
            }

            return submissionDeadline;
        }
        set
        {
            if (SetProperty(ref submissionDeadline, value))
            {
                ApplicationData.Current.LocalSettings.Values[nameof(SubmissionDeadline)] = submissionDeadline.ToLongDateString();
            }
        }
    }

    private bool useDarkTheme;
    public bool UseDarkTheme
    {
        get
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(nameof(UseDarkTheme), out var rawValue))
            {
                useDarkTheme = (bool)rawValue;
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values[nameof(UseDarkTheme)] = useDarkTheme;
            }

            return useDarkTheme;
        }
        set
        {
            if (SetProperty(ref useDarkTheme, value))
            {
                ApplicationData.Current.LocalSettings.Values[nameof(UseDarkTheme)] = useDarkTheme;

                // WIP
                //Application.Current.RequestedTheme = value ? ApplicationTheme.Dark : ApplicationTheme.Light;
                ((App.CurrentWindow.Content as ShellView)!).RequestedTheme = value ? ElementTheme.Dark : ElementTheme.Light;
            }
        }
    }

    public void RefreshProperties()
    {
        OnPropertyChanged(nameof(IsLoggedIn));
        OnPropertyChanged(nameof(ProfileImagePath));
    }

    public override async Task OnLoadedAsync()
    {
        //if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
        //{
        //    await new MessageDialog("This application requires an internet connection. Please check your connection and try again.", "No Internet").ShowAsync();
        //    return;
        //}

        RefreshProperties();

        await base.OnLoadedAsync();
    }
}
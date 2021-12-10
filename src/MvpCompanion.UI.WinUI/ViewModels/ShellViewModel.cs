using System;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Popups;
using MvpApi.Services.Utilities;
using MvpCompanion.UI.WinUI.Helpers;
using CommonHelpers.Common;
using CommunityToolkit.WinUI.Connectivity;

namespace MvpCompanion.UI.WinUI.ViewModels;

public class ShellViewModel : ViewModelBase
{
    private MvpApi.Common.Models.ProfileViewModel mvp;
    private DateTime submissionStartDate = ServiceConstants.SubmissionStartDate;
    private DateTime submissionDeadline = ServiceConstants.SubmissionDeadline;

    public ShellViewModel()
    {
        if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
        {
            Mvp = DesignTimeHelpers.GenerateSampleMvp();
            //IsLoggedIn = true;
            //ProfileImagePath = "/Images/MvpIcon.png";
        }
    }

    public string ProfileImagePath => App.ApiService.ProfileImagePath;

    public MvpApi.Common.Models.ProfileViewModel Mvp
    {
        get => mvp;
        set => SetProperty(ref mvp, value);
    }

    public bool IsLoggedIn => App.ApiService.IsLoggedIn;
    
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

    public void RefreshProperties()
    {
        OnPropertyChanged(nameof(IsLoggedIn));
        OnPropertyChanged(nameof(ProfileImagePath));
    }

    public async void OnLoaded()
    {
        if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
        {
            await new MessageDialog("This application requires an internet connection. Please check your connection and try again.", "No Internet").ShowAsync();
            return;
        }

        RefreshProperties();
    }

    public async void OnUnloaded()
    {

    }
}
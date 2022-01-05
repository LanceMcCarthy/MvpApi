using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using MvpCompanion.UI.WinUI.Views;
using Windows.ApplicationModel.Email;
using System.Diagnostics;
using CommunityToolkit.WinUI.Connectivity;
using MvpCompanion.UI.WinUI.Helpers;

namespace MvpCompanion.UI.WinUI.ViewModels;

public class SettingsViewModel : TabViewModelBase
{
    public SettingsViewModel()
    {
        //if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
        //{

        //}
    }

    public DateTime SubmissionStartDate
    {
        get => ((ShellViewModel)ShellView.Instance.DataContext).SubmissionStartDate;
        set => ((ShellViewModel)ShellView.Instance.DataContext).SubmissionStartDate = value;
    }

    public DateTime SubmissionDeadline
    {
        get => ((ShellViewModel)ShellView.Instance.DataContext).SubmissionDeadline;
        set => ((ShellViewModel)ShellView.Instance.DataContext).SubmissionDeadline = value;
    }

    public bool UseDarkTheme
    {
        get => ((ShellViewModel)ShellView.Instance.DataContext).UseDarkTheme;
        set => ((ShellViewModel)ShellView.Instance.DataContext).UseDarkTheme = value;
    }

    private void OpenUrl(string url)
    {
        Process.Start(url);
    }

    private async Task CreateEmailAsync()
    {
        try
        {
            IsBusy = true;
            IsBusyMessage = "opening email...";

            var email = new EmailMessage();
            email.To.Add(new EmailRecipient("awesome.apps@outlook.com", "Lancelot Software"));
            //email.Subject = $"MVP Companion {AppVersion}";
            email.Body = "[write your message here]\r\n\n";

            await EmailManager.ShowComposeNewEmailAsync(email);
        }
        catch (Exception ex)
        {
            await ex.LogExceptionAsync();
            
            await App.ShowMessageAsync($"Something went wrong trying to open your email application automatically. You can still manually send an email to awesome.apps@outlook.com. /r/n/nError: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            IsBusyMessage = "";
        }
    }

    public override async Task OnLoadedAsync()
    {
        //if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
        //{
        //    await App.ShowMessageAsync("This application requires an internet connection. Please check your connection and try again.", "No Internet");
        //    return;
        //}

        if (!App.ApiService.IsLoggedIn)
        {
            IsBusy = true;
            IsBusyMessage = "signing in...";

            await ShellView.Instance.LoginDialog.SignInAsync();

            IsBusy = false;
            IsBusyMessage = "";
        }

        await base.OnLoadedAsync();
    }
}
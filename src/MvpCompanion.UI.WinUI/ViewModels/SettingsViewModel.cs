using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Microsoft.UI.Xaml.Navigation;
using MvpCompanion.UI.WinUI.Views;
using CommonHelpers.Common;
using Windows.ApplicationModel.Email;
using System.Diagnostics;
using Windows.UI.Popups;

namespace MvpCompanion.UI.WinUI.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    public SettingsViewModel()
    {
        if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
        {
            UseBetaEditor = true;
        }
    }

    public bool UseBetaEditor
    {
        get => ((ShellViewModel)ShellView.Instance.DataContext).UseBetaEditor;
        set => ((ShellViewModel)ShellView.Instance.DataContext).UseBetaEditor = value;
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

    public async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
    {
        //old way
        //if (ShellView.Instance.DataContext is ShellViewModel shellVm)
        //{
        //    if (!shellVm.IsLoggedIn)
        //    {
        //        await ShellView.Instance.LoginDialog.SignInAsync();
        //    }

        //    if (IsBusy)
        //    {
        //        IsBusy = false;
        //        IsBusyMessage = "";
        //    }
        //}

        if (!App.ApiService.IsLoggedIn)
        {
            IsBusy = true;
            IsBusyMessage = "signing in...";

            await ShellView.Instance.LoginDialog.SignInAsync();

            IsBusy = false;
            IsBusyMessage = "";
        }
    }

    public async Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
    {

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
            //Crashes.TrackError(ex);

            await new MessageDialog($"Something went wrong trying to open your email application automatically. You can still manually send an email to awesome.apps@outlook.com. /r/n/nError: {ex.Message}")
                .ShowAsync();
        }
        finally
        {
            IsBusy = false;
            IsBusyMessage = "";
        }
    }
}
using CommonHelpers.Common;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.Storage;
using Windows.UI.Popups;

namespace MvpCompanion.UI.WinUI.ViewModels;

public class AboutViewModel : TabViewModelBase
{
    private readonly ApplicationDataContainer roamingSettings;
    private string appVersion;
    private Visibility feedbackHubButtonVisibility;
    private int daysToKeepErrorLogs = 5;

    public AboutViewModel()
    {
        if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
        {
            AppVersion = "1.0.1";
        }
        else
        {
            roamingSettings = ApplicationData.Current.RoamingSettings;
        }
    }

    public string AppVersion
    {
        get => appVersion;
        set => SetProperty(ref appVersion, value);
    }

    public int DaysToKeepErrorLogs
    {
        get
        {
            if (roamingSettings.Values.TryGetValue("DaysToKeepErrorLogs", out object rawValue))
            {
                daysToKeepErrorLogs = Convert.ToInt32(rawValue);
            }
            else
            {
                roamingSettings.Values["DaysToKeepErrorLogs"] = daysToKeepErrorLogs;
            }

            return daysToKeepErrorLogs;
        }
        set
        {
            SetProperty(ref daysToKeepErrorLogs, value);

            roamingSettings.Values["DaysToKeepErrorLogs"] = value;
        }
    }

    public Visibility FeedbackHubButtonVisibility
    {
        get => feedbackHubButtonVisibility;
        set => SetProperty(ref feedbackHubButtonVisibility, value);
    }

    // Methods

    public async void EmailButton_Click(object sender, RoutedEventArgs e)
    {
        await CreateEmailAsync();
    }

    public async void FeedbackButton_Click(object sender, RoutedEventArgs e)
    {
        // await StoreServicesFeedbackLauncher.GetDefault().LaunchAsync();
    }

    private async Task CreateEmailAsync()
    {
        try
        {
            IsBusy = true;
            IsBusyMessage = "opening email...";

            var email = new EmailMessage();
            email.To.Add(new EmailRecipient("awesome.apps@outlook.com", "Lancelot Software"));
            email.Subject = $"MVP Companion {AppVersion}";
            email.Body = "[write your message here]\r\n\n";

            await EmailManager.ShowComposeNewEmailAsync(email);
        }
        catch (Exception ex)
        {
            await new MessageDialog(
                    "No Default Email App", 
                    $"Something went wrong trying to open your email application automatically. You can still manually send an email to awesome.apps@outlook.com. /r/n/nError: {ex.Message}")
                .ShowAsync();
        }
        finally
        {
            IsBusy = false;
            IsBusyMessage = "";
        }
    }

    public override Task OnLoadedAsync()
    {
        if(string.IsNullOrEmpty(AppVersion))
        {
            AppVersion = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";
        }

        return base.OnLoadedAsync();
    }
}
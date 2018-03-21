using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using MvpApi.Uwp.Helpers;
using MvpApi.Uwp.Services;
using MvpApi.Uwp.Views;
using Template10.Common;
using Template10.Controls;
using Microsoft.Services.Store.Engagement;

namespace MvpApi.Uwp
{
    sealed partial class App : BootStrapper
    {
        public static ShellPage ShellPage { get; private set; }
        
        public static MvpApiService ApiService { get; set; }
        
        public App()
        {
            this.InitializeComponent();
            UnhandledException += OnUnhandledException;
        }
        
        public override UIElement CreateRootElement(IActivatedEventArgs e)
        {
            var service = NavigationServiceFactory(BackButton.Attach, ExistingContent.Exclude);

            ShellPage = new ShellPage(service);

            return new ModalDialog
            {
                DisableBackButtonWhenModal = true,
                Content = ShellPage
            };
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            var engagementManager = StoreServicesEngagementManager.GetDefault();

            await engagementManager.RegisterNotificationChannelAsync();
            
            if (args.Kind == ActivationKind.ToastNotification)
            {
                var toastArgs = args as ToastNotificationActivatedEventArgs;
                
                var originalArgs = engagementManager.ParseArgumentsAndTrackAppLaunch(toastArgs?.Argument);

                if (originalArgs != null && originalArgs.Contains("id"))
                {
                    Debug.WriteLine($"OnActivated ToastNotification argument: {originalArgs}");
                    NavigationService.Navigate(typeof(HomePage), originalArgs);
                }
                else
                {
                    NavigationService.Navigate(typeof(HomePage));
                }
            }
            else
            {
                NavigationService.Navigate(typeof(HomePage));
            }
        }

        private async void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            await e.Exception.LogExceptionWithUserMessage(
                "Sorry, there has been an unexpected error. If you'd like to send a techincal summary to the app development team, click Yes.",
                "Unexpected Error");
        }
    }
}
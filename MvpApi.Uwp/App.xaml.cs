using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using MvpApi.Uwp.Helpers;
using MvpApi.Uwp.Services;
using MvpApi.Uwp.Views;
using Template10.Common;
using Template10.Controls;

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

        public override Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // If the app was launched from a reminder, it will have the appointment ID in the args
            if (args.Kind == ActivationKind.ToastNotification)
            {
                var toastArgs = args as ToastNotificationActivatedEventArgs;
                var argument = toastArgs?.Argument;

                if (argument != null && argument.Contains("id"))
                {
                    Debug.WriteLine($"OnActivated ToastNotification argument: {argument}");

                    NavigationService.Navigate(typeof(HomePage), argument);
                }
            }
            else
            {
                NavigationService.Navigate(typeof(HomePage));
            }

            return Task.CompletedTask;
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
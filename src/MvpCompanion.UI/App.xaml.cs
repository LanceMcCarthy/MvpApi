using System;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Microsoft.UI.Xaml;
using MvpApi.Services.Apis;
using MvpCompanion.UI.Helpers;
using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel;
using MvpCompanion.UI.Views;

namespace MvpCompanion.UI
{
    sealed partial class App : Application
    {
        public static MvpApiService ApiService { get; set; }

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }


        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;


            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.UWPLaunchActivatedEventArgs.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.UWPLaunchActivatedEventArgs.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(ShellPage), e.Arguments);
                }

                Window.Current.Activate();
            }
        }

        // TODO uncomment after project reload
        //protected override void OnActivated(IActivatedEventArgs args)
        //{
        //    
        //    var engagementManager = StoreServicesEngagementManager.GetDefault();

        //    await engagementManager.RegisterNotificationChannelAsync();

        //    if (args.Kind == ActivationKind.ToastNotification)
        //    {
        //        var toastArgs = args as ToastNotificationActivatedEventArgs;

        //        var originalArgs = engagementManager.ParseArgumentsAndTrackAppLaunch(toastArgs?.Argument);

        //        if (originalArgs != null && originalArgs.Contains("id"))
        //        {
        //            Debug.WriteLine($"OnActivated ToastNotification argument: {originalArgs}");
        //        }
        //    }

        //    base.OnActivated(args);
        //}


        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }


        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}

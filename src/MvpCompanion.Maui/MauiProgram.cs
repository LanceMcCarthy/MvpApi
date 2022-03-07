using Microsoft.Maui.LifecycleEvents;
using MvpCompanion.Maui.Services;
using Telerik.Maui.Controls.Compatibility;

namespace MvpCompanion.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		builder
			.UseMauiApp<App>()
			.UseTelerik()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemiBold");
				fonts.AddFont("fa-solid-900.ttf", "FontAwesome");
				fonts.AddFont("telerikfontexamples.ttf", "telerikfontexamples");
			})
            .ConfigureLifecycleEvents(lifecycle => {
#if WINDOWS
            lifecycle
                .AddWindows(windows =>
                    windows.OnNativeMessage((app, args) => {
                        if (WindowExtensions.Hwnd == IntPtr.Zero)
                        {
                            WindowExtensions.Hwnd = args.Hwnd;
                            WindowExtensions.SetIcon("Platforms/Windows/trayicon.ico");
                        }
                        app.ExtendsContentIntoTitleBar = false;
                    }));
#endif
            });

        var services = builder.Services;
#if WINDOWS
        // On Windows, we can use the system tray
        services.AddSingleton<ITrayService, WinUI.TrayService>();
        services.AddSingleton<INotificationService, WinUI.NotificationService>();
#elif MACCATALYST
        // On Mac, we also have a tray to take advantage of
        services.AddSingleton<ITrayService, MacCatalyst.TrayService>();
        services.AddSingleton<INotificationService, MacCatalyst.NotificationService>();
#endif
        
        return builder.Build();
    }
}

﻿using Microsoft.Maui.LifecycleEvents;
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
            });

        var services = builder.Services;

#if WINDOWS10_0_17763_0_OR_GREATER
        services.AddSingleton<INotificationService, NotificationService_WinUI>();
#elif IOS
        services.AddSingleton<INotificationService, NotificationService_iOS>();
#elif ANDROID
        services.AddSingleton<INotificationService, NotificationService_Android>();
#elif MACCATALYST
        services.AddSingleton<INotificationService, NotificationService_Mac>();
#endif

        return builder.Build();
    }
}
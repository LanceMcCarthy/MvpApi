﻿using MvpCompanion.Maui.Services;
using UserNotifications;

namespace MvpCompanion.Maui.MacCatalyst;

public class NotificationService : INotificationService
{
    public void ShowNotification(string title, string body)
    {
        UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert, (approved, topError) => 
        {
            if (!approved)
                return;

            var content = new UNMutableNotificationContent()
            {
                Title = title,
                Body = body
            };

            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(0.25, false);
            var request = UNNotificationRequest.FromIdentifier(Guid.NewGuid().ToString(), content, trigger);

            UNUserNotificationCenter.Current.AddNotificationRequest(request, (innerError) => 
            {
                if (innerError != null)
                {
                    throw new System.Exception($"Failed to schedule notification: {innerError}");
                }
            });
        });
    }
}

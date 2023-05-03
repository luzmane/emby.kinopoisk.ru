using System.Collections.Generic;

using MediaBrowser.Controller.Notifications;
using MediaBrowser.Model.Notifications;

namespace EmbyKinopoiskRu.Notification
{
    public class NotificationsTypeFactory : INotificationTypeFactory
    {
        public static readonly string TokenIssueType = "KinopoiskTokenIssueNotification";

        public NotificationsTypeFactory()
        {
        }

        public IEnumerable<NotificationTypeInfo> GetNotificationTypes()
        {
            return new List<NotificationTypeInfo>
            {
                new NotificationTypeInfo
                {
                     Type = NotificationsTypeFactory.TokenIssueType,
                     Name = "Problems With a Token",
                     Category = "Kinopoisk Notification",
                     Enabled = false,
                     IsBasedOnUserEvent = false
                }
            };
        }


        /*
        INotificationManager _notificationManager;
                    await _notificationManager.SendNotification(
                new MediaBrowser.Model.Notifications.NotificationRequest()
                {
                    NotificationType = "NewMediaReportNotification",
                    Date = DateTime.UtcNow,
                    Name = "New Media Report Notification",
                    Description = "New Media Report NotificationNotificationNotificationNotificationNotificationNotification"
                }
                , cancellationToken
            );
        */
    }
}

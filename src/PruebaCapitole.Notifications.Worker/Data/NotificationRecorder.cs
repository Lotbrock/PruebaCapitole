using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace PruebaCapitole.Notifications.Worker.Data
{
    public sealed class NotificationRecorder(NotificationsDbContext db, ILogger<NotificationRecorder> logger)
    {
        public async Task<bool> RecordAsync(Notification notification, CancellationToken ct)
        {
            if (await db.Notifications.AnyAsync(n => n.EventId == notification.EventId, ct))
            {
                LogDuplicate(notification);
                return false;
            }

            db.Notifications.Add(notification);

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                LogDuplicate(notification);
                return false;
            }

            logger.LogInformation(
                "Notification sent (simulated). Type={NotificationType} BookingId={BookingId} ResourceId={ResourceId} To={CustomerEmail} EventId={EventId}",
                notification.Type, notification.BookingId, notification.ResourceId, notification.CustomerEmail, notification.EventId);

            return true;
        }

        private void LogDuplicate(Notification notification) =>
            logger.LogInformation("Duplicate event ignored. EventId={EventId} Type={NotificationType}",
                notification.EventId, notification.Type);
    }
}

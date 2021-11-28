namespace FileDB.Notifications
{
    public enum NotificationType { Info, Warning, Error };

    public record Notification(NotificationType Type, string Message);
}

using System;
using System.Collections.Generic;

namespace FileDB.Notifiers;

public enum NotificationType { Info, Warning, Error };

public record Notification(NotificationType Type, string Message, DateTime DateTime);

interface INotifier
{
    List<Notification> Run();
}

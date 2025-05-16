using System.Collections.Generic;
using FileDB.Notifications;

namespace FileDB.Notifiers;

public interface INotifier
{
    List<Notification> Run();
}

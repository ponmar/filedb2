using FileDB.Lang;
using FileDB.Notifications;
using FileDB.Notifiers;
using Xunit;

namespace FileDB.Tests.Notifiers;

public class RestInPeaceNotifierTests
{
    [Fact]
    public void Run_WhenDeceasedToday_ReturnsNotification()
    {
        var person = NotifierTestHelpers.CreatePerson("Charlie Example", new DateTime(1980, 1, 1), DateTime.Today);
        var notifier = new RestInPeaceNotifier([person]);

        var result = notifier.Run().ToList();

        var notification = Assert.Single(result);
        var rip = Assert.IsType<PersonRestInPeaceNotification>(notification);
        Assert.Equal(string.Format(Strings.RestInPeaceNotifierRestInPeace, person.FullName), rip.Message);
    }

    [Fact]
    public void Run_WhenNotDeceasedToday_ReturnsNoNotifications()
    {
        var person = NotifierTestHelpers.CreatePerson("Charlie Example", new DateTime(1980, 1, 1), new DateTime(2020, 1, 1));
        var notifier = new RestInPeaceNotifier([person]);

        var result = notifier.Run().ToList();

        Assert.Empty(result);
    }
}

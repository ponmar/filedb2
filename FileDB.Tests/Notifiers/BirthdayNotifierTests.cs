using FileDB.Lang;
using FileDB.Notifications;
using FileDB.Notifiers;
using Xunit;

namespace FileDB.Tests.Notifiers;

public class BirthdayNotifierTests
{
    [Fact]
    public void Run_ForAlivePersonWithBirthdayToday_ReturnsNotification()
    {
        var person = NotifierTestHelpers.CreatePerson("Alice Example", NotifierTestHelpers.CreateBirthdayDate());
        var notifier = new BirthdayNotifier([person], BirthdayNotificationFor.Alive);

        var result = notifier.Run().ToList();

        var notification = Assert.Single(result);
        var birthday = Assert.IsType<PersonBirthdayNotification>(notification);
        Assert.Equal(string.Format(Strings.BirthdayNotifierHappyBirthday, person.FullName), birthday.Message);
    }

    [Fact]
    public void Run_ForDeceasedPersonWithBirthdayToday_ReturnsNotification()
    {
        var person = NotifierTestHelpers.CreatePerson("Bob Example", NotifierTestHelpers.CreateBirthdayDate(), new DateTime(2020, 1, 1));
        var notifier = new BirthdayNotifier([person], BirthdayNotificationFor.Deceased);

        var result = notifier.Run().ToList();

        var notification = Assert.Single(result);
        var birthday = Assert.IsType<PersonBirthdayForDeceasedNotification>(notification);
        Assert.Equal(string.Format(Strings.BirthdayNotifierTodayIsTheBirthdayFor, person.FullName), birthday.Message);
    }

    [Fact]
    public void Run_ForDeceasedPersonWithAliveReminder_ReturnsNoNotifications()
    {
        var person = NotifierTestHelpers.CreatePerson("Bob Example", NotifierTestHelpers.CreateBirthdayDate(), new DateTime(2020, 1, 1));
        var notifier = new BirthdayNotifier([person], BirthdayNotificationFor.Alive);

        var result = notifier.Run().ToList();

        Assert.Empty(result);
    }
}

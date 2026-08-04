using FileDB.Lang;
using FileDB.Model;
using FileDB.Notifications;
using FileDB.Notifiers;
using Xunit;

namespace FileDBTests.Notifiers;

public class DemoNotifierTests
{
    [Fact]
    public void Run_WhenDemoConfigIsUsed_ReturnsNotification()
    {
        var filePaths = new ApplicationFilePaths(
            Path.GetTempPath(),
            Path.Combine(Path.GetTempPath(), "Demo.FileDB"),
            Path.Combine(Path.GetTempPath(), "filedb.db"));
        var notifier = new DemoNotifier(NotifierTestHelpers.CreateConfigProvider(filePaths));

        var result = notifier.Run().ToList();

        var notification = Assert.Single(result);
        var demo = Assert.IsType<CollectionDemoUsedNotification>(notification);
        Assert.Equal(Strings.StartupNotificationDemoConfigurationEnabled, demo.Message);
    }

    [Fact]
    public void Run_WhenNormalConfigIsUsed_ReturnsNoNotifications()
    {
        var filePaths = new ApplicationFilePaths(
            Path.GetTempPath(),
            Path.Combine(Path.GetTempPath(), "Collection.FileDB"),
            Path.Combine(Path.GetTempPath(), "filedb.db"));
        var notifier = new DemoNotifier(NotifierTestHelpers.CreateConfigProvider(filePaths));

        var result = notifier.Run().ToList();

        Assert.Empty(result);
    }
}

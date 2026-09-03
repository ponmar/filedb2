using System.IO.Abstractions.TestingHelpers;
using FileDB.Lang;
using FileDB.Notifications;
using FileDB.Notifiers;
using Xunit;

namespace FileDB.Tests.Notifiers;

public class MissingDatabaseNotifierTests
{
    [Fact]
    public void Run_WhenDatabaseIsMissing_ReturnsNotification()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), "filedb-notifier-tests", Guid.NewGuid().ToString("N"));
        var filePaths = NotifierTestHelpers.CreateFilePaths(rootDirectory);
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(rootDirectory);

        var notifier = new MissingDatabaseNotifier(filePaths, fileSystem);

        var result = notifier.Run().ToList();

        var notification = Assert.Single(result);
        var missing = Assert.IsType<DatabaseMissingNotification>(notification);
        Assert.Equal(string.Format(Strings.NotificationDatabaseIsMissing, filePaths.DatabasePath), missing.Message);
    }

    [Fact]
    public void Run_WhenDatabaseExists_ReturnsNoNotifications()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), "filedb-notifier-tests", Guid.NewGuid().ToString("N"));
        var filePaths = NotifierTestHelpers.CreateFilePaths(rootDirectory);
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(rootDirectory);
        fileSystem.AddFile(filePaths.DatabasePath, new MockFileData(""));

        var notifier = new MissingDatabaseNotifier(filePaths, fileSystem);

        var result = notifier.Run().ToList();

        Assert.Empty(result);
    }
}

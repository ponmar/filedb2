using FakeItEasy;
using FileDB.Model;
using FileDBInterface.Model;

namespace FileDBTests.Notifiers;

internal static class NotifierTestHelpers
{
    internal static PersonModel CreatePerson(string fullName, DateTime? birthDate = null, DateTime? deceasedDate = null)
    {
        return new PersonModel
        {
            Id = 1,
            ShortName = fullName,
            FullName = fullName,
            DateOfBirth = birthDate?.ToString("yyyy-MM-dd"),
            Deceased = deceasedDate?.ToString("yyyy-MM-dd"),
        };
    }

    internal static DateTime CreateBirthdayDate()
    {
        var today = DateTime.Today;
        var year = today.Month == 2 && today.Day == 29 ? 2000 : 1990;
        return new DateTime(year, today.Month, today.Day);
    }

    internal static ApplicationFilePaths CreateFilePaths(string rootDirectory)
    {
        var configPath = Path.Combine(rootDirectory, "Collection.FileDB");
        var databasePath = Path.Combine(rootDirectory, "filedb.db");
        return new ApplicationFilePaths(rootDirectory, configPath, databasePath);
    }

    internal static IConfigProvider CreateConfigProvider(ApplicationFilePaths filePaths)
    {
        var configProvider = A.Fake<IConfigProvider>();
        A.CallTo(() => configProvider.FilePaths).Returns(filePaths);
        return configProvider;
    }

    internal static string CreateBackupFilePath(string databasePath, DateTime timestamp)
    {
        var directory = Path.GetDirectoryName(databasePath)!;
        var filename = Path.GetFileNameWithoutExtension(databasePath);
        var extension = Path.GetExtension(databasePath);
        return Path.Combine(directory, $"{filename}_backup_{timestamp:yyyy-MM-ddTHHmmss}{extension}");
    }
}

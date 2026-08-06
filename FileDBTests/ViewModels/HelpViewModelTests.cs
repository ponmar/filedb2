using FakeItEasy;
using FileDB;
using FileDB.ViewModels;
using System.IO.Abstractions;
using Xunit;

namespace FileDBTests.ViewModels;

public class HelpViewModelTests
{
    private readonly IFileSystem fileSystem = A.Fake<IFileSystem>();
    private readonly IProcessUtils processUtils = A.Fake<IProcessUtils>();

    private void SetupLicenseFile()
    {
        var licensesPath = Path.Combine(AppContext.BaseDirectory, "Resources", "licenses.json");
        A.CallTo(() => fileSystem.File.ReadAllText(licensesPath)).Returns("""[{"PackageId":"pkg","PackageVersion":"1.0","PackageProjectUrl":"https://example.com","License":"MIT"}]""");
    }

    [Fact]
    public void Constructor_LoadsLicensesAndChangesText()
    {
        SetupLicenseFile();
        A.CallTo(() => fileSystem.File.Exists("CHANGES.txt")).Returns(true);
        A.CallTo(() => fileSystem.File.ReadAllText("CHANGES.txt")).Returns("changes");

        var viewModel = new HelpViewModel(fileSystem, processUtils);

        Assert.Single(viewModel.Licenses);
        Assert.Equal("changes", viewModel.Changes);
    }

    [Fact]
    public void OpenUrlCommand_UsesProcessUtils()
    {
        SetupLicenseFile();
        var viewModel = new HelpViewModel(fileSystem, processUtils);

        viewModel.OpenUrlCommand.Execute("https://example.com");

        A.CallTo(() => processUtils.OpenUriInBrowser("https://example.com")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Constructor_WithoutChangesFile_UsesFallbackText()
    {
        SetupLicenseFile();
        A.CallTo(() => fileSystem.File.Exists("CHANGES.txt")).Returns(false);

        var viewModel = new HelpViewModel(fileSystem, processUtils);

        Assert.Equal("Not deployed", viewModel.Changes);
    }
}

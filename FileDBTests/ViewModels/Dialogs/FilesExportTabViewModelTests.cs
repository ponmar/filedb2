using FakeItEasy;
using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Export.SearchResult;
using FileDB.Lang;
using FileDB.Model.FileFormats;
using FileDB.ViewModels.Dialogs;
using FileDB.ViewModels.Search;
using FileDBInterface.Model;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace FileDBTests.ViewModels.Dialogs;

public class FilesExportTabViewModelTests
{
    private readonly IDialogs dialogs = A.Fake<IDialogs>();
    private readonly ISearchResultExportDataBuilder dataBuilder = A.Fake<ISearchResultExportDataBuilder>();
    private readonly IFilesExporter filesExporter = A.Fake<IFilesExporter>();

    private FilesExportTabViewModel CreateViewModel(IFileSystem? fs = null) =>
        new(dialogs, fs ?? A.Fake<IFileSystem>(), dataBuilder, filesExporter);

    private static SearchResult MakeSearchResult() =>
        new() { Files = [new FileModel { Id = 1, Path = "a.jpg" }] };

    [Fact]
    public void ExportEnabled_DefaultState_IsFalse()
    {
        var vm = CreateViewModel();
        Assert.False(vm.ExportEnabled);
    }

    [Fact]
    public void ExportEnabled_WithDirectoryNoName_IsFalse()
    {
        var vm = CreateViewModel();
        vm.ExportName = string.Empty;
        vm.DestinationDirectory = "/export";
        Assert.False(vm.ExportEnabled);
    }

    [Fact]
    public void ExportEnabled_WithNameAndDirectory_IsTrue()
    {
        var vm = CreateViewModel();
        vm.DestinationDirectory = "/export";
        Assert.True(vm.ExportEnabled);
    }

    [Fact]
    public async Task BrowseDirectoryCommand_SetsDestinationDirectory()
    {
        A.CallTo(() => dialogs.ShowBrowseExistingDirectoryDialogAsync(A<string>._, A<string>._)).Returns("/export");
        var vm = CreateViewModel();

        await vm.BrowseDirectoryCommand.ExecuteAsync(null);

        Assert.Equal("/export", vm.DestinationDirectory);
    }

    [Fact]
    public async Task ExportToDirectoryCommand_DirectoryMissing_ShowsError()
    {
        var fakeFs = A.Fake<IFileSystem>();
        A.CallTo(() => fakeFs.Directory.Exists(A<string>._)).Returns(false);
        var vm = CreateViewModel(fakeFs);
        vm.SearchResult = MakeSearchResult();
        vm.DestinationDirectory = "/export";

        await vm.ExportToDirectoryCommand.ExecuteAsync(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExportToDirectoryCommand_NonEmptyDirectory_ShowsError()
    {
        var mockFs = new MockFileSystem();
        mockFs.AddDirectory("/export");
        mockFs.AddFile("/export/existing.txt", new MockFileData("x"));
        var vm = CreateViewModel(mockFs);
        vm.SearchResult = MakeSearchResult();
        vm.DestinationDirectory = "/export";

        await vm.ExportToDirectoryCommand.ExecuteAsync(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync(Strings.ExportDestinationDirectoryIsNotEmpty))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExportToDirectoryCommand_UserCancelsConfirm_DoesNotExport()
    {
        var mockFs = new MockFileSystem();
        mockFs.AddDirectory("/export");
        A.CallTo(() => dialogs.ShowConfirmDialogAsync(A<string>._)).Returns(false);
        var vm = CreateViewModel(mockFs);
        vm.SearchResult = MakeSearchResult();
        vm.DestinationDirectory = "/export";

        await vm.ExportToDirectoryCommand.ExecuteAsync(null);

        A.CallTo(() => dialogs.ShowProgressDialogAsync(A<System.Action<System.IProgress<string>, System.Threading.CancellationToken>>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ExportToDirectoryCommand_ValidDirectory_CallsFilesExporter()
    {
        var mockFs = new MockFileSystem();
        mockFs.AddDirectory("/export");
        A.CallTo(() => dialogs.ShowConfirmDialogAsync(A<string>._)).Returns(true);
        A.CallTo(() => dataBuilder.BuildFilesList(A<List<FileModel>>._, A<System.Threading.CancellationToken>._))
            .Returns([]);
        A.CallTo(() => dialogs.ShowProgressDialogAsync(A<Action<IProgress<string>, System.Threading.CancellationToken>>._))
            .ReturnsLazily(call =>
            {
                var work = (Action<IProgress<string>, System.Threading.CancellationToken>)call.Arguments[0]!;
                work(A.Fake<IProgress<string>>(), System.Threading.CancellationToken.None);
                return Task.CompletedTask;
            });
        var vm = CreateViewModel(mockFs);
        vm.SearchResult = MakeSearchResult();
        vm.DestinationDirectory = "/export";

        await vm.ExportToDirectoryCommand.ExecuteAsync(null);

        A.CallTo(() => filesExporter.Export(A<List<ExportedFile>>._, "/export")).MustHaveHappenedOnceExactly();
    }
}

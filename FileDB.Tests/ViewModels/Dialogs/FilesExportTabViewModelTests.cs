using FakeItEasy;
using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Export.SearchResult;
using FileDB.Lang;
using FileDB.Model.FileFormats;
using FileDB.ViewModels.Dialogs;
using FileDB.ViewModels.Search;
using FileDBInterface.Model;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace FileDB.Tests.ViewModels.Dialogs;

public class FilesExportTabViewModelTests
{
    private readonly IDialogs dialogs = A.Fake<IDialogs>();
    private readonly ISearchResultExportDataBuilder dataBuilder = A.Fake<ISearchResultExportDataBuilder>();
    private readonly IFilesExporter filesExporter = A.Fake<IFilesExporter>();
    private readonly IProcessUtils processUtils = A.Fake<IProcessUtils>();

    private FilesExportTabViewModel CreateViewModel(IFileSystem? fs = null) =>
        new(dialogs, fs ?? A.Fake<IFileSystem>(), dataBuilder, processUtils, filesExporter);

    private static SearchResult MakeSearchResult() =>
        new() { Files = [new FileModel { Id = 1, Path = "a.jpg" }] };

    [Fact]
    public async Task SelectAndExportCommand_NothingSelected_DoesNotExport()
    {
        A.CallTo(() => dialogs.ShowBrowseExistingDirectoryDialogAsync(A<string>._, A<string>._)).Returns((string?)null);
        var vm = CreateViewModel();
        vm.SearchResult = MakeSearchResult();

        await vm.SelectAndExportCommand.ExecuteAsync(null);

        A.CallTo(() => dialogs.ShowProgressDialogAsync(A<Action<IProgress<string>, System.Threading.CancellationToken>>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task SelectAndExportCommand_NonExistentDirectory_ShowsError()
    {
        A.CallTo(() => dialogs.ShowBrowseExistingDirectoryDialogAsync(A<string>._, A<string>._)).Returns("/export");
        var fakeFs = A.Fake<IFileSystem>();
        A.CallTo(() => fakeFs.Directory.Exists(A<string>._)).Returns(false);
        var vm = CreateViewModel(fakeFs);
        vm.SearchResult = MakeSearchResult();

        await vm.SelectAndExportCommand.ExecuteAsync(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync(A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SelectAndExportCommand_NonEmptyDirectory_ShowsError()
    {
        A.CallTo(() => dialogs.ShowBrowseExistingDirectoryDialogAsync(A<string>._, A<string>._)).Returns("/export");
        var mockFs = new MockFileSystem();
        mockFs.AddDirectory("/export");
        mockFs.AddFile("/export/existing.txt", new MockFileData("x"));
        var vm = CreateViewModel(mockFs);
        vm.SearchResult = MakeSearchResult();

        await vm.SelectAndExportCommand.ExecuteAsync(null);

        A.CallTo(() => dialogs.ShowErrorDialogAsync(Strings.ExportDestinationDirectoryIsNotEmpty))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SelectAndExportCommand_ValidDirectory_CallsFilesExporter()
    {
        A.CallTo(() => dialogs.ShowBrowseExistingDirectoryDialogAsync(A<string>._, A<string>._)).Returns("/export");
        var mockFs = new MockFileSystem();
        mockFs.AddDirectory("/export");
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

        await vm.SelectAndExportCommand.ExecuteAsync(null);

        A.CallTo(() => filesExporter.Export(A<List<ExportedFile>>._, "/export")).MustHaveHappenedOnceExactly();
    }
}

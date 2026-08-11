using FakeItEasy;
using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Export.SearchResult;
using FileDB.Model.FileFormats;
using FileDB.ViewModels.Dialogs;
using FileDB.ViewModels.Search;
using FileDBInterface.Model;
using System.Collections.Generic;
using Xunit;

namespace FileDBTests.ViewModels.Dialogs;

public class SelfContainedHtmlExportTabViewModelTests
{
    private readonly IDialogs dialogs = A.Fake<IDialogs>();
    private readonly ISearchResultExportDataBuilder dataBuilder = A.Fake<ISearchResultExportDataBuilder>();
    private readonly ISelfContainedHtmlExporter selfContainedHtmlExporter = A.Fake<ISelfContainedHtmlExporter>();

    private SelfContainedHtmlExportTabViewModel CreateViewModel() =>
        new(dialogs, dataBuilder, selfContainedHtmlExporter);

    private static SearchResult MakeSearchResult() =>
        new() { Files = [new FileModel { Id = 1, Path = "a.jpg" }] };

    [Fact]
    public void ExportEnabled_DefaultState_IsFalse()
    {
        var vm = CreateViewModel();
        Assert.False(vm.ExportEnabled);
    }

    [Fact]
    public void ExportEnabled_WithFileAndName_IsTrue()
    {
        var vm = CreateViewModel();
        vm.DestinationFile = "/export/out.html";
        Assert.True(vm.ExportEnabled);
    }

    [Fact]
    public void ExportEnabled_WithFileNoName_IsFalse()
    {
        var vm = CreateViewModel();
        vm.ExportName = string.Empty;
        vm.DestinationFile = "/export/out.html";
        Assert.False(vm.ExportEnabled);
    }

    [Fact]
    public async Task BrowseFileCommand_SetsDestinationFile()
    {
        A.CallTo(() => dialogs.ShowSaveFileDialogAsync(A<string>._, A<string>._, A<string>._, A<string>._))
            .Returns("/export/out.html");
        var vm = CreateViewModel();

        await vm.BrowseFileCommand.ExecuteAsync(null);

        Assert.Equal("/export/out.html", vm.DestinationFile);
    }

    [Fact]
    public async Task ExportToFileCommand_UserCancelsConfirm_DoesNotExport()
    {
        A.CallTo(() => dialogs.ShowConfirmDialogAsync(A<string>._)).Returns(false);
        var vm = CreateViewModel();
        vm.SearchResult = MakeSearchResult();
        vm.DestinationFile = "/export/out.html";

        await vm.ExportToFileCommand.ExecuteAsync(null);

        A.CallTo(() => dialogs.ShowProgressDialogAsync(A<System.Action<System.IProgress<string>, System.Threading.CancellationToken>>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ExportToFileCommand_UserConfirms_CallsExporter()
    {
        A.CallTo(() => dialogs.ShowConfirmDialogAsync(A<string>._)).Returns(true);
        A.CallTo(() => dataBuilder.BuildRich(A<List<FileModel>>._, A<string>._, A<System.Threading.CancellationToken>._))
            .Returns(new RichExportData("Name", "1.0", System.DateTime.Now, "https://example.com", [], [], [], []));
        A.CallTo(() => dialogs.ShowProgressDialogAsync(A<Action<IProgress<string>, System.Threading.CancellationToken>>._))
            .ReturnsLazily(call =>
            {
                var work = (Action<IProgress<string>, System.Threading.CancellationToken>)call.Arguments[0]!;
                work(A.Fake<IProgress<string>>(), System.Threading.CancellationToken.None);
                return Task.CompletedTask;
            });
        var vm = CreateViewModel();
        vm.SearchResult = MakeSearchResult();
        vm.DestinationFile = "/export/out.html";

        await vm.ExportToFileCommand.ExecuteAsync(null);

        A.CallTo(() => selfContainedHtmlExporter.Export(A<RichExportData>._, "/export/out.html"))
            .MustHaveHappenedOnceExactly();
    }
}

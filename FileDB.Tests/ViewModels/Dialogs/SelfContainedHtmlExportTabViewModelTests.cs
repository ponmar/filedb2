using FakeItEasy;
using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Export.SearchResult;
using FileDB.Model.FileFormats;
using FileDB.ViewModels.Dialogs;
using FileDB.ViewModels.Search;
using FileDBInterface.Model;
using Xunit;

namespace FileDB.Tests.ViewModels.Dialogs;

public class SelfContainedHtmlExportTabViewModelTests
{
    private readonly IDialogs dialogs = A.Fake<IDialogs>();
    private readonly ISearchResultExportDataBuilder dataBuilder = A.Fake<ISearchResultExportDataBuilder>();
    private readonly ISelfContainedHtmlExporter selfContainedHtmlExporter = A.Fake<ISelfContainedHtmlExporter>();
    private readonly IProcessUtils processUtils = A.Fake<IProcessUtils>();

    private SelfContainedHtmlExportTabViewModel CreateViewModel() =>
        new(dialogs, dataBuilder, processUtils, selfContainedHtmlExporter);

    private static SearchResult MakeSearchResult() =>
        new() { Files = [new FileModel { Id = 1, Path = "a.jpg" }] };

    [Fact]
    public async Task SelectAndExportCommand_NothingSelected_DoesNotExport()
    {
        A.CallTo(() => dialogs.ShowSaveFileDialogAsync(A<string>._, A<string>._, A<string>._, A<string>._)).Returns((string?)null);
        var vm = CreateViewModel();
        vm.SearchResult = MakeSearchResult();

        await vm.SelectAndExportCommand.ExecuteAsync(null);

        A.CallTo(() => dialogs.ShowProgressDialogAsync(A<Action<IProgress<string>, System.Threading.CancellationToken>>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task SelectAndExportCommand_FileSelected_CallsExporter()
    {
        A.CallTo(() => dialogs.ShowSaveFileDialogAsync(A<string>._, A<string>._, A<string>._, A<string>._))
            .Returns("/export/out.html");
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

        await vm.SelectAndExportCommand.ExecuteAsync(null);

        A.CallTo(() => selfContainedHtmlExporter.Export(A<RichExportData>._, "/export/out.html"))
            .MustHaveHappenedOnceExactly();
    }
}

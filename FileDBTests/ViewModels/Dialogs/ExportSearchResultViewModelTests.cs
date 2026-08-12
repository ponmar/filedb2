using FakeItEasy;
using FileDB;
using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Export.SearchResult;
using FileDB.ViewModels.Dialogs;
using FileDB.ViewModels.Search;
using System.IO.Abstractions;
using Xunit;
using FileDBInterface.Model;

namespace FileDBTests.ViewModels.Dialogs;

public class ExportSearchResultViewModelTests
{
    private static IProcessUtils FakeProcessUtils() => A.Fake<IProcessUtils>();

    private static FilesExportTabViewModel CreateFilesTab() =>
        new(A.Fake<IDialogs>(), A.Fake<IFileSystem>(), A.Fake<ISearchResultExportDataBuilder>(), FakeProcessUtils(), A.Fake<IFilesExporter>());

    private static M3uExportTabViewModel CreateM3uTab() =>
        new(A.Fake<IDialogs>(), A.Fake<IFileSystem>(), A.Fake<ISearchResultExportDataBuilder>(), FakeProcessUtils(), A.Fake<IFilesExporter>(), A.Fake<IM3uExporter>());

    private static HtmlExportTabViewModel CreateHtmlTab() =>
        new(A.Fake<IDialogs>(), A.Fake<IFileSystem>(), A.Fake<ISearchResultExportDataBuilder>(), FakeProcessUtils(), A.Fake<IHtmlExporter>());

    private static SelfContainedHtmlExportTabViewModel CreateSelfContainedHtmlTab() =>
        new(A.Fake<IDialogs>(), A.Fake<ISearchResultExportDataBuilder>(), FakeProcessUtils(), A.Fake<ISelfContainedHtmlExporter>());

    private static JsonExportTabViewModel CreateJsonTab() =>
        new(A.Fake<IDialogs>(), A.Fake<ISearchResultExportDataBuilder>(), FakeProcessUtils(), A.Fake<IJsonExporter>());

    private static PdfExportTabViewModel CreatePdfTab() =>
        new(A.Fake<IDialogs>(), A.Fake<ISearchResultExportDataBuilder>(), FakeProcessUtils(), A.Fake<IPdfExporter>());

    private static ExportSearchResultViewModel CreateViewModel() =>
        new(CreateFilesTab(), CreateM3uTab(), CreateHtmlTab(), CreateSelfContainedHtmlTab(), CreateJsonTab(), CreatePdfTab());

    [Fact]
    public void SearchResult_Set_PropagatedToAllTabs()
    {
        var filesTab = CreateFilesTab();
        var m3uTab = CreateM3uTab();
        var htmlTab = CreateHtmlTab();
        var selfContainedHtmlTab = CreateSelfContainedHtmlTab();
        var jsonTab = CreateJsonTab();
        var pdfTab = CreatePdfTab();
        var vm = new ExportSearchResultViewModel(filesTab, m3uTab, htmlTab, selfContainedHtmlTab, jsonTab, pdfTab);

        var searchResult = new SearchResult { Files = [new FileModel { Id = 1, Path = "a.jpg" }] };
        vm.SearchResult = searchResult;

        Assert.Equal(searchResult, filesTab.SearchResult);
        Assert.Equal(searchResult, m3uTab.SearchResult);
        Assert.Equal(searchResult, htmlTab.SearchResult);
        Assert.Equal(searchResult, selfContainedHtmlTab.SearchResult);
        Assert.Equal(searchResult, jsonTab.SearchResult);
        Assert.Equal(searchResult, pdfTab.SearchResult);
    }

    [Fact]
    public void TabProperties_ReturnInjectedInstances()
    {
        var filesTab = CreateFilesTab();
        var m3uTab = CreateM3uTab();
        var htmlTab = CreateHtmlTab();
        var selfContainedHtmlTab = CreateSelfContainedHtmlTab();
        var jsonTab = CreateJsonTab();
        var pdfTab = CreatePdfTab();
        var vm = new ExportSearchResultViewModel(filesTab, m3uTab, htmlTab, selfContainedHtmlTab, jsonTab, pdfTab);

        Assert.Same(filesTab, vm.FilesTab);
        Assert.Same(m3uTab, vm.M3uTab);
        Assert.Same(htmlTab, vm.HtmlTab);
        Assert.Same(selfContainedHtmlTab, vm.SelfContainedHtmlTab);
        Assert.Same(jsonTab, vm.JsonTab);
        Assert.Same(pdfTab, vm.PdfTab);
    }
}

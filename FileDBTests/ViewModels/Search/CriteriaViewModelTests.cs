using FakeItEasy;
using FileDB.Dialogs;
using FileDB.Infrastructure;
using FileDB.Model;
using FileDB.ViewModels;
using FileDB.ViewModels.Search;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.ViewModels.Search;

public class CriteriaViewModelTests : IDisposable
{
    private readonly IDialogs dialogs = A.Fake<IDialogs>();
    private readonly IDatabaseAccessProvider dbAccessProvider = A.Fake<IDatabaseAccessProvider>();
    private readonly ISearchResultRepositoryManagement searchResultRepoManagement = A.Fake<ISearchResultRepositoryManagement>();
    private readonly IDatabaseAccess dbAccess = A.Fake<IDatabaseAccess>();

    public CriteriaViewModelTests()
    {
        ServiceLocator.Reset();

        var configProvider = A.Fake<IConfigProvider>();
        A.CallTo(() => configProvider.Config).Returns(new ConfigBuilder
        {
            InitialSearchFilterType = FilterType.AllFiles,
        }.Build());

        var searchResultRepository = A.Fake<ISearchResultRepository>();
        A.CallTo(() => searchResultRepository.Files).Returns([]);

        ServiceLocator.RegisterSingleton<IConfigProvider>(configProvider);
        ServiceLocator.RegisterSingleton<ISearchResultRepository>(searchResultRepository);
        ServiceLocator.RegisterTransient<FilterSelectionViewModel>();
        ServiceLocator.RegisterTransient<AllFilesViewModel>();
        ServiceLocator.RegisterTransient<FileListViewModel>();

        A.CallTo(() => dbAccessProvider.DbAccess).Returns(dbAccess);
    }

    public void Dispose() => ServiceLocator.Reset();

    [Fact]
    public void Constructor_AddsInitialFilter()
    {
        var viewModel = new CriteriaViewModel(dialogs, dbAccessProvider, searchResultRepoManagement);

        Assert.Single(viewModel.FilterSettings);
        Assert.False(viewModel.FilterCanBeRemoved);
    }

    [Fact]
    public async Task SearchForFilesAsync_PopulatesSearchResultRepository()
    {
        var files = new[] { new FileModel { Id = 1, Path = "a.jpg" } };
        A.CallTo(() => dbAccess.SearchFilesFromIds(A<IEnumerable<int>>._)).Returns(files);

        var viewModel = new CriteriaViewModel(dialogs, dbAccessProvider, searchResultRepoManagement);

        await viewModel.SearchForFilesAsync("1");

        A.CallTo(() => searchResultRepoManagement.PopulateRepo(A<IEnumerable<FileModel>>.That.Matches(x => x.Single().Id == 1))).MustHaveHappened();
        Assert.Single(viewModel.FilterSettings);
        Assert.IsType<FileListViewModel>(viewModel.FilterSettings.First().FilterViewModel);
    }
}

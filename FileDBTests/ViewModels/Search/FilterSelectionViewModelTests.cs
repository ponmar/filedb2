using FakeItEasy;
using FileDB.Infrastructure;
using FileDB.Model;
using FileDB.ViewModels.Search;
using FileDB.ViewModels.Search.Filters;
using Xunit;

namespace FileDBTests.ViewModels.Search;

public class FilterSelectionViewModelTests : IDisposable
{
    private readonly IConfigProvider configProvider = A.Fake<IConfigProvider>();

    public FilterSelectionViewModelTests()
    {
        ServiceLocator.Reset();

        var containerConfigProvider = A.Fake<IConfigProvider>();
        A.CallTo(() => containerConfigProvider.Config).Returns(new ConfigBuilder
        {
            InitialSearchFilterType = FilterType.AllFiles,
        }.Build());

        var searchResultRepository = A.Fake<ISearchResultRepository>();
        A.CallTo(() => searchResultRepository.Files).Returns([]);

        ServiceLocator.RegisterSingleton<IConfigProvider>(containerConfigProvider);
        ServiceLocator.RegisterSingleton<ISearchResultRepository>(searchResultRepository);
        ServiceLocator.RegisterTransient<FilterSelectionViewModel>();
        ServiceLocator.RegisterTransient<AllFilesViewModel>();
        ServiceLocator.RegisterTransient<FileListViewModel>();
    }

    public void Dispose() => ServiceLocator.Reset();

    [Fact]
    public void Constructor_UsesInitialFilterTypeFromConfig()
    {
        A.CallTo(() => configProvider.Config).Returns(new ConfigBuilder
        {
            InitialSearchFilterType = FilterType.FileList,
        }.Build());

        var viewModel = new FilterSelectionViewModel(configProvider);

        Assert.Equal(FilterType.FileList, viewModel.SelectedFilterType);
        Assert.IsType<FileListViewModel>(viewModel.FilterViewModel);
    }

    [Fact]
    public void SelectedFilterType_ChangesFilterViewModel()
    {
        A.CallTo(() => configProvider.Config).Returns(new ConfigBuilder
        {
            InitialSearchFilterType = FilterType.AllFiles,
        }.Build());

        var viewModel = new FilterSelectionViewModel(configProvider);

        viewModel.SelectedFilterType = FilterType.FileList;

        Assert.IsType<FileListViewModel>(viewModel.FilterViewModel);
    }
}

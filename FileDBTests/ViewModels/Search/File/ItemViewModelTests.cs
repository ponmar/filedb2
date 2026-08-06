using FakeItEasy;
using FileDB.Model;
using FileDB.ViewModels.Search.File;
using FileDBInterface.Model;
using Xunit;

namespace FileDBTests.ViewModels.Search.File;

public class ItemViewModelTests
{
    private readonly IConfigProvider configProvider = A.Fake<IConfigProvider>();

    public ItemViewModelTests()
    {
        A.CallTo(() => configProvider.Config).Returns(new ConfigBuilder { ShortItemNameMaxLength = 50 }.Build());
    }

    [Fact]
    public void Constructor_WithPerson_SetsPersonType()
    {
        var person = new PersonModel { Id = 1, ShortName = "Alice", FullName = "Alice Smith" };

        var viewModel = new ItemViewModel(person, configProvider);

        Assert.Equal(CombinedItemType.Person, viewModel.Type);
        Assert.True(viewModel.IsPerson);
        Assert.Equal(1, viewModel.Id);
        Assert.Contains(viewModel.Name, viewModel.DisplayName);
    }

    [Fact]
    public void Constructor_WithTag_SetsTagType()
    {
        var tag = new TagModel { Id = 2, Name = "Favorites" };

        var viewModel = new ItemViewModel(tag, configProvider);

        Assert.Equal(CombinedItemType.Tag, viewModel.Type);
        Assert.False(viewModel.IsPerson);
        Assert.Equal(2, viewModel.Id);
        Assert.Contains("Favorites", viewModel.DisplayName);
    }

    [Fact]
    public void ApplyFilters_WithMatchingText_ShowsItem()
    {
        var location = new LocationModel { Id = 3, Name = "Home" };
        var viewModel = new ItemViewModel(location, configProvider);

        viewModel.ApplyFilters(["ho"], false);

        Assert.True(viewModel.IsVisible);
    }
}

using FileDB.ViewModels.Search.File;
using Xunit;

namespace FileDB.Tests.ViewModels.Search.File;

public class HistoryItemViewModelTests
{
    [Fact]
    public void Constructor_WithoutHotKey_UsesGenericToggleText()
    {
        var viewModel = new HistoryItemViewModel
        {
            Type = CombinedItemType.Person,
            ItemId = 1,
            ShortItemName = "Alice",
            ItemName = "Alice Smith",
            FunctionKey = 0,
        };

        Assert.Contains("Alice", viewModel.ToggleText);
        Assert.Contains("Alice Smith", viewModel.ToolTip);
    }

    [Fact]
    public void Constructor_WithHotKey_UsesHotKeyToggleText()
    {
        var viewModel = new HistoryItemViewModel
        {
            Type = CombinedItemType.Tag,
            ItemId = 2,
            ShortItemName = "Favorites",
            ItemName = "Favorites",
            FunctionKey = 3,
        };

        Assert.Contains("3", viewModel.ToggleText);
    }

    [Fact]
    public void IsChecked_ChangesTooltipText()
    {
        var viewModel = new HistoryItemViewModel
        {
            Type = CombinedItemType.Location,
            ItemId = 3,
            ShortItemName = "Home",
            ItemName = "Home",
        };

        var included = viewModel.ToolTip;
        viewModel.IsChecked = true;

        Assert.NotEqual(included, viewModel.ToolTip);
    }
}

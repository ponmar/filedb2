using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Lang;

namespace FileDB.ViewModels.Search.File;

public partial class HistoryItemViewModel : ObservableObject
{
    public required UpdateHistoryType Type { get; init; }
    public required int ItemId { get; init; }
    public required string ShortItemName { get; init; }
    public required string ItemName { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ToggleText))]
    public partial int FunctionKey { get; set; }

    private bool HasHotKey => FunctionKey >= 1 && FunctionKey <= 12;

    public string ToggleText => HasHotKey ?
        string.Format(Strings.SearchToggleWithKeyText, FunctionKey, ShortItemName) :
        string.Format(Strings.SearchToggleText, ShortItemName);

    public string ToolTip => IsChecked ?
        string.Format(Strings.SearchToggleToolTipExclude, ItemName) :
        string.Format(Strings.SearchToggleToolTipInclude, ItemName);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ToolTip))]
    public partial bool IsChecked { get; set; }
}

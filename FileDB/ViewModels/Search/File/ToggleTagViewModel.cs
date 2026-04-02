using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Model;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.File;

public partial class ToggleTagViewModel : ObservableObject
{
    public int Id { get; }
    public string Name { get; }
    public string ToolTip { get; }

    [ObservableProperty]
    public partial bool IsChecked { get; set; }

    [ObservableProperty]
    public partial bool IsVisible { get; set; }

    public ToggleTagViewModel(TagModel tag, IConfigProvider configProvider)
    {
        Id = tag.Id;
        Name = Utils.CreateShortText(tag.Name, configProvider.Config.ShortItemNameMaxLength);
        ToolTip = tag.Name;
    }

    public void ApplyFilters(IEnumerable<string> filters, bool caseSensitive)
    {
        var stringComparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        IsVisible = !filters.Any() || filters.Any(x => Name.Contains(x, stringComparison));
    }
}

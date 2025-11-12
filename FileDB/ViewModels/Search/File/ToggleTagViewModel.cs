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
    private bool isChecked;

    [ObservableProperty]
    private bool isVisible;

    public ToggleTagViewModel(TagModel tag, IConfigProvider configProvider)
    {
        Id = tag.Id;
        Name = Utils.CreateShortText(tag.Name, configProvider.Config.ShortItemNameMaxLength);
        ToolTip = tag.Name;
    }

    public void ApplyFilters(IEnumerable<string> filters)
    {
        IsVisible = !filters.Any() || filters.Any(x => Name.Contains(x, StringComparison.OrdinalIgnoreCase));
    }
}

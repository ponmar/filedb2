using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Model;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.File;

public partial class ToggleLocationViewModel : ObservableObject
{
    public int Id { get; }
    public string Name { get; }
    public string ToolTip { get; }

    [ObservableProperty]
    private bool isChecked;

    [ObservableProperty]
    private bool isVisible;

    public ToggleLocationViewModel(LocationModel location, IConfigProvider configProvider)
    {
        Id = location.Id;
        Name = Utils.CreateShortText(location.Name, configProvider.Config.ShortItemNameMaxLength);
        ToolTip = location.Name + (location.Description is not null ? $":\n{location.Description}" : string.Empty);
    }

    public void ApplyFilters(IEnumerable<string> filters)
    {
        IsVisible = !filters.Any() || filters.Any(x => Name.Contains(x, StringComparison.OrdinalIgnoreCase));
    }
}

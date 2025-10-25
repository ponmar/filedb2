using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Model;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.File;

public partial class TogglePersonViewModel : ObservableObject
{
    public int Id { get; }
    public string Name { get; }
    public string ToolTip { get; }

    [ObservableProperty]
    private bool isChecked;

    [ObservableProperty]
    private bool isVisible;

    private readonly string nameForFilters;

    public TogglePersonViewModel(PersonModel person, IConfigProvider configProvider)
    {
        Id = person.Id;
        Name = FileTextOverlayCreator.GetShortPersonText(person, DateTime.Now, configProvider.Config.ShortItemNameMaxLength);
        ToolTip = $"{person.Firstname} {person.Lastname}" + (person.Description is not null ? $":\n{person.Description}" : string.Empty);
        nameForFilters = $"{person.Firstname} {person.Lastname}";
    }

    public void ApplyFilters(IEnumerable<string> filters)
    {
        IsVisible = !filters.Any() || filters.Any(x => nameForFilters.Contains(x, StringComparison.OrdinalIgnoreCase));
    }
}

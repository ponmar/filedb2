using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Model;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.File;

public enum CombinedItemType { Person, Location, Tag }

public partial class ItemViewModel : ObservableObject
{
    public int Id { get; }
    public CombinedItemType Type { get; }
    public string Name { get; }
    public string ToolTip { get; }

    [ObservableProperty]
    public partial bool IsChecked { get; set; }

    [ObservableProperty]
    public partial bool IsVisible { get; set; }

    private readonly string nameForFilters;

    public ItemViewModel(PersonModel person, IConfigProvider configProvider)
    {
        Type = CombinedItemType.Person;
        Id = person.Id;
        Name = FileTextOverlayCreator.GetShortPersonText(person, DateTime.Now, configProvider.Config.ShortItemNameMaxLength);
        ToolTip = person.FullName + (person.Description is not null ? $":\n{person.Description}" : string.Empty);
        nameForFilters = person.FullName;
    }

    public ItemViewModel(TagModel tag, IConfigProvider configProvider)
    {
        Type = CombinedItemType.Tag;
        Id = tag.Id;
        Name = Utils.CreateShortText(tag.Name, configProvider.Config.ShortItemNameMaxLength);
        ToolTip = tag.Name;
        nameForFilters = tag.Name;
    }

    public ItemViewModel(LocationModel location, IConfigProvider configProvider)
    {
        Type = CombinedItemType.Location;
        Id = location.Id;
        Name = Utils.CreateShortText(location.Name, configProvider.Config.ShortItemNameMaxLength);
        ToolTip = location.Name + (location.Description is not null ? $":\n{location.Description}" : string.Empty);
        nameForFilters = location.Name;
    }

    public string Prefix => Type switch
    {
        CombinedItemType.Person => "\uD83D\uDC64", // 👤
        CombinedItemType.Location => "\uD83C\uDFE0", // 🏠
        CombinedItemType.Tag => "\uD83D\uDD16", // 🔖
        _ => string.Empty
    };

    public string DisplayName => string.IsNullOrWhiteSpace(Prefix) ? Name : $"{Prefix} {Name}";

    public void ApplyFilters(IEnumerable<string> filters, bool caseSensitive)
    {
        var stringComparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        IsVisible = !filters.Any() || filters.Any(x => nameForFilters.Contains(x, stringComparison));
    }
}

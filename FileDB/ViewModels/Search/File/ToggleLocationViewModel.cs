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

    public void ApplyFilter(string filterText)
    {
        IsVisible = string.IsNullOrWhiteSpace(filterText) || Name.Contains(filterText, System.StringComparison.OrdinalIgnoreCase);
    }
}

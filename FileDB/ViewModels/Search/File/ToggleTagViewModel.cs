using CommunityToolkit.Mvvm.ComponentModel;

namespace FileDB.ViewModels.Search.File;

public partial class ToggleTagViewModel : ObservableObject
{
    public int Id { get; }
    public string Name { get; }
    public string ShortName { get; }

    [ObservableProperty]
    private bool isChecked;

    [ObservableProperty]
    private bool isVisible;

    public ToggleTagViewModel(int id, string name, string shortName)
    {
        Id = id;
        Name = name;
        ShortName = shortName;
    }

    // Note: overridden for combobox text search
    public override string ToString() => ShortName;

    public void ApplyFilter(string filterText)
    {
        IsVisible = string.IsNullOrWhiteSpace(filterText) || Name.Contains(filterText, System.StringComparison.OrdinalIgnoreCase);
    }
}

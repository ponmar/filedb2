using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;
using FileDB.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FileDB.ViewModel;

public partial class FilterSettingsViewModel : ObservableObject
{
    public static IEnumerable<FilterType> FilterTypes => Enum.GetValues<FilterType>().OrderBy(x => x.ToFriendlyString());

    [ObservableProperty]
    private FilterType selectedFilterType = FilterTypes.First();

    [ObservableProperty]
    private string textFilterSearchPattern = string.Empty;

    [ObservableProperty]
    private string fileListIds = string.Empty;

    public static IEnumerable<Model.FileType> FileTypes => Enum.GetValues<Model.FileType>().OrderBy(x => x.ToFriendlyString());

    [ObservableProperty]
    private Model.FileType selectedFileType = FileTypes.First();

    public IFilesFilter Create()
    {
        return SelectedFilterType switch
        {
            FilterType.NoDateTime => new WithoutDateTime(),
            FilterType.NoMetaData => new WithoutMetaData(),
            FilterType.Text => new Text(TextFilterSearchPattern),
            FilterType.FileList => new FileList(FileListIds),
            FilterType.FileType => new FilesFilter.FileType(SelectedFileType),
            _ => throw new NotImplementedException(),
        };
    }
}

using System.Collections.Generic;
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;
using FileDB.Model;
using FileDBInterface.FileFormats;
using System.Linq;
using FileDB.Extensions;

namespace FileDB.ViewModels.Search.Filters;

public partial class FileTypeViewModel : ObservableObject, IFilterViewModel
{
    public static IEnumerable<FileType> FileTypes { get; } = Enum.GetValues<FileType>().Where(x => x != FileType.Unknown).OrderBy(x => x.ToFriendlyString());

    [ObservableProperty]
    private FileType selectedFileType = FileTypes.First();

    public IFilesFilter CreateFilter() => new FileTypeFilter(SelectedFileType);
}

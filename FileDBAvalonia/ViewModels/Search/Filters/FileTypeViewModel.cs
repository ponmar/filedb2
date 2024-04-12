using System.Collections.Generic;
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;
using FileDBAvalonia.Model;
using FileDBShared.FileFormats;
using System.Linq;
using FileDBAvalonia.Extensions;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public partial class FileTypeViewModel : AbstractFilterViewModel
{
    public static IEnumerable<FileType> FileTypes { get; } = Enum.GetValues<FileType>().Where(x => x != FileType.Unknown).OrderBy(x => x.ToFriendlyString());

    [ObservableProperty]
    private FileType selectedFileType = FileTypes.First();

    public FileTypeViewModel() : base(FilterType.FileType)
    {
    }

    protected override IFilesFilter DoCreate() => new FilterFileType(SelectedFileType);
}

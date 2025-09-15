using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Extensions;
using FileDB.Model;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.FileFormats;
using FileDBInterface.Model;
using FileDBInterface.Extensions;

namespace FileDB.ViewModels.Search.Filters;

public partial class FileTypeViewModel : ObservableValidator, IFilterViewModel
{
    public static IEnumerable<FileType> FileTypes { get; } = Enum.GetValues<FileType>().Where(x => x != FileType.Unknown).OrderBy(x => x.ToFriendlyString());

    [ObservableProperty]
    private FileType selectedFileType = FileTypes.First();

    public FileTypeViewModel()
    {
        ValidateAllProperties();
    }

    public IEnumerable<FileModel> ApplyFilter(IDatabaseAccess dbAccess)
    {
        var fileExtensions = SelectedFileType.GetSupportedFileExtensions();

        var result = new List<FileModel>();
        foreach (var extension in fileExtensions)
        {
            result.AddRange(dbAccess.SearchFilesByExtension(extension));
        }

        return result;
    }
}

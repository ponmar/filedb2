using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Model;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.FilesystemAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class TextFileContentViewModel : ObservableValidator, IFilterViewModel
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    public partial string FileExtensions { get; set; } = ".txt .md";

    private readonly IFilesystemAccess filesystemAccess;

    public TextFileContentViewModel(IFilesystemAccessProvider filesystemAccessProvider)
    {
        filesystemAccess = filesystemAccessProvider.FilesystemAccess;
        ValidateAllProperties();
    }

    public IEnumerable<FileModel> ApplyFilter(IDatabaseAccess dbAccess)
    {
        var extensions = FileExtensions
            .Split([' ', ',', ';'], StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.StartsWith('.') ? e : '.' + e)
            .ToList();

        var candidates = new List<FileModel>();
        foreach (var ext in extensions)
        {
            candidates.AddRange(dbAccess.SearchFilesByExtension(ext));
        }

        var result = new List<FileModel>();
        foreach (var file in candidates)
        {
            try
            {
                var absolutePath = filesystemAccess.ToAbsolutePath(file.Path);
                var content = filesystemAccess.FileSystem.File.ReadAllText(absolutePath);
                if (content.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase))
                {
                    result.Add(file);
                }
            }
            catch (Exception)
            {
                // Skip files that cannot be read
            }
        }

        return result;
    }
}

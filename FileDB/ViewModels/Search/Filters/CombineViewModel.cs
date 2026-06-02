using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Infrastructure;
using FileDB.Model;
using FileDB.Services;
using FileDB.Validators;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Extensions;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class CombineViewModel : ObservableValidator, IFilterViewModel
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    [IsFileIdsText(ErrorMessage = "Format error")]
    [NotifyPropertyChangedFor(nameof(CombineSearchResultPossible))]
    public partial string CombineSearch1 { get; set; } = string.Empty;

    partial void OnCombineSearch1Changed(string value)
    {
        CombineSearchResult = string.Empty;
    }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    [IsFileIdsText(ErrorMessage = "Format error")]
    [NotifyPropertyChangedFor(nameof(CombineSearchResultPossible))]
    public partial string CombineSearch2 { get; set; } = string.Empty;

    partial void OnCombineSearch2Changed(string value)
    {
        CombineSearchResult = string.Empty;
    }

    [ObservableProperty]
    public partial string CombineSearchResult { get; set; } = string.Empty;

    public bool CombineSearchResultPossible => CombineSearch1.HasContent() && CombineSearch2.HasContent();
    
    public bool HasSearchResult => searchResultRepository.Files.Any();

    private readonly ISearchResultRepository searchResultRepository;
    private readonly IClipboardService clipboardService;
    private readonly IFileSelector fileSelector;

    public CombineViewModel(ISearchResultRepository searchResultRepository, IClipboardService clipboardService, IFileSelector fileSelector)
    {
        this.searchResultRepository = searchResultRepository;
        this.clipboardService = clipboardService;
        this.fileSelector = fileSelector;

        this.RegisterForEvent<SearchResultRepositoryUpdated>(x => OnPropertyChanged(nameof(HasSearchResult)));

        ValidateAllProperties();
    }

    [RelayCommand]
    private void SetCombineSearch1()
    {
        CombineSearch1 = Utils.CreateFileList(searchResultRepository.Files);
    }

    [RelayCommand]
    private void SetCombineSearch2()
    {
        CombineSearch2 = Utils.CreateFileList(searchResultRepository.Files);
    }

    [RelayCommand]
    private void CombineSearchIntersection()
    {
        if (Utils.TryParseFileIds(CombineSearch1, out var files1) &&
            Utils.TryParseFileIds(CombineSearch2, out var files2))
        {
            var result = files1!.Intersect(files2!);
            CombineSearchResult = Utils.CreateFileList(result);
        }
    }

    [RelayCommand]
    private void CombineSearchUnion()
    {
        if (Utils.TryParseFileIds(CombineSearch1, out var files1) &&
            Utils.TryParseFileIds(CombineSearch2, out var files2))
        {
            var result = files1!.Union(files2!);
            CombineSearchResult = Utils.CreateFileList(result);
        }
    }

    [RelayCommand]
    private void CombineSearchDifference()
    {
        if (Utils.TryParseFileIds(CombineSearch1, out var files1) &&
            Utils.TryParseFileIds(CombineSearch2, out var files2))
        {
            var uniqueFiles1 = files1!.Except(files2!);
            var uniqueFiles2 = files2!.Except(files1!);
            var result = uniqueFiles1.Union(uniqueFiles2);
            CombineSearchResult = Utils.CreateFileList(result);
        }
    }

    public IEnumerable<FileModel> ApplyFilter(IDatabaseAccess dbAccess)
    {
        return Utils.TryParseFileIds(CombineSearchResult, out var fileIds) ? dbAccess.SearchFilesFromIds(fileIds!) : [];
    }
}

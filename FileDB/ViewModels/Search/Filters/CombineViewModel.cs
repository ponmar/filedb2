using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.FilesFilter;
using FileDB.Model;
using FileDBInterface.Extensions;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class CombineViewModel : ObservableObject, IFilterViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CombineSearchResultPossible))]
    private string combineSearch1 = string.Empty;

    partial void OnCombineSearch1Changed(string value)
    {
        CombineSearchResult = string.Empty;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CombineSearchResultPossible))]
    private string combineSearch2 = string.Empty;

    partial void OnCombineSearch2Changed(string value)
    {
        CombineSearchResult = string.Empty;
    }

    [ObservableProperty]
    private string combineSearchResult = string.Empty;

    public bool CombineSearchResultPossible => CombineSearch1.HasContent() && CombineSearch2.HasContent();

    [ObservableProperty]
    private FileModel? selectedFile;

    partial void OnSelectedFileChanged(FileModel? value)
    {
        HasFiles = value is not null;
    }

    [ObservableProperty]
    private bool hasFiles;

    private readonly ISearchResultRepository searchResultRepository;
    private readonly IClipboardService clipboardService;
    private readonly IFileSelector fileSelector;

    public CombineViewModel(ISearchResultRepository searchResultRepository, IClipboardService clipboardService, IFileSelector fileSelector)
    {
        this.searchResultRepository = searchResultRepository;
        this.clipboardService = clipboardService;
        this.fileSelector = fileSelector;

        this.RegisterForEvent<FileSelectionChanged>(x =>
        {
            SelectedFile = fileSelector.SelectedFile;
        });
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
        var files1 = Utils.CreateFileIds(CombineSearch1);
        var files2 = Utils.CreateFileIds(CombineSearch2);
        var result = files1.Intersect(files2);
        CombineSearchResult = Utils.CreateFileList(result);
    }

    [RelayCommand]
    private void CombineSearchUnion()
    {
        var files1 = Utils.CreateFileIds(CombineSearch1);
        var files2 = Utils.CreateFileIds(CombineSearch2);
        var result = files1.Union(files2);
        CombineSearchResult = Utils.CreateFileList(result);
    }

    [RelayCommand]
    private void CombineSearchDifference()
    {
        var files1 = Utils.CreateFileIds(CombineSearch1);
        var files2 = Utils.CreateFileIds(CombineSearch2);
        var uniqueFiles1 = files1.Except(files2);
        var uniqueFiles2 = files2.Except(files1);
        var result = uniqueFiles1.Union(uniqueFiles2);
        CombineSearchResult = Utils.CreateFileList(result);
    }

    public IFilesFilter CreateFilter()
    {
        return new FileListFilter(CombineSearchResult);
    }
}

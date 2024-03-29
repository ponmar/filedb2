using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBAvalonia.Dialogs;
using FileDBAvalonia.Extensions;
using FileDBAvalonia.Model;
using FileDBShared.Model;

namespace FileDBAvalonia.ViewModels;

public partial class TagsViewModel : ObservableObject
{
    [ObservableProperty]
    private string filterText = string.Empty;

    partial void OnFilterTextChanged(string value)
    {
        FilterTags();
    }

    [ObservableProperty]
    private bool readWriteMode;

    public ObservableCollection<TagModel> Tags { get; } = [];

    private readonly List<TagModel> allTags = [];

    [ObservableProperty]
    private TagModel? selectedTag;

    private readonly IConfigProvider configProvider;
    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IDialogs dialogs;
    private readonly ITagsRepository tagsRepository;

    public TagsViewModel(IConfigProvider configProvider, IDatabaseAccessProvider dbAccessProvider, IDialogs dialogs, ITagsRepository tagsRepository)
    {
        this.configProvider = configProvider;
        this.dbAccessProvider = dbAccessProvider;
        this.dialogs = dialogs;
        this.tagsRepository = tagsRepository;

        ReadWriteMode = !configProvider.Config.ReadOnly;

        ReloadTags();

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            ReadWriteMode = !this.configProvider.Config.ReadOnly;
        });

        this.RegisterForEvent<TagsUpdated>((x) =>
        {
            ReloadTags();
        });
    }

    [RelayCommand]
    private async Task RemoveTagAsync()
    {
        if (!await dialogs.ShowConfirmDialogAsync($"Remove {SelectedTag!.Name}?"))
        {
            return;
        }

        var filesWithTag = dbAccessProvider.DbAccess.SearchFilesWithTags([SelectedTag.Id]).ToList();
        if (filesWithTag.Count == 0 || await dialogs.ShowConfirmDialogAsync($"Tag is used in {filesWithTag.Count} files, remove anyway?"))
        {
            dbAccessProvider.DbAccess.DeleteTag(SelectedTag.Id);
            Messenger.Send<TagEdited>();
        }
    }

    [RelayCommand]
    private void EditTag()
    {
        dialogs.ShowAddTagDialogAsync(SelectedTag!.Id);
    }

    [RelayCommand]
    private void AddTag()
    {
        dialogs.ShowAddTagDialogAsync();
    }

    [RelayCommand]
    public void TagSelection(TagModel parameter)
    {
        SelectedTag = parameter;
    }

    private void ReloadTags()
    {
        allTags.Clear();
        allTags.AddRange(tagsRepository.Tags);
        FilterTags();
    }

    private void FilterTags()
    {
        Tags.Clear();
        foreach (var tag in allTags.Where(x => x.MatchesTextFilter(FilterText)))
        {
            Tags.Add(tag);
        }
    }

    [RelayCommand]
    private void ClearFilterText() => FilterText = string.Empty;
}

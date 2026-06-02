using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Dialogs;
using FileDB.Extensions;
using FileDB.Infrastructure;
using FileDB.Lang;
using FileDB.Model;
using FileDBInterface.Model;

namespace FileDB.ViewModels;

public partial class UpdateTagsViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string FilterText { get; set; } = string.Empty;

    partial void OnFilterTextChanged(string value)
    {
        FilterTags();
    }

    [ObservableProperty]
    public partial bool ReadOnly { get; set; }
    public ObservableCollection<TagModel> Tags { get; } = [];

    private readonly List<TagModel> allTags = [];

    [ObservableProperty]
    public partial TagModel? SelectedTag { get; set; }

    private readonly IConfigProvider configProvider;
    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IDialogs dialogs;
    private readonly ITagsRepository tagsRepository;

    public UpdateTagsViewModel(IConfigProvider configProvider, IDatabaseAccessProvider dbAccessProvider, IDialogs dialogs, ITagsRepository tagsRepository)
    {
        this.configProvider = configProvider;
        this.dbAccessProvider = dbAccessProvider;
        this.dialogs = dialogs;
        this.tagsRepository = tagsRepository;
        ReadOnly = configProvider.Config.ReadOnly;

        ReloadTags();

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            ReadOnly = this.configProvider.Config.ReadOnly;
        });

        this.RegisterForEvent<TagsUpdated>((x) =>
        {
            ReloadTags();
        });
    }

    [RelayCommand]
    private async Task RemoveTagAsync()
    {
        if (!await dialogs.ShowConfirmDialogAsync(string.Format(Strings.UpdateTagsRemoveSelectedTag, SelectedTag!.Name)))
        {
            return;
        }

        var filesWithTag = dbAccessProvider.DbAccess.SearchFilesWithTags([SelectedTag.Id]).ToList();
        if (filesWithTag.Count == 0 || await dialogs.ShowConfirmDialogAsync(string.Format(Strings.UpdateTagsRemoveUsedTag, filesWithTag.Count)))
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
        dialogs.ShowAddTagDialogAsync(tagName: FilterText);
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

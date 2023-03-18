using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Model;
using FileDB.Sorters;

namespace FileDB.ViewModel;

public record Tag(int Id, string Name);

public partial class TagsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool readWriteMode;

    public ObservableCollection<Tag> Tags { get; } = new();

    [ObservableProperty]
    private Tag? selectedTag;

    private readonly IConfigRepository configRepository;
    private readonly IDbAccessRepository dbAccessRepository;
    private readonly IDialogs dialogs;

    public TagsViewModel(IConfigRepository configRepository, IDbAccessRepository dbAccessRepository, IDialogs dialogs)
    {
        this.configRepository = configRepository;
        this.dbAccessRepository = dbAccessRepository;
        this.dialogs = dialogs;
        ReadWriteMode = !configRepository.Config.ReadOnly;

        ReloadTags();

        this.RegisterForEvent<ConfigLoaded>((x) =>
        {
            ReadWriteMode = !this.configRepository.Config.ReadOnly;
        });

        this.RegisterForEvent<TagsUpdated>((x) =>
        {
            ReloadTags();
        });
    }

    [RelayCommand]
    private void RemoveTag()
    {
        if (dialogs.ShowConfirmDialog($"Remove {SelectedTag!.Name}?"))
        {
            var filesWithTag = dbAccessRepository.DbAccess.SearchFilesWithTags(new List<int>() { SelectedTag.Id }).ToList();
            if (filesWithTag.Count == 0 || dialogs.ShowConfirmDialog($"Tag is used in {filesWithTag.Count} files, remove anyway?"))
            {
                dbAccessRepository.DbAccess.DeleteTag(SelectedTag.Id);
                Events.Send<TagsUpdated>();
            }
        }
    }

    [RelayCommand]
    private void EditTag()
    {
        dialogs.ShowAddTagDialog(SelectedTag!.Id);
    }

    [RelayCommand]
    private void AddTag()
    {
        dialogs.ShowAddTagDialog();
    }

    [RelayCommand]
    public void TagSelection(Tag parameter)
    {
        SelectedTag = parameter;
    }

    private void ReloadTags()
    {
        Tags.Clear();

        var tags = dbAccessRepository.DbAccess.GetTags().ToList();
        tags.Sort(new TagModelByNameSorter());
        foreach (var tag in tags.Select(tm => new Tag(tm.Id, tm.Name)))
        {
            Tags.Add(tag);
        }
    }
}

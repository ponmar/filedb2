using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Configuration;
using FileDB.Model;
using FileDB.Sorters;
using FileDBInterface.DbAccess;

namespace FileDB.ViewModel;

public record Tag(int Id, string Name);

public partial class TagsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool readWriteMode;

    public ObservableCollection<Tag> Tags { get; } = new();

    [ObservableProperty]
    private Tag? selectedTag;

    private Config config;
    private readonly IDbAccess dbAccess;
    private readonly IDialogs dialogs;

    public TagsViewModel(Config config, IDbAccess dbAccess, IDialogs dialogs)
    {
        this.config = config;
        this.dbAccess = dbAccess;
        this.dialogs = dialogs;
        ReadWriteMode = !config.ReadOnly;

        ReloadTags();

        this.RegisterForEvent<ConfigLoaded>((x) =>
        {
            this.config = x.Config;
            ReadWriteMode = !this.config.ReadOnly;
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
            var filesWithTag = dbAccess.SearchFilesWithTags(new List<int>() { SelectedTag.Id }).ToList();
            if (filesWithTag.Count == 0 || dialogs.ShowConfirmDialog($"Tag is used in {filesWithTag.Count} files, remove anyway?"))
            {
                dbAccess.DeleteTag(SelectedTag.Id);
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

        var tags = dbAccess.GetTags().ToList();
        tags.Sort(new TagModelByNameSorter());
        foreach (var tag in tags.Select(tm => new Tag(tm.Id, tm.Name)))
        {
            Tags.Add(tag);
        }
    }
}

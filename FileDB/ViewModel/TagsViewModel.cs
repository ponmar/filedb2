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

    public TagsViewModel(Config config, IDbAccess dbAccess)
    {
        this.config = config;
        this.dbAccess = dbAccess;
        ReadWriteMode = !config.ReadOnly;

        ReloadTags();

        WeakReferenceMessenger.Default.Register<ConfigLoaded>(this, (r, m) =>
        {
            this.config = m.Config;
            ReadWriteMode = !this.config.ReadOnly;
        });

        WeakReferenceMessenger.Default.Register<TagsUpdated>(this, (r, m) =>
        {
            ReloadTags();
        });
    }

    [RelayCommand]
    private void RemoveTag()
    {
        if (Dialogs.Instance.ShowConfirmDialog($"Remove {SelectedTag!.Name}?"))
        {
            var filesWithTag = dbAccess.SearchFilesWithTags(new List<int>() { SelectedTag.Id }).ToList();
            if (filesWithTag.Count == 0 || Dialogs.Instance.ShowConfirmDialog($"Tag is used in {filesWithTag.Count} files, remove anyway?"))
            {
                dbAccess.DeleteTag(SelectedTag.Id);
                WeakReferenceMessenger.Default.Send(new TagsUpdated());
            }
        }
    }

    [RelayCommand]
    private void EditTag()
    {
        Dialogs.Instance.ShowAddTagDialog(SelectedTag!.Id);
    }

    [RelayCommand]
    private void AddTag()
    {
        Dialogs.Instance.ShowAddTagDialog();
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

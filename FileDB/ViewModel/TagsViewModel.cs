using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Model;
using FileDB.Sorters;

namespace FileDB.ViewModel;

public record Tag(int Id, string Name);

public partial class TagsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool readWriteMode = !Model.Model.Instance.Config.ReadOnly;

    public ObservableCollection<Tag> Tags { get; } = new();

    [ObservableProperty]
    private Tag? selectedTag;

    private readonly Model.Model model = Model.Model.Instance;

    public TagsViewModel()
    {
        ReloadTags();

        WeakReferenceMessenger.Default.Register<ConfigLoaded>(this, (r, m) =>
        {
            ReadWriteMode = !model.Config.ReadOnly;
        });

        WeakReferenceMessenger.Default.Register<TagsUpdated>(this, (r, m) =>
        {
            ReloadTags();
        });
    }

    [RelayCommand]
    private void RemoveTag()
    {
        if (Dialogs.Instance.ShowConfirmDialog($"Remove {selectedTag!.Name}?"))
        {
            var filesWithTag = model.DbAccess.SearchFilesWithTags(new List<int>() { selectedTag.Id }).ToList();
            if (filesWithTag.Count == 0 || Dialogs.Instance.ShowConfirmDialog($"Tag is used in {filesWithTag.Count} files, remove anyway?"))
            {
                model.DbAccess.DeleteTag(selectedTag.Id);
                WeakReferenceMessenger.Default.Send(new TagsUpdated());
            }
        }
    }

    [RelayCommand]
    private void EditTag()
    {
        Dialogs.Instance.ShowAddTagDialog(selectedTag!.Id);
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

        var tags = model.DbAccess.GetTags().ToList();
        tags.Sort(new TagModelByNameSorter());
        foreach (var tag in tags.Select(tm => new Tag(tm.Id, tm.Name)))
        {
            Tags.Add(tag);
        }
    }
}

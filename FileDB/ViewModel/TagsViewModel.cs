using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Sorters;
using FileDB.View;

namespace FileDB.ViewModel
{
    public class Tag
    {
        public string Name { get; set; }

        private readonly int id;

        public Tag(int id)
        {
            this.id = id;
        }

        public int GetId()
        {
            return id;
        }
    }

    public partial class TagsViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool readWriteMode = !Model.Model.Instance.Config.ReadOnly;

        public ObservableCollection<Tag> Tags { get; } = new();

        [ObservableProperty]
        private Tag selectedTag;

        private readonly Model.Model model = Model.Model.Instance;

        public TagsViewModel()
        {
            ReloadTags();
            model.TagsUpdated += Model_TagsUpdated;
            model.ConfigLoaded += Model_ConfigLoaded;
        }

        private void Model_ConfigLoaded(object sender, System.EventArgs e)
        {
            ReadWriteMode = !model.Config.ReadOnly;
        }

        private void Model_TagsUpdated(object sender, System.EventArgs e)
        {
            ReloadTags();
        }

        [ICommand]
        private void RemoveTag()
        {
            if (Dialogs.ShowConfirmDialog($"Remove {selectedTag.Name}?"))
            {
                var filesWithTag = model.DbAccess.SearchFilesWithTags(new List<int>() { selectedTag.GetId() }).ToList();
                if (filesWithTag.Count == 0 || Dialogs.ShowConfirmDialog($"Tag is used in {filesWithTag.Count} files, remove anyway?"))
                {
                    model.DbAccess.DeleteTag(selectedTag.GetId());
                    model.NotifyTagsUpdated();
                }
            }
        }

        [ICommand]
        private void EditTag()
        {
            var window = new AddTagWindow(selectedTag.GetId())
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        [ICommand]
        private void AddTag()
        {
            var window = new AddTagWindow
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        [ICommand]
        public void TagSelection(Tag parameter)
        {
            SelectedTag = parameter;
        }

        private void ReloadTags()
        {
            Tags.Clear();

            var tags = model.DbAccess.GetTags().ToList();
            tags.Sort(new TagModelByNameSorter());
            foreach (var tag in tags.Select(tm => new Tag(tm.Id) { Name = tm.Name }))
            {
                Tags.Add(tag);
            }
        }
    }
}

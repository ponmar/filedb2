using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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

    public class TagsViewModel : ViewModelBase
    {
        public ICommand AddTagCommand => addTagCommand ??= new CommandHandler(AddTag);
        private ICommand addTagCommand;

        public ICommand EditTagCommand => editTagCommand ??= new CommandHandler(EditTag);
        private ICommand editTagCommand;

        public ICommand RemoveTagCommand => removeTagCommand ??= new CommandHandler(RemoveTag);
        private ICommand removeTagCommand;

        public ICommand TagSelectionCommand => tagSelectionCommand ??= new CommandHandler(TagSelectionChanged);
        private ICommand tagSelectionCommand;

        public bool ReadWriteMode
        {
            get => readWriteMode;
            set => SetProperty(ref readWriteMode, value);
        }
        private bool readWriteMode = !Model.Model.Instance.Config.ReadOnly;

        public ObservableCollection<Tag> Tags { get; } = new();

        public Tag SelectedTag
        {
            get => selectedTag;
            set => SetProperty(ref selectedTag, value);
        }
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

        public void RemoveTag()
        {
            if (Utils.ShowConfirmDialog($"Remove {selectedTag.Name}?"))
            {
                var filesWithTag = model.DbAccess.SearchFilesWithTags(new List<int>() { selectedTag.GetId() }).ToList();
                if (filesWithTag.Count == 0 || Utils.ShowConfirmDialog($"Tag is used in {filesWithTag.Count} files, remove anyway?"))
                {
                    model.DbAccess.DeleteTag(selectedTag.GetId());
                    model.NotifyTagsUpdated();
                }
            }
        }

        public void EditTag()
        {
            var window = new AddTagWindow(selectedTag.GetId())
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        public void AddTag()
        {
            var window = new AddTagWindow
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        public void TagSelectionChanged(object parameter)
        {
            SelectedTag = (Tag)parameter;
        }

        private void ReloadTags()
        {
            Tags.Clear();

            var tags = model.DbAccess.GetTags().Select(tm => new Tag(tm.Id) { Name = tm.Name });
            foreach (var tag in tags)
            {
                Tags.Add(tag);
            }
        }
    }
}

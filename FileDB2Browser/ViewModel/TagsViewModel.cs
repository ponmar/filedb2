using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FileDB2Browser.View;
using FileDB2Interface;
using FileDB2Interface.Model;

namespace FileDB2Browser.ViewModel
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

        public ObservableCollection<Tag> Tags { get; } = new();

        private Tag selectedTag;

        public TagsViewModel()
        {
            ReloadTags();
        }

        public void RemoveTag()
        {
            if (selectedTag != null)
            {
                var filesWithTag = Utils.FileDB2Handle.GetFilesWithTags(new List<int>() { selectedTag.GetId() }).ToList();
                if (filesWithTag.Count == 0 || Utils.ShowConfirmDialog($"Tag is used in {filesWithTag.Count} files, remove anyway?"))
                {
                    Utils.FileDB2Handle.DeleteTag(selectedTag.GetId());
                    ReloadTags();
                }
            }
        }

        public void EditTag()
        {
            if (selectedTag != null)
            {
                var window = new AddTagWindow(selectedTag.GetId())
                {
                    Owner = Application.Current.MainWindow
                };
                window.ShowDialog();
                ReloadTags();
            }
        }

        public void AddTag()
        {
            var window = new AddTagWindow
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            ReloadTags();
        }

        public void TagSelectionChanged(object parameter)
        {
            selectedTag = (Tag)parameter;
        }

        private void ReloadTags()
        {
            Tags.Clear();

            var tags = Utils.FileDB2Handle.GetTags().Select(tm => new Tag(tm.id) { Name = tm.name });
            foreach (var tag in tags)
            {
                Tags.Add(tag);
            }
        }
    }
}

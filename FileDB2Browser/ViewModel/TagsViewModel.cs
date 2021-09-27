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
        public ICommand AddTagCommand
        {
            get
            {
                return addTagCommand ??= new CommandHandler(AddTag);
            }
        }
        private ICommand addTagCommand;

        public ICommand EditTagCommand
        {
            get
            {
                return editTagCommand ??= new CommandHandler(EditTag);
            }
        }
        private ICommand editTagCommand;

        public ICommand RemoveTagCommand
        {
            get
            {
                return removeTagCommand ??= new CommandHandler(RemoveTag);
            }
        }
        private ICommand removeTagCommand;

        public ICommand TagSelectionCommand
        {
            get
            {
                return tagSelectionCommand ??= new CommandHandler(TagSelectionChanged);
            }
        }
        private ICommand tagSelectionCommand;

        public ObservableCollection<Tag> Tags { get; } = new ObservableCollection<Tag>();

        private Tag selectedTag;

        public TagsViewModel()
        {
            ReloadTags();
        }

        public void RemoveTag(object parameter)
        {
            if (selectedTag != null)
            {
                var filesWithTag = Utils.FileDB2Handle.GetFilesWithTags(new List<int>() { selectedTag.GetId() });
                if (filesWithTag.Count == 0 || Utils.ShowConfirmDialog($"Tag is used in {filesWithTag.Count} files, remove anyway?"))
                {
                    Utils.FileDB2Handle.DeleteTag(selectedTag.GetId());
                    ReloadTags();
                }
            }
        }

        public void EditTag(object parameter)
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

        public void AddTag(object parameter)
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

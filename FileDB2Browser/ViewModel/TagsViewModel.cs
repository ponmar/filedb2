using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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

    public class TagsViewModel
    {
        public ICommand RemoveTagCommand
        {
            get
            {
                return removeTagCommand ??= new CommandHandler(RemoveTag);
            }
        }
        private ICommand removeTagCommand;

        public ICommand EditTagCommand
        {
            get
            {
                return editTagCommand ??= new CommandHandler(EditTag);
            }
        }
        private ICommand editTagCommand;

        public ICommand TagSelectionCommand
        {
            get
            {
                return tagSelectionCommand ??= new CommandHandler(TagSelectionChanged);
            }
        }
        private ICommand tagSelectionCommand;

        public ObservableCollection<Tag> Tags { get; }

        private Tag selectedTag;

        private readonly FileDB2Handle fileDB2Handle;

        public TagsViewModel(FileDB2Handle fileDB2Handle)
        {
            this.fileDB2Handle = fileDB2Handle;
            var tags = GetTags();
            Tags = new ObservableCollection<Tag>(tags);
        }

        public void RemoveTag(object parameter)
        {
            if (selectedTag != null)
            {
                // TODO: only delete if tag not used in files?
                fileDB2Handle.DeleteTag(selectedTag.GetId());

                Tags.Clear();
                foreach (var tag in GetTags())
                {
                    Tags.Add(tag);
                }
            }
        }

        public void EditTag(object parameter)
        {
            if (selectedTag != null)
            {
                // TODO: edit in new window
            }
        }

        public void TagSelectionChanged(object parameter)
        {
            selectedTag = (Tag)parameter;
        }

        private IEnumerable<Tag> GetTags()
        {
            return fileDB2Handle.GetTags().Select(tm => new Tag(tm.id) { Name = tm.name });
        }
    }
}

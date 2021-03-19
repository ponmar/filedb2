using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileDB2Interface;
using FileDB2Interface.Model;

namespace FileDB2Browser.ViewModel
{
    public class Tag
    {
        public string Name { get; set; }
    }

    public class TagsViewModel
    {
        public ObservableCollection<Tag> Tags { get; }

        public TagsViewModel(FileDB2Handle fileDB2Handle)
        {
            var tags = fileDB2Handle.GetTags().Select(tm => new Tag() { Name = tm.name });
            Tags = new ObservableCollection<Tag>(tags);
        }
    }
}

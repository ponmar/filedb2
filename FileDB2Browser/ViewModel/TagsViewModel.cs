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
    public class TagsViewModel
    {
        private readonly FileDB2Handle fileDB2Handle;

        public ObservableCollection<TagModel> Tags { get; } = new ObservableCollection<TagModel>();

        public TagsViewModel(FileDB2Handle fileDB2Handle)
        {
            this.fileDB2Handle = fileDB2Handle;

            foreach (var tag in fileDB2Handle.GetTags())
            {
                Tags.Add(tag);
            }
        }
    }
}

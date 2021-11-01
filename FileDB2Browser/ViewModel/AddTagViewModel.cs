using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FileDB2Interface;
using FileDB2Interface.Exceptions;

namespace FileDB2Browser.ViewModel
{
    public class AddTagViewModel : ViewModelBase
    {
        private readonly int tagId;

        public string Title
        {
            get => title;
            set { SetProperty(ref title, value); }
        }
        private string title;

        public string Name
        {
            get => name;
            set { SetProperty(ref name, value); }
        }
        private string name = string.Empty;

        public ICommand SaveCommand => saveCommand ??= new CommandHandler(Save);
        private ICommand saveCommand;

        public AddTagViewModel(int tagId = -1)
        {
            this.tagId = tagId;

            Title = tagId == -1 ? "Add Tag" : "Edit Tag";

            if (tagId != -1)
            {
                var tagModel = Utils.FileDB2Handle.GetTagById(tagId);
                Name = tagModel.name;
            }
        }

        public void Save()
        {
            try
            {
                if (tagId == -1)
                {
                    Utils.FileDB2Handle.InsertTag(name);
                }
                else
                {
                    Utils.FileDB2Handle.UpdateTagName(tagId, name);
                }
            }
            catch (FileDB2DataValidationException e)
            {
                Utils.ShowErrorDialog(e.Message);
            }
        }
    }
}

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
        private readonly FileDB2Handle fileDB2Handle;
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

        public ICommand AddTagCommand
        {
            get
            {
                return addTagCommand ??= new CommandHandler(AddTag);
            }
        }
        private ICommand addTagCommand;

        public AddTagViewModel(FileDB2Handle fileDB2Handle, int tagId = -1)
        {
            this.fileDB2Handle = fileDB2Handle;
            this.tagId = tagId;

            Title = tagId == -1 ? "Add Tag" : "Edit Tag";

            if (tagId != -1)
            {
                var tagModel = fileDB2Handle.GetTagById(tagId);
                Name = tagModel.name;
            }
        }

        public void AddTag(object parameter)
        {
            try
            {
                if (tagId == -1)
                {
                    fileDB2Handle.InsertTag(name);
                }
                else
                {
                    fileDB2Handle.UpdateTagName(tagId, name);
                }
            }
            catch (FileDB2DataValidationException e)
            {
                Utils.ShowErrorDialog(e.Message);
            }
        }
    }
}

using System.Windows.Input;
using FileDBInterface.Exceptions;

namespace FileDB.ViewModel
{
    public class AddTagViewModel : ViewModelBase
    {
        private readonly int? tagId;

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

        public AddTagViewModel(int? tagId = null)
        {
            this.tagId = tagId;

            Title = tagId.HasValue ? "Edit Tag" : "Add Tag";

            if (tagId.HasValue)
            {
                var tagModel = Utils.FileDBHandle.GetTagById(tagId.Value);
                Name = tagModel.name;
            }
        }

        public void Save()
        {
            try
            {
                if (tagId.HasValue)
                {
                    Utils.FileDBHandle.UpdateTagName(tagId.Value, name);
                }
                else
                {
                    Utils.FileDBHandle.InsertTag(name);
                }
            }
            catch (FileDBDataValidationException e)
            {
                Utils.ShowErrorDialog(e.Message);
            }
        }
    }
}

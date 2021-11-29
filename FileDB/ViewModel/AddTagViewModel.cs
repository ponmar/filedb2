using System.Windows.Input;
using FileDBInterface.Exceptions;
using FileDBInterface.Model;

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
                var tagModel = Utils.DatabaseWrapper.GetTagById(tagId.Value);
                Name = tagModel.Name;
            }
        }

        public void Save()
        {
            try
            {
                var tag = new TagModel() { Id = tagId.HasValue ? tagId.Value : default, Name = name };

                if (tagId.HasValue)
                {
                    Utils.DatabaseWrapper.UpdateTag(tag);
                }
                else
                {
                    Utils.DatabaseWrapper.InsertTag(tag);
                }
            }
            catch (DataValidationException e)
            {
                Utils.ShowErrorDialog(e.Message);
            }
        }
    }
}

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

        private readonly Model.Model model = Model.Model.Instance;

        public AddTagViewModel(int? tagId = null)
        {
            this.tagId = tagId;

            Title = tagId.HasValue ? "Edit Tag" : "Add Tag";

            if (tagId.HasValue)
            {
                var tagModel = model.DbAccess.GetTagById(tagId.Value);
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
                    model.DbAccess.UpdateTag(tag);
                }
                else
                {
                    model.DbAccess.InsertTag(tag);
                }

                model.NotifyTagsUpdated();
            }
            catch (DataValidationException e)
            {
                Dialogs.ShowErrorDialog(e.Message);
            }
        }
    }
}

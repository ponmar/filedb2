using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBInterface.Exceptions;
using FileDBInterface.Model;

namespace FileDB.ViewModel
{
    public partial class AddTagViewModel : ObservableObject
    {
        private readonly int? tagId;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string name = string.Empty;

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

        [ICommand]
        private void Save()
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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBInterface.Exceptions;
using FileDBInterface.Model;
using System.Linq;

namespace FileDB.ViewModel
{
    public partial class AddTagViewModel : ObservableObject
    {
        private int? tagId;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string name = string.Empty;

        private readonly Model.Model model = Model.Model.Instance;

        public TagModel? AffectedTag { get; private set; }

        public AddTagViewModel(int? tagId = null)
        {
            this.tagId = tagId;

            title = tagId.HasValue ? "Edit Tag" : "Add Tag";

            if (tagId.HasValue)
            {
                var tagModel = model.DbAccess.GetTagById(tagId.Value);
                Name = tagModel.Name;
            }
        }

        [RelayCommand]
        private void Save()
        {
            try
            {
                var tag = new TagModel() { Id = tagId ?? default, Name = name };

                if (tagId.HasValue)
                {
                    model.DbAccess.UpdateTag(tag);
                    AffectedTag = model.DbAccess.GetTagById(tag.Id);
                }
                else
                {
                    model.DbAccess.InsertTag(tag);
                    AffectedTag = model.DbAccess.GetTags().First(x => x.Name == tag.Name);
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

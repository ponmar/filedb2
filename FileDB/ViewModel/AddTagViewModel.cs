using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Model;
using FileDBInterface.Exceptions;
using FileDBInterface.Model;
using System.Linq;

namespace FileDB.ViewModel;

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
                if (model.DbAccess.GetTags().Any(x => x.Name == tag.Name))
                {
                    Dialogs.ShowErrorDialog($"Tag '{tag.Name}' already added");
                    return;
                }

                model.DbAccess.InsertTag(tag);
                AffectedTag = model.DbAccess.GetTags().First(x => x.Name == tag.Name);
            }

            WeakReferenceMessenger.Default.Send(new CloseModalDialogRequested());
            WeakReferenceMessenger.Default.Send(new TagsUpdated());
        }
        catch (DataValidationException e)
        {
            Dialogs.ShowErrorDialog(e.Message);
        }
    }
}

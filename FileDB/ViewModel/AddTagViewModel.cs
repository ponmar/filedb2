using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Model;
using FileDBInterface.DbAccess;
using FileDBInterface.Exceptions;
using FileDBShared.Model;
using System.Linq;

namespace FileDB.ViewModel;

public partial class AddTagViewModel : ObservableObject
{
    private readonly int? tagId;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string name = string.Empty;

    public TagModel? AffectedTag { get; private set; }

    private readonly IDbAccess dbAccess;
    private readonly IDialogs dialogs;

    public AddTagViewModel(IDbAccess dbAccess, IDialogs dialogs, int? tagId = null)
    {
        this.dbAccess = dbAccess;
        this.dialogs = dialogs;
        this.tagId = tagId;

        title = tagId.HasValue ? "Edit Tag" : "Add Tag";

        if (tagId.HasValue)
        {
            var tagModel = dbAccess.GetTagById(tagId.Value);
            Name = tagModel.Name;
        }
    }

    [RelayCommand]
    private void Save()
    {
        try
        {
            var tag = new TagModel() { Id = tagId ?? default, Name = Name };

            if (tagId.HasValue)
            {
                dbAccess.UpdateTag(tag);
                AffectedTag = dbAccess.GetTagById(tag.Id);
            }
            else
            {
                if (dbAccess.GetTags().Any(x => x.Name == tag.Name))
                {
                    dialogs.ShowErrorDialog($"Tag '{tag.Name}' already added");
                    return;
                }

                dbAccess.InsertTag(tag);
                AffectedTag = dbAccess.GetTags().First(x => x.Name == tag.Name);
            }

            Events.Send<CloseModalDialogRequested>();
            Events.Send<TagsUpdated>();
        }
        catch (DataValidationException e)
        {
            dialogs.ShowErrorDialog(e.Message);
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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

    private IDbAccess dbAccess;

    public AddTagViewModel(IDbAccess dbAccess, int? tagId = null)
    {
        this.dbAccess = dbAccess;
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
                    Dialogs.Instance.ShowErrorDialog($"Tag '{tag.Name}' already added");
                    return;
                }

                dbAccess.InsertTag(tag);
                AffectedTag = dbAccess.GetTags().First(x => x.Name == tag.Name);
            }

            WeakReferenceMessenger.Default.Send(new CloseModalDialogRequested());
            WeakReferenceMessenger.Default.Send(new TagsUpdated());
        }
        catch (DataValidationException e)
        {
            Dialogs.Instance.ShowErrorDialog(e.Message);
        }
    }
}

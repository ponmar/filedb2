using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Model;
using FileDB.Resources;
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

    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IDialogs dialogs;

    public AddTagViewModel(IDatabaseAccessProvider dbAccessProvider, IDialogs dialogs, int? tagId = null)
    {
        this.dbAccessProvider = dbAccessProvider;
        this.dialogs = dialogs;
        this.tagId = tagId;

        title = tagId.HasValue ? Strings.AddTagEditTitle : Strings.AddTagAddTitle;

        if (tagId.HasValue)
        {
            var tagModel = dbAccessProvider.DbAccess.GetTagById(tagId.Value);
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
                dbAccessProvider.DbAccess.UpdateTag(tag);
                AffectedTag = dbAccessProvider.DbAccess.GetTagById(tag.Id);
            }
            else
            {
                if (dbAccessProvider.DbAccess.GetTags().Any(x => x.Name == tag.Name))
                {
                    dialogs.ShowErrorDialog($"Tag '{tag.Name}' already added");
                    return;
                }

                dbAccessProvider.DbAccess.InsertTag(tag);
                AffectedTag = dbAccessProvider.DbAccess.GetTags().First(x => x.Name == tag.Name);
            }

            Events.Send<CloseModalDialogRequest>();
            Events.Send<TagEdited>();
        }
        catch (DataValidationException e)
        {
            dialogs.ShowErrorDialog(e.Message);
        }
    }
}

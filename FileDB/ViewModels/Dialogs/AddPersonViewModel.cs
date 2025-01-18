using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Dialogs;
using FileDB.Lang;
using FileDB.Model;
using FileDB.ViewModels.Search;
using FileDBInterface.Exceptions;
using FileDBInterface.Extensions;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Dialogs;

public partial class AddPersonViewModel : ObservableObject
{
    private readonly int? personId;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string firstname = string.Empty;

    [ObservableProperty]
    private string lastname = string.Empty;

    [ObservableProperty]
    private string? description = null;

    [ObservableProperty]
    private string? dateOfBirth = null;

    [ObservableProperty]
    private string? deceased = null;

    [ObservableProperty]
    private FileModel? profilePictureFile;

    partial void OnProfilePictureFileChanged(FileModel? value)
    {
        if (value is not null)
        {
            ProfilePictureRotation = -DatabaseParsing.OrientationToDegrees(value!.Orientation ?? 0);
            var imageAbsPath = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(value!.Path);
            imageLoader.LoadImage(imageAbsPath);
        }
        else
        {
            ProfilePictureRotation = 0;
            ProfilePicture = null;
        }
    }

    [ObservableProperty]
    private Bitmap? profilePicture = null;

    [ObservableProperty]
    private int profilePictureRotation = 0;

    [ObservableProperty]
    private string sexSelection = Sex.NotApplicable.ToString();

    public List<string> SexValues { get; } = Enum.GetNames(typeof(Sex)).ToList();

    public PersonModel? AffectedPerson { get; private set; }

    public bool CanSetProfilePicture => fileSelector.SelectedFile is not null;

    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IDialogs dialogs;
    private readonly IFileSelector fileSelector;
    private readonly IFilesystemAccessProvider filesystemAccessProvider;
    private readonly IImageLoader imageLoader;
    private readonly IClipboardService clipboardService;

    public AddPersonViewModel(IDatabaseAccessProvider dbAccessProvider, IDialogs dialogs, IFileSelector fileSelector, IFilesystemAccessProvider filesystemAccessProvider, IImageLoader imageLoader, IClipboardService clipboardService, int? personId = null)
    {
        this.dbAccessProvider = dbAccessProvider;
        this.dialogs = dialogs;
        this.fileSelector = fileSelector;
        this.filesystemAccessProvider = filesystemAccessProvider;
        this.imageLoader = imageLoader;
        this.clipboardService = clipboardService;
        this.personId = personId;

        title = personId.HasValue ? Strings.AddPersonEditTitle : Strings.AddPersonAddTitle;

        this.RegisterForEvent<ImageLoaded>((x) =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (ProfilePictureFile is not null && x.FilePath == filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(ProfilePictureFile.Path))
                {
                    ProfilePicture = x.Image;
                }
            });
        });

        this.RegisterForEvent<ImageLoadError>(x =>
        {
            ProfilePicture = null;
            // TODO: show load error?
        });


        if (personId.HasValue)
        {
            var personModel = dbAccessProvider.DbAccess.GetPersonById(personId.Value);
            Firstname = personModel.Firstname;
            Lastname = personModel.Lastname;
            Description = personModel.Description;
            DateOfBirth = personModel.DateOfBirth;
            Deceased = personModel.Deceased;
            SexSelection = personModel.Sex.ToString();
            ProfilePictureFile = personModel.ProfileFileId is not null ? dbAccessProvider.DbAccess.GetFileById(personModel.ProfileFileId.Value) : null;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var newProfilePictureFileId = ProfilePictureFile?.Id;
        var newDescription = Description.HasContent() ? Description : null;
        var newDateOfBirth = DateOfBirth.HasContent() ? DateOfBirth : null;
        var newDeceased = Deceased.HasContent() ? Deceased : null;

        try
        {
            var person = new PersonModel()
            {
                Id = personId ?? default,
                Firstname = Firstname,
                Lastname = Lastname,
                DateOfBirth = newDateOfBirth,
                Deceased = newDeceased,
                Description = newDescription,
                ProfileFileId = newProfilePictureFileId,
                Sex = Enum.Parse<Sex>(SexSelection),
            };

            if (personId.HasValue)
            {
                dbAccessProvider.DbAccess.UpdatePerson(person);
                AffectedPerson = dbAccessProvider.DbAccess.GetPersonById(person.Id);
            }
            else
            {
                var anyPersonsWithThatName = dbAccessProvider.DbAccess.GetPersons().Any(x => x.Firstname == person.Firstname && x.Lastname == person.Lastname);
                if (anyPersonsWithThatName &&
                    !await dialogs.ShowConfirmDialogAsync(Strings.AddPersonPersonAlreadyAdded))
                {
                    return;
                }

                dbAccessProvider.DbAccess.InsertPerson(person);
                AffectedPerson = dbAccessProvider.DbAccess.GetPersons().First(x => x.Firstname == person.Firstname && x.Lastname == person.Lastname && x.DateOfBirth == person.DateOfBirth && x.Deceased == person.Deceased && x.Description == person.Description);
            }

            Messenger.Send<CloseModalDialogRequest>();
            Messenger.Send<PersonEdited>();
        }
        catch (DataValidationException e)
        {
            await dialogs.ShowErrorDialogAsync(e.Message);
        }
    }

    [RelayCommand]
    private void SetProfilePictureFromSearchResult()
    {
        ProfilePictureFile = fileSelector.SelectedFile;
    }

    [RelayCommand]
    private void CopyProfilePictureId()
    {
        var fileList = Utils.CreateFileList([ProfilePictureFile!]);
        clipboardService.SetTextAsync(fileList);
    }

    [RelayCommand]
    private void ClearProfilePicture()
    {
        ProfilePictureFile = null;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBAvalonia.Dialogs;
using FileDBAvalonia.Lang;
using FileDBAvalonia.Model;
using FileDBAvalonia.ViewModels.Search;
using FileDBInterface.Exceptions;
using FileDBInterface.Extensions;
using FileDBShared.Model;

namespace FileDBAvalonia.ViewModels.Dialogs;

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
    private string profilePictureFileId = string.Empty;

    [ObservableProperty]
    private string sexSelection = Sex.NotApplicable.ToString();

    public List<string> SexValues { get; } = Enum.GetNames(typeof(Sex)).ToList();

    public PersonModel? AffectedPerson { get; private set; }

    public bool CanSetProfilePicture => fileSelector.SelectedFile is not null;

    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IDialogs dialogs;
    private readonly IFileSelector fileSelector;

    public AddPersonViewModel(IDatabaseAccessProvider dbAccessProvider, IDialogs dialogs, IFileSelector fileSelector, int? personId = null)
    {
        this.dbAccessProvider = dbAccessProvider;
        this.dialogs = dialogs;
        this.fileSelector = fileSelector;
        this.personId = personId;

        title = personId.HasValue ? Strings.AddPersonEditTitle : Strings.AddPersonAddTitle;

        if (personId.HasValue)
        {
            var personModel = dbAccessProvider.DbAccess.GetPersonById(personId.Value);
            Firstname = personModel.Firstname;
            Lastname = personModel.Lastname;
            Description = personModel.Description;
            DateOfBirth = personModel.DateOfBirth;
            Deceased = personModel.Deceased;
            ProfilePictureFileId = personModel.ProfileFileId is null ? string.Empty : personModel.ProfileFileId.Value.ToString();
            SexSelection = personModel.Sex.ToString();
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        int? newProfileFileId = null;
        if (ProfilePictureFileId.HasContent())
        {
            if (!int.TryParse(ProfilePictureFileId, out var value))
            {
                await dialogs.ShowErrorDialogAsync("Given profile picture file id format not valid");
                return;
            }

            newProfileFileId = value;
        }

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
                ProfileFileId = newProfileFileId,
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
                    !await dialogs.ShowConfirmDialogAsync($"There is already a person with that name. Add anyway?"))
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
        if (fileSelector.SelectedFile is null)
        {
            return;
        }

        ProfilePictureFileId = fileSelector.SelectedFile.Id.ToString();
    }
}

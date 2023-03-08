using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Model;
using FileDBInterface.DbAccess;
using FileDBInterface.Exceptions;
using FileDBShared.Model;

namespace FileDB.ViewModel;

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

    private readonly IDbAccess dbAccess;
    private readonly IDialogs dialogs;

    public AddPersonViewModel(IDbAccess dbAccess, IDialogs dialogs, int? personId = null)
    {
        this.dbAccess = dbAccess;
        this.dialogs = dialogs;
        this.personId = personId;

        title = personId.HasValue ? "Edit Person" : "Add Person";

        if (personId.HasValue)
        {
            var personModel = dbAccess.GetPersonById(personId.Value);
            Firstname = personModel.Firstname;
            Lastname = personModel.Lastname;
            Description = personModel.Description;
            DateOfBirth = personModel.DateOfBirth;
            Deceased = personModel.Deceased;
            ProfilePictureFileId = personModel.ProfileFileId == null ? string.Empty : personModel.ProfileFileId.Value.ToString();
            SexSelection = personModel.Sex.ToString();
        }
    }

    [RelayCommand]
    private void Save()
    {
        int? newProfileFileId = null;
        if (!string.IsNullOrEmpty(ProfilePictureFileId))
        {
            if (!int.TryParse(ProfilePictureFileId, out var value))
            {
                dialogs.ShowErrorDialog("Given profile picture file id format not valid");
                return;
            }

            newProfileFileId = value;
        }

        var newDescription = string.IsNullOrEmpty(Description) ? null : Description;
        var newDateOfBirth = string.IsNullOrEmpty(DateOfBirth) ? null : DateOfBirth;
        var newDeceased = string.IsNullOrEmpty(Deceased) ? null : Deceased;

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
                dbAccess.UpdatePerson(person);
                AffectedPerson = dbAccess.GetPersonById(person.Id);
            }
            else
            {
                var anyPersonsWithThatName = dbAccess.GetPersons().Any(x => x.Firstname == person.Firstname && x.Lastname == person.Lastname);
                if (anyPersonsWithThatName &&
                    !dialogs.ShowConfirmDialog($"There is already a person with that name. Add anyway?"))
                {
                    return;
                }

                dbAccess.InsertPerson(person);
                AffectedPerson = dbAccess.GetPersons().First(x => x.Firstname == person.Firstname && x.Lastname == person.Lastname && x.DateOfBirth == person.DateOfBirth && x.Deceased == person.Deceased && x.Description == person.Description);
            }

            Events.Send<CloseModalDialogRequested>();
            Events.Send<PersonsUpdated>();
        }
        catch (DataValidationException e)
        {
            dialogs.ShowErrorDialog(e.Message);
        }
    }
}

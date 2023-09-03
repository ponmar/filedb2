using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Model;
using FileDB.Resources;
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

    public bool CanSetProfilePicture => searchResultRepository.SelectedFile != null;

    private readonly IDbAccessRepository dbAccessRepository;
    private readonly IDialogs dialogs;
    private readonly ISearchResultRepository searchResultRepository;

    public AddPersonViewModel(IDbAccessRepository dbAccessRepository, IDialogs dialogs, ISearchResultRepository searchResultRepository, int? personId = null)
    {
        this.dbAccessRepository = dbAccessRepository;
        this.dialogs = dialogs;
        this.searchResultRepository = searchResultRepository;
        this.personId = personId;

        title = personId.HasValue ? Strings.AddPersonEditTitle : Strings.AddPersonAddTitle;

        if (personId.HasValue)
        {
            var personModel = dbAccessRepository.DbAccess.GetPersonById(personId.Value);
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
                dbAccessRepository.DbAccess.UpdatePerson(person);
                AffectedPerson = dbAccessRepository.DbAccess.GetPersonById(person.Id);
            }
            else
            {
                var anyPersonsWithThatName = dbAccessRepository.DbAccess.GetPersons().Any(x => x.Firstname == person.Firstname && x.Lastname == person.Lastname);
                if (anyPersonsWithThatName &&
                    !dialogs.ShowConfirmDialog($"There is already a person with that name. Add anyway?"))
                {
                    return;
                }

                dbAccessRepository.DbAccess.InsertPerson(person);
                AffectedPerson = dbAccessRepository.DbAccess.GetPersons().First(x => x.Firstname == person.Firstname && x.Lastname == person.Lastname && x.DateOfBirth == person.DateOfBirth && x.Deceased == person.Deceased && x.Description == person.Description);
            }

            Events.Send<CloseModalDialogRequest>();
            Events.Send<PersonEdited>();
        }
        catch (DataValidationException e)
        {
            dialogs.ShowErrorDialog(e.Message);
        }
    }

    [RelayCommand]
    private void SetProfilePictureFromSearchResult()
    {
        if (searchResultRepository.SelectedFile == null)
        {
            return;
        }

        ProfilePictureFileId = searchResultRepository.SelectedFile.Id.ToString();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBInterface.Exceptions;
using FileDBInterface.Model;

namespace FileDB.ViewModel
{
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
        private string description = string.Empty;

        [ObservableProperty]
        private string dateOfBirth = string.Empty;

        [ObservableProperty]
        private string deceased = string.Empty;

        [ObservableProperty]
        private string profilePictureFileId = string.Empty;

        [ObservableProperty]
        private string sexSelection = Sex.NotApplicable.ToString();

        public List<string> SexValues { get; } = Enum.GetNames(typeof(Sex)).ToList();

        private readonly Model.Model model = Model.Model.Instance;

        public AddPersonViewModel(int? personId = null)
        {
            this.personId = personId;

            Title = personId.HasValue ? "Edit Person" : "Add Person";

            if (personId.HasValue)
            {
                var personModel = model.DbAccess.GetPersonById(personId.Value);
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
            if (!string.IsNullOrEmpty(profilePictureFileId))
            {
                if (!int.TryParse(profilePictureFileId, out var value))
                {
                    Dialogs.ShowErrorDialog("Given profile picture file id format not valid");
                    return;
                }

                newProfileFileId = value;
            }

            var newDescription = string.IsNullOrEmpty(description) ? null : description;
            var newDateOfBirth = string.IsNullOrEmpty(dateOfBirth) ? null : dateOfBirth;
            var newDeceased = string.IsNullOrEmpty(deceased) ? null : deceased;

            try
            {
                Sex SexEnum = Enum.Parse<Sex>(SexSelection);

                var person = new PersonModel()
                {
                    Id = personId.HasValue ? personId.Value : default,
                    Firstname = firstname,
                    Lastname = lastname,
                    DateOfBirth = newDateOfBirth,
                    Deceased = newDeceased,
                    Description = newDescription,
                    ProfileFileId = newProfileFileId,
                    Sex = SexEnum
                };

                if (personId.HasValue)
                {
                    model.DbAccess.UpdatePerson(person);
                }
                else
                {
                    model.DbAccess.InsertPerson(person);
                }

                model.NotifyPersonsUpdated();
            }
            catch (DataValidationException e)
            {
                Dialogs.ShowErrorDialog(e.Message);
            }
        }
    }
}

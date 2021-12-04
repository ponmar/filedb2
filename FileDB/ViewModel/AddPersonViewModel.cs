using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using FileDBInterface.Exceptions;
using FileDBInterface.Model;

namespace FileDB.ViewModel
{
    public class AddPersonViewModel : ViewModelBase
    {
        private readonly int? personId;

        public string Title
        {
            get => title;
            set { SetProperty(ref title, value); }
        }
        private string title;

        public string Firstname
        {
            get => firstname;
            set { SetProperty(ref firstname, value); }
        }
        private string firstname = string.Empty;

        public string Lastname
        {
            get => lastname;
            set { SetProperty(ref lastname, value); }
        }
        private string lastname = string.Empty;

        public string Description
        {
            get => description;
            set { SetProperty(ref description, value); }
        }
        private string description = string.Empty;

        public string DateOfBirth
        {
            get => dateOfBirth;
            set { SetProperty(ref dateOfBirth, value); }
        }
        private string dateOfBirth = string.Empty;

        public string Deceased
        {
            get => deceased;
            set { SetProperty(ref deceased, value); }
        }
        private string deceased = string.Empty;

        public string ProfilePictureFileId
        {
            get => profilePictureFileId;
            set { SetProperty(ref profilePictureFileId, value); }
        }
        private string profilePictureFileId = string.Empty;

        public string SexSelection
        {
            get => sex;
            set { SetProperty(ref sex, value); }
        }
        private string sex = Sex.NotApplicable.ToString();

        public List<string> SexValues { get; } = Enum.GetNames(typeof(Sex)).ToList();

        public ICommand SaveCommand => saveCommand ??= new CommandHandler(Save);
        private ICommand saveCommand;

        public AddPersonViewModel(int? personId = null)
        {
            this.personId = personId;

            Title = personId.HasValue ? "Edit Person" : "Add Person";

            if (personId.HasValue)
            {
                var personModel = Utils.DbAccess.GetPersonById(personId.Value);
                Firstname = personModel.Firstname;
                Lastname = personModel.Lastname;
                Description = personModel.Description;
                DateOfBirth = personModel.DateOfBirth;
                Deceased = personModel.Deceased;
                ProfilePictureFileId = personModel.ProfileFileId == null ? string.Empty : personModel.ProfileFileId.Value.ToString();
                SexSelection = personModel.Sex.ToString();
            }
        }

        public void Save()
        {
            int? newProfileFileId = null;
            if (!string.IsNullOrEmpty(profilePictureFileId))
            {
                if (!int.TryParse(profilePictureFileId, out var value))
                {
                    Utils.ShowErrorDialog("Given profile picture file id format not valid");
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
                    Utils.DbAccess.UpdatePerson(person);
                }
                else
                {
                    Utils.DbAccess.InsertPerson(person);
                }
            }
            catch (DataValidationException e)
            {
                Utils.ShowErrorDialog(e.Message);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FileDB2Interface;
using FileDB2Interface.Exceptions;
using FileDB2Interface.Model;

namespace FileDB2Browser.ViewModel
{
    public class AddPersonViewModel : ViewModelBase
    {
        private readonly int personId;

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

        public string ProfilePictureFileId
        {
            get => profilePictureFileId;
            set { SetProperty(ref profilePictureFileId, value); }
        }
        private string profilePictureFileId = string.Empty;

        public ICommand AddPersonCommand
        {
            get
            {
                return addPersonCommand ??= new CommandHandler(AddPerson);
            }
        }
        private ICommand addPersonCommand;

        public AddPersonViewModel(int personId = -1)
        {
            this.personId = personId;

            Title = personId == -1 ? "Add Person" : "Edit Person";

            if (personId != -1)
            {
                var personModel = Utils.FileDB2Handle.GetPersonById(personId);
                Firstname = personModel.firstname;
                Lastname = personModel.lastname;
                Description = personModel.description;
                DateOfBirth = personModel.dateofbirth;
                ProfilePictureFileId = personModel.profilefileid == null ? string.Empty : personModel.profilefileid.Value.ToString();
            }
        }

        public void AddPerson(object parameter)
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

            try
            {
                // TODO: make gui dropdown menu
                Sex sex = Sex.NotApplicable;

                if (personId == -1)
                {
                    Utils.FileDB2Handle.InsertPerson(firstname, lastname, newDescription, newDateOfBirth, newProfileFileId, sex);
                }
                else
                {
                    Utils.FileDB2Handle.UpdatePerson(personId, firstname, lastname, newDescription, newDateOfBirth, newProfileFileId, sex);
                }
            }
            catch (FileDB2DataValidationException e)
            {
                Utils.ShowErrorDialog(e.Message);
            }
        }
    }
}

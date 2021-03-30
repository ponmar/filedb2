using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FileDB2Interface;
using FileDB2Interface.Exceptions;

namespace FileDB2Browser.ViewModel
{
    public class AddPersonViewModel : ViewModelBase
    {
        private readonly FileDB2Handle fileDB2Handle;
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

        public AddPersonViewModel(FileDB2Handle fileDB2Handle, int personId = -1)
        {
            this.fileDB2Handle = fileDB2Handle;
            this.personId = personId;

            Title = personId == -1 ? "Add Person" : "Edit Person";

            if (personId != -1)
            {
                var personModel = fileDB2Handle.GetPersonById(personId);
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
                if (personId == -1)
                {
                    fileDB2Handle.InsertPerson(firstname, lastname, newDescription, newDateOfBirth, newProfileFileId);
                }
                else
                {
                    // TODO: update all in one transaction?
                    fileDB2Handle.UpdatePersonFirstname(personId, firstname);
                    fileDB2Handle.UpdatePersonLastname(personId, lastname);
                    fileDB2Handle.UpdatePersonDescription(personId, newDescription);
                    fileDB2Handle.UpdatePersonDateOfBirth(personId, newDateOfBirth);
                    fileDB2Handle.UpdatePersonProfileFileId(personId, newProfileFileId);
                }
            }
            catch (FileDB2DataValidationException e)
            {
                Utils.ShowErrorDialog(e.Message);
            }
        }
    }
}

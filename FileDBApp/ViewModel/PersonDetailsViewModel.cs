using FileDBApp.Model;

namespace FileDBApp.ViewModel
{
    [QueryProperty("PersonModel", "PersonModel")]
    public class PersonDetailsViewModel : ViewModelBase
    {
        public PersonModel PersonModel
        {
            get => personModel;
            set
            {
                personModel = value;

                Title = $"{personModel.Firstname} {personModel.Lastname}";
                //var dateOfBirth = Utils.ParsePersonsDateOfBirth(personModel.DateOfBirth);
                //Birthday = dateOfBirth.ToString("d MMMM");
                //DaysLeft = Utils.GetDaysToNextBirthday(dateOfBirth);
                //Age = Utils.GetYearsAgo(DateTime.Now, dateOfBirth);
                Description = personModel.Description ?? "No description available";
            }
        }
        private PersonModel personModel;

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }
        private string title;

        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }
        private string description;

        public PersonDetailsViewModel()
        {
        }
    }
}

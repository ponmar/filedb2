namespace FileDBApp.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        public string PersonsJson
        {
            get => personsJson;
            set => SetProperty(ref personsJson, value);
        }
        private string personsJson;

        public Command ImportPersonsCommand { get; }

        public SettingsViewModel()
        {
            ImportPersonsCommand = new Command(() => ImportPersons());
        }

        private void ImportPersons()
        {

        }
    }
}

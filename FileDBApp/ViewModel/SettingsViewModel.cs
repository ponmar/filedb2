using FileDBApp.Model;
using FileDBApp.Services;
using Newtonsoft.Json;
using System.Diagnostics;

namespace FileDBApp.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        public string DataJson
        {
            get => dataJson;
            set
            {
                if (SetProperty(ref dataJson, value))
                {
                    OnPropertyChanged(nameof(ImportPossible));
                }
            }
        }
        private string dataJson;

        public bool ImportPossible => !string.IsNullOrEmpty(dataJson);

        public Command ImportCommand { get; }

        public SettingsViewModel()
        {
            ImportCommand = new Command(async () => await ImportAsync());
        }

        private async Task ImportAsync()
        {
            try
            {
                // Make sure json can be deserialized before saving data to file
                _ = JsonConvert.DeserializeObject<ExportedDatabaseFileFormat>(DataJson);
                await File.WriteAllTextAsync(PersonService.DataFilePath, DataJson);
                DataJson = string.Empty;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                await Shell.Current.DisplayAlert("Error!", $"Unable to import data: {e.Message}", "OK");
            }
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBApp.Model;
using FileDBApp.Services;
using Newtonsoft.Json;
using System.Diagnostics;

namespace FileDBApp.ViewModel;

public partial class SettingsViewModel : ObservableObject
{
    [RelayCommand]
    private async Task ImportAsync()
    {
        var customFileType = new FilePickerFileType(
            new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "application/json" } },
                { DevicePlatform.WinUI, new[] { ".json" } },
            });

        PickOptions options = new()
        {
            PickerTitle = "Please select a JSON file",
            FileTypes = customFileType,
        };

        var result = await FilePicker.Default.PickAsync(options);
        if (result != null)
        {
            try
            {
                // Make sure json can be deserialized before saving data to file
                var json = File.ReadAllText(result.FullPath);
                _ = JsonConvert.DeserializeObject<ExportedDatabaseFileFormat>(json);

                await File.WriteAllTextAsync(PersonService.DataFilePath, json);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                await Shell.Current.DisplayAlert("Error!", $"Unable to import data: {e.Message}", "OK");
            }
        }
    }

    [RelayCommand]
    private async Task ClearAsync()
    {
        try
        {
            if (File.Exists(PersonService.DataFilePath))
            {
                File.Delete(PersonService.DataFilePath);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            await Shell.Current.DisplayAlert("Error!", $"Unable to clear data: {e.Message}", "OK");
        }
    }
}

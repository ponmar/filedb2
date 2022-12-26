using CommunityToolkit.Mvvm.ComponentModel;
using FileDBInterface.Model;

namespace FileDBApp.ViewModel;

[QueryProperty("PersonModel", "PersonModel")]
public partial class PersonDetailsViewModel : ObservableObject
{
    public PersonModel PersonModel
    {
        get => personModel;
        set
        {
            personModel = value;
            Title = $"{personModel.Firstname} {personModel.Lastname}";
            Description = personModel.Description ?? "No description available";
        }
    }
    private PersonModel personModel;

    [ObservableProperty]
    string title;

    [ObservableProperty]
    string description;
}

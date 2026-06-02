using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Infrastructure;
using FileDB.Model;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public record LocationForSearch(int Id, string Name)
{
    public override string ToString() => Name;
}

public partial class LocationViewModel : ObservableValidator, IFilterViewModel
{
    [ObservableProperty]
    public partial ObservableCollection<LocationForSearch> Locations { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<LocationForSearch> LocationsWithPosition { get; set; } = [];

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    public partial LocationForSearch? SelectedLocation { get; set; }

    [ObservableProperty]
    public partial bool Negate { get; set; }

    private readonly ILocationsRepository locationsRepository;
    private readonly IFileSelector fileSelector;
    private readonly IDatabaseAccessProvider databaseAccessProvider;

    public LocationViewModel(ILocationsRepository locationsRepository, IFileSelector fileSelector, IDatabaseAccessProvider databaseAccessProvider)
    {
        this.locationsRepository = locationsRepository;
        this.fileSelector = fileSelector;
        this.databaseAccessProvider = databaseAccessProvider;
        ReloadLocations();
        this.RegisterForEvent<LocationsUpdated>((x) => ReloadLocations());
        TrySelectLocationFromSelectedFile();

        ValidateAllProperties();
    }

    private void TrySelectLocationFromSelectedFile()
    {
        if (fileSelector.SelectedFile is not null)
        {
            var locationsInFile = databaseAccessProvider.DbAccess.GetLocationsFromFile(fileSelector.SelectedFile.Id);
            if (locationsInFile.Any())
            {
                var firstLocationFromFile = locationsInFile.First();
                SelectedLocation = Locations.First(x => x.Id == firstLocationFromFile.Id);
            }
        }
    }

    private void ReloadLocations()
    {
        Locations.Clear();
        LocationsWithPosition.Clear();
        foreach (var location in locationsRepository.Locations)
        {
            Locations.Add(new(location.Id, location.Name));
            if (location.Position is not null)
            {
                LocationsWithPosition.Add(new(location.Id, location.Name));
            }
        }
    }

    public IEnumerable<FileModel> ApplyFilter(IDatabaseAccess dbAccess)
    {
        return Negate ?
            dbAccess.SearchFilesWithoutLocation(SelectedLocation!.Id) :
            dbAccess.SearchFilesWithLocations([SelectedLocation!.Id]);
    }
}

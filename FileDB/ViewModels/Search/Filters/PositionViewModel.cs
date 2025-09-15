using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Model;
using FileDB.Validators;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class PositionViewModel : ObservableValidator, IFilterViewModel
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    [TextIncludesLatitudeAndLongitude()]
    private string positionText = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    [Range(1, 10000, ErrorMessage = "Radius must be between 1 and 10000 meters")]
    private int radius = 500;

    public ObservableCollection<LocationForSearch> LocationsWithPosition { get; } = [];

    [ObservableProperty]
    private LocationForSearch? selectedLocationsWithPosition = null;

    partial void OnSelectedLocationsWithPositionChanged(LocationForSearch? value)
    {
        if (value is not null)
        {
            var location = databaseAccessProvider.DbAccess.GetLocationById(value.Id);
            PositionText = location.Position!;
        }
    }

    [ObservableProperty]
    private bool currentFileHasPosition;

    private readonly ILocationsRepository locationsRepository;
    private readonly IDatabaseAccessProvider databaseAccessProvider;
    private readonly IFileSelector fileSelector;

    public PositionViewModel(ILocationsRepository locationsRepository, IDatabaseAccessProvider databaseAccessProvider, IFileSelector fileSelector)
    {
        this.locationsRepository = locationsRepository;
        this.databaseAccessProvider = databaseAccessProvider;
        this.fileSelector = fileSelector;
        ReloadLocations();
        this.RegisterForEvent<LocationsUpdated>(x => ReloadLocations());
        this.RegisterForEvent<FileSelectionChanged>(x => CurrentFileHasPosition = fileSelector.SelectedFile?.Position is not null);

        ValidateAllProperties();
    }

    private void ReloadLocations()
    {
        LocationsWithPosition.Clear();
        foreach (var location in locationsRepository.Locations)
        {
            var locationToUpdate = new LocationForSearch(location.Id, location.Name);
            if (location.Position is not null)
            {
                LocationsWithPosition.Add(locationToUpdate);
            }
        }
    }

    [RelayCommand]
    private void UsePositionFromCurrentFile()
    {
        var filePosition = fileSelector.SelectedFile?.Position;
        if (filePosition is not null)
        {
            PositionText = filePosition;
        }
    }

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        (var lat, var lon) = TextIncludesLatitudeAndLongitudeAttribute.ParsePositionFromTextOrUrl(PositionText)!.Value;

        var result = dbAccess.SearchFilesNearGpsPosition(lat, lon, Radius).ToList();
        var nearLocations = dbAccess.SearchLocationsNearGpsPosition(lat, lon, Radius);
        result.AddRange(dbAccess.SearchFilesWithLocations(nearLocations.Select(x => x.Id)));
        return result;
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Infrastructure;
using FileDB.Model;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using FileDBInterface.Utils;

namespace FileDB.ViewModels.Search.Filters;

public partial class PersonTimelineViewModel : ObservableValidator, IFilterViewModel
{
    [ObservableProperty]
    public partial ObservableCollection<PersonForSearch> Persons { get; set; } = [];

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    public partial PersonForSearch? SelectedPerson { get; set; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    [Range(0, 200, ErrorMessage = "Must be between 0 and 200")]
    public partial int AgeFrom { get; set; } = 0;

    partial void OnAgeFromChanged(int value)
    {
        if (value > AgeTo)
            AgeTo = value;
    }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    [Range(0, 200, ErrorMessage = "Must be between 0 and 200")]
    public partial int AgeTo { get; set; } = 100;

    partial void OnAgeToChanged(int value)
    {
        if (value < AgeFrom)
            AgeFrom = value;
    }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    [Range(1, 100, ErrorMessage = "Must be between 1 and 100")]
    public partial int FilesPerYear { get; set; } = 1;

    private readonly IPersonsRepository personsRepository;

    public PersonTimelineViewModel(IPersonsRepository personsRepository)
    {
        this.personsRepository = personsRepository;
        ReloadPersons();
        this.RegisterForEvent<PersonsUpdated>(_ => ReloadPersons());
        ValidateAllProperties();
    }

    private void ReloadPersons()
    {
        Persons.Clear();
        foreach (var person in personsRepository.Persons)
        {
            Persons.Add(new(person.Id, person.FullName));
        }
    }

    public IEnumerable<FileModel> ApplyFilter(IDatabaseAccess dbAccess)
    {
        var personId = SelectedPerson!.Id;
        var personModel = personsRepository.Persons.FirstOrDefault(p => p.Id == personId);
        if (personModel?.DateOfBirth is null)
            return [];

        var dateOfBirth = DatabaseParsing.ParsePersonDateOfBirth(personModel.DateOfBirth);

        var filesForPerson = dbAccess.SearchFilesWithPersons([personId]);

        var result = new List<FileModel>();
        var byAge = new Dictionary<int, List<FileModel>>();

        foreach (var file in filesForPerson)
        {
            var fileDatetime = DatabaseParsing.ParseFilesDatetime(file.Datetime);
            if (fileDatetime is null)
                continue;

            int age = TimeUtils.GetAgeInYears(fileDatetime.Value, dateOfBirth);
            if (age < AgeFrom || age > AgeTo)
                continue;

            if (!byAge.TryGetValue(age, out var bucket))
            {
                bucket = [];
                byAge[age] = bucket;
            }
            bucket.Add(file);
        }

        var candidateFiles = byAge.Values.SelectMany(b => b).ToList();
        var personCountById = dbAccess.GetPersonCountsFromFiles(candidateFiles.Select(f => f.Id));

        foreach (var bucket in byAge.Values)
            result.AddRange(bucket.OrderBy(f => personCountById.GetValueOrDefault(f.Id, 0)).Take(FilesPerYear));

        return result;
    }
}

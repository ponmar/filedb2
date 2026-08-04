using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using FileDBInterface.Utils;

namespace FileDB.ViewModels.Search.Filters;

public partial class PersonAgeViewModel : ObservableValidator, IFilterViewModel
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    [Range(0, 200, ErrorMessage = "Must be between 0 and 200")]
    public partial int PersonAgeFrom { get; set; } = 1;

    partial void OnPersonAgeFromChanged(int value)
    {
        if (value > PersonAgeTo)
        {
            PersonAgeTo = value;
        }
    }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    [Range(0, 200, ErrorMessage = "Must be between 0 and 200")]
    public partial int PersonAgeTo { get; set; } = 100;

    partial void OnPersonAgeToChanged(int value)
    {
        if (value < PersonAgeFrom)
        {
            PersonAgeFrom = value;
        }
    }

    public PersonAgeViewModel()
    {
        ValidateAllProperties();
    }

    public IEnumerable<FileModel> ApplyFilter(IDatabaseAccess dbAccess)
    {
        var result = new List<FileModel>();
        var personsWithAge = dbAccess.GetPersons().Where(p => p.DateOfBirth is not null);

        foreach (var person in personsWithAge)
        {
            var dateOfBirth = DatabaseParsing.ParsePersonDateOfBirth(person.DateOfBirth!);
            foreach (var file in dbAccess.SearchFilesWithPersons([person.Id]))
            {
                var fileDatetime = DatabaseParsing.ParseFilesDatetime(file.Datetime);
                if (fileDatetime is not null)
                {
                    int personAgeInFile = TimeUtils.GetAgeInYears(fileDatetime.Value, dateOfBirth);
                    if (personAgeInFile >= PersonAgeFrom && personAgeInFile <= PersonAgeTo)
                    {
                        result.Add(file);
                    }
                }
            }
        }

        return result.DistinctBy(x => x.Id);
    }
}

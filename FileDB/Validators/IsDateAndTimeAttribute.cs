using System.ComponentModel.DataAnnotations;
using FileDBInterface.Validators;

namespace FileDB.Validators;

public class IsDateAndTimeAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value is string s && (s == string.Empty || FileModelValidator.ValidateDatetime(s));
    }
}

using System.ComponentModel.DataAnnotations;

namespace FileDB.Validators;

public class IsFileIdsTextAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value is string s && Utils.TryParseFileIds(s, out var _);
    }
}

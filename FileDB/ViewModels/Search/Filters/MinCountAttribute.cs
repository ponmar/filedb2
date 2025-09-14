using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace FileDB.ViewModels.Search.Filters;

public class MinCountAttribute : ValidationAttribute
{
    public int Min { get; }

    public MinCountAttribute(int min)
    {
        Min = min;
    }

    public override bool IsValid(object? value)
    {
        if (value is ICollection collection)
        {
            return collection.Count >= Min;
        }
        return false;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"The number of items in {name} must be at least {Min}.";
    }
}

using System;
using System.Linq;
using FluentValidation.Results;

namespace FileDBInterface.Exceptions;

public class DataValidationException : Exception
{
    public DataValidationException(string message) : base(message)
    {
    }

    public DataValidationException(ValidationResult result) : base(string.Join("\n", result.Errors.Select(x => x.ErrorMessage)))
    {
    }
}

using System.Collections.Generic;
using FileDBInterface.Exceptions;
using FluentValidation.Results;
using Xunit;

namespace FileDBInterfaceTests.Exceptions;

public class DataValidationExceptionTests
{
    [Fact]
    public void Message()
    {
        var failures = new List<ValidationFailure>()
        {
            new ValidationFailure("Property1", "Error1"),
            new ValidationFailure("Property2", "Error2"),
        };
        var result = new ValidationResult(failures);

        var e = new DataValidationException(result);
        Assert.Equal("Error1\nError2", e.Message);
    }
}

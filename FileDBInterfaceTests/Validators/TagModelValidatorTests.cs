using FileDBInterface.Model;
using FileDBInterface.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace FileDBInterfaceTests.Validators;

public class TagModelValidatorTests
{
    private readonly TagModelValidator validator = new();

    [Fact]
    public void Validate_Valid()
    {
        var model = new TagModel() { Id = 1, Name = "name" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_IdIsNegative_Error()
    {
        var model = new TagModel() { Id = -1, Name = "name" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_NameIsEmpty_Error()
    {
        var model = new TagModel() { Id = 1, Name = string.Empty };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameIsTooShort_Error()
    {
        var model = new TagModel() { Id = 1, Name = "x" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}

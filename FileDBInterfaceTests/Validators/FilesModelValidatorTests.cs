using FileDBInterface.Model;
using FileDBInterface.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace FileDBInterfaceTests.Validators;

public class FileModelValidatorTests
{
    private readonly FileModelValidator validator = new();

    [Fact]
    public void Validate_NoOptionalData_Valid()
    {
        var model = new FileModel() { Id = 1, Path = "name", Description = null, Datetime = null, Position = null };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_AllData_Valid()
    {
        var model = new FileModel() { Id = 1, Path = "name", Description = "description", Datetime = "2010-07-20T20:50:10", Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_IdIsNegative_Error()
    {
        var model = new FileModel() { Id = -1, Path = "my/path/file.jpg", Description = "description", Datetime = "2010-07-20T20:50:10", Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_PathIsNull_Error()
    {
        var model = new FileModel() { Id = -1, Path = null!, Description = "description", Datetime = "2010-07-20T20:50:10", Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Path);
    }

    [Fact]
    public void Validate_DescriptonIsEmpty_Error()
    {
        var model = new FileModel() { Id = -1, Path = "my/path/file.jpg", Description = string.Empty, Datetime = "2010-07-20T20:50:10", Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_DescriptonHasInvalidLineEnding_Error()
    {
        var model = new FileModel() { Id = -1, Path = "my/path/file.jpg", Description = "First line\r\nSecond line", Datetime = "2010-07-20T20:50:10", Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_DescriptonHasValidLineEnding_Error()
    {
        var model = new FileModel() { Id = -1, Path = "my/path/file.jpg", Description = "First line\nSecond line", Datetime = "2010-07-20T20:50:10", Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_DatetimeIsNull()
    {
        var model = new FileModel() { Id = -1, Path = "my/path/file.jpg", Description = string.Empty, Datetime = null, Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Datetime);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(4, true)]
    [InlineData(5, true)]
    [InlineData(6, true)]
    [InlineData(7, true)]
    [InlineData(8, true)]
    [InlineData(9, false)]
    public void Validate_Orientation(int? orientation, bool isValid)
    {
        var model = new FileModel() { Id = -1, Path = "my/path/file.jpg", Description = string.Empty, Datetime = "2010-03-05", Position = "15.1 64.10", Orientation = orientation };
        var result = validator.TestValidate(model);
        if (isValid)
        {
            result.ShouldNotHaveValidationErrorFor(x => x.Orientation);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(x => x.Orientation);
        }
    }

    [Fact]
    public void Validate_DatetimeIsEmpty()
    {
        var model = new FileModel() { Id = -1, Path = "my/path/file.jpg", Description = string.Empty, Datetime = string.Empty, Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Datetime);
    }

    [Fact]
    public void Validate_DatetimeIsYear()
    {
        var model = new FileModel() { Id = -1, Path = "my/path/file.jpg", Description = string.Empty, Datetime = "2010", Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Datetime);
    }

    [Fact]
    public void Validate_DatetimeIsDate()
    {
        var model = new FileModel() { Id = -1, Path = "my/path/file.jpg", Description = string.Empty, Datetime = "2010-03-05", Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Datetime);
    }

    [Fact]
    public void Validate_DatetimeIsDateAndTime()
    {
        var model = new FileModel() { Id = -1, Path = "my/path/file.jpg", Description = string.Empty, Datetime = "2010-03-05T20:50:10", Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Datetime);
    }

    // TODO: re-use gps position test from location?
}

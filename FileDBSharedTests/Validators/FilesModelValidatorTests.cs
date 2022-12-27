using FileDBShared.Model;
using FileDBShared.Validators;
using FluentValidation.TestHelper;

namespace FileDBSharedTests.Validators;

[TestClass]
public class FilesModelValidatorTests
{
    private FilesModelValidator validator;

    [TestInitialize]
    public void Initialize()
    {
        validator = new();
    }

    [TestMethod]
    public void Validate_NoOptionalData_Valid()
    {
        var model = new FilesModel() { Id = 1, Path = "name", Description = null, Datetime = null, Position = null };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Validate_AllData_Valid()
    {
        var model = new FilesModel() { Id = 1, Path = "name", Description = "description", Datetime = "2010-07-20T20:50:10", Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Validate_IdIsNegative_Error()
    {
        var model = new FilesModel() { Id = -1, Path = "my/path/file.jpg", Description = "description", Datetime = "2010-07-20T20:50:10", Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [TestMethod]
    public void Validate_PathIsNull_Error()
    {
        var model = new FilesModel() { Id = -1, Path = null, Description = "description", Datetime = "2010-07-20T20:50:10", Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Path);
    }

    [TestMethod]
    public void Validate_DescriptonIsEmpty_Error()
    {
        var model = new FilesModel() { Id = -1, Path = "my/path/file.jpg", Description = string.Empty, Datetime = "2010-07-20T20:50:10", Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [TestMethod]
    public void Validate_DescriptonHasInvalidLineEnding_Error()
    {
        var model = new FilesModel() { Id = -1, Path = "my/path/file.jpg", Description = "First line\r\nSecond line", Datetime = "2010-07-20T20:50:10", Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [TestMethod]
    public void Validate_DescriptonHasValidLineEnding_Error()
    {
        var model = new FilesModel() { Id = -1, Path = "my/path/file.jpg", Description = "First line\nSecond line", Datetime = "2010-07-20T20:50:10", Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [TestMethod]
    public void Validate_DatetimeIsNull()
    {
        var model = new FilesModel() { Id = -1, Path = "my/path/file.jpg", Description = string.Empty, Datetime = null, Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Datetime);
    }

    [DataTestMethod]
    [DataRow(null, true)]
    [DataRow(0, false)]
    [DataRow(1, true)]
    [DataRow(2, true)]
    [DataRow(3, true)]
    [DataRow(4, true)]
    [DataRow(5, true)]
    [DataRow(6, true)]
    [DataRow(7, true)]
    [DataRow(8, true)]
    [DataRow(9, false)]
    public void Validate_Orientation(int? orientation, bool isValid)
    {
        var model = new FilesModel() { Id = -1, Path = "my/path/file.jpg", Description = string.Empty, Datetime = "2010-03-05", Position = "15.1 64.10", Orientation = orientation };
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

    [TestMethod]
    public void Validate_DatetimeIsEmpty()
    {
        var model = new FilesModel() { Id = -1, Path = "my/path/file.jpg", Description = string.Empty, Datetime = string.Empty, Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Datetime);
    }

    [TestMethod]
    public void Validate_DatetimeIsYear()
    {
        var model = new FilesModel() { Id = -1, Path = "my/path/file.jpg", Description = string.Empty, Datetime = "2010", Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Datetime);
    }

    [TestMethod]
    public void Validate_DatetimeIsDate()
    {
        var model = new FilesModel() { Id = -1, Path = "my/path/file.jpg", Description = string.Empty, Datetime = "2010-03-05", Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Datetime);
    }

    [TestMethod]
    public void Validate_DatetimeIsDateAndTime()
    {
        var model = new FilesModel() { Id = -1, Path = "my/path/file.jpg", Description = string.Empty, Datetime = "2010-03-05T20:50:10", Position = "15.1 64.10" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Datetime);
    }

    // TODO: re-use gps position test from location?
}

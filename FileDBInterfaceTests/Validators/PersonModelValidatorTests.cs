using FileDBInterface.Model;
using FileDBInterface.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace FileDBInterfaceTests.Validators;

public class PersonModelValidatorTests
{
    private readonly PersonModelValidator validator = new();

    [Fact]
    public void Validate_NoOptionalData_Valid()
    {
        var model = new PersonModel()
        {
            Id = 1,
            Firstname = "firstname",
            Lastname = "lastname",
            Description = null,
            DateOfBirth = null,
            Deceased = null,
            ProfileFileId = null,
            Sex = Sex.NotKnown,
        };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_AllData_Valid()
    {
        var model = new PersonModel()
        {
            Id = 1,
            Firstname = "firstname",
            Lastname = "lastname",
            Description = "description",
            DateOfBirth = "2000-01-01",
            Deceased = "2000-01-02",
            ProfileFileId = 10,
            Sex = Sex.NotApplicable,
        };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_IdIsNegative_Error()
    {
        var model = new PersonModel()
        {
            Id = -1,
            Firstname = "firstname",
            Lastname = "lastname",
            Description = null,
            DateOfBirth = null,
            Deceased = null,
            ProfileFileId = null,
            Sex = Sex.NotKnown,
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_FirstnameIsEmpty_Error()
    {
        var model = new PersonModel()
        {
            Id = 1,
            Firstname = string.Empty,
            Lastname = "lastname",
            Description = null,
            DateOfBirth = null,
            Deceased = null,
            ProfileFileId = null,
            Sex = Sex.NotKnown,
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Firstname);
    }

    [Fact]
    public void Validate_LastnameIsEmpty_Error()
    {
        var model = new PersonModel()
        {
            Id = 1,
            Firstname = "firstname",
            Lastname = string.Empty,
            Description = null,
            DateOfBirth = null,
            Deceased = null,
            ProfileFileId = null,
            Sex = Sex.NotKnown,
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Lastname);
    }

    [Fact]
    public void Validate_FirstnameIsTooShort_Error()
    {
        var model = new PersonModel()
        {
            Id = 1,
            Firstname = "x",
            Lastname = "lastname",
            Description = null,
            DateOfBirth = null,
            Deceased = null,
            ProfileFileId = null,
            Sex = Sex.NotKnown,
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Firstname);
    }

    [Fact]
    public void Validate_LastnameIsTooShort_Error()
    {
        var model = new PersonModel()
        {
            Id = 1,
            Firstname = "firstname",
            Lastname = "x",
            Description = null,
            DateOfBirth = null,
            Deceased = null,
            ProfileFileId = null,
            Sex = Sex.NotKnown,
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Lastname);
    }

    [Fact]
    public void Validate_DescriptionIsEmpty_Error()
    {
        var model = new PersonModel()
        {
            Id = 1,
            Firstname = "firstname",
            Lastname = "lastname",
            Description = string.Empty,
            DateOfBirth = null,
            Deceased = null,
            ProfileFileId = null,
            Sex = Sex.NotKnown,
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_DateOfBirthInvalid_Error()
    {
        var model = new PersonModel()
        {
            Id = 1,
            Firstname = "firstname",
            Lastname = "lastname",
            Description = null,
            DateOfBirth = "invalid",
            Deceased = null,
            ProfileFileId = null,
            Sex = Sex.NotKnown,
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Validate_DeceasedInvalid_Error()
    {
        var model = new PersonModel()
        {
            Id = 1,
            Firstname = "firstname",
            Lastname = "lastname",
            Description = null,
            DateOfBirth = null,
            Deceased = "invalid",
            ProfileFileId = null,
            Sex = Sex.NotKnown,
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Deceased);
    }

    [Fact]
    public void Validate_ProfileFileIdInvalid_Error()
    {
        var model = new PersonModel()
        {
            Id = 1,
            Firstname = "firstname",
            Lastname = "lastname",
            Description = null,
            DateOfBirth = null,
            Deceased = "invalid",
            ProfileFileId = -10,
            Sex = Sex.NotKnown,
        };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ProfileFileId);
    }
}

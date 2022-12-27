using FileDBShared.Model;
using FileDBShared.Validators;
using FluentValidation.TestHelper;

namespace FileDBSharedTests.Validators;

[TestClass]
public class PersonModelValidatorTests
{
    private PersonModelValidator validator;

    [TestInitialize]
    public void Initialize()
    {
        validator = new();
    }

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

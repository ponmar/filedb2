using FileDBShared.Model;
using FileDBShared.Validators;
using FluentValidation.TestHelper;

namespace FileDBSharedTests.Validators;

[TestClass]
public class TagModelValidatorTests
{
    private TagModelValidator validator;

    [TestInitialize]
    public void Initialize()
    {
        validator = new();
    }

    [TestMethod]
    public void Validate_Valid()
    {
        var model = new TagModel() { Id = 1, Name = "name" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Validate_IdIsNegative_Error()
    {
        var model = new TagModel() { Id = -1, Name = "name" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [TestMethod]
    public void Validate_NameIsEmpty_Error()
    {
        var model = new TagModel() { Id = 1, Name = string.Empty };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [TestMethod]
    public void Validate_NameIsTooShort_Error()
    {
        var model = new TagModel() { Id = 1, Name = "x" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}

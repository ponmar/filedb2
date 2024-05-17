using FileDBInterface.Model;
using FileDBInterface.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace FileDBInterfaceTests.Validators;

public class LocationModelValidatorTests
{
    private readonly LocationModelValidator validator = new();

    [Fact]
    public void Validate_NoOptionalData()
    {
        var model = new LocationModel() { Id = 1, Name = "name", Description = null, Position = null };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_AllOptionalData()
    {
        var model = new LocationModel() { Id = 1, Name = "name", Description = "description", Position = "5.10 6.11" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_IdIsNegative_Error()
    {
        var model = new LocationModel() { Id = -1, Name = "name", Description = "description", Position = "5.10 6.11" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_NameIsEmpty_Error()
    {
        var model = new LocationModel() { Id = 1, Name = string.Empty, Description = "description", Position = "5.10 6.11" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_DescriptionIsEmpty_Error()
    {
        var model = new LocationModel() { Id = 1, Name = "x", Description = string.Empty, Position = "5.10 6.11" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_PositionIsNoPosition_Error()
    {
        var model = new LocationModel() { Id = 1, Name = "x", Description = "description", Position = "test" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Position);
    }

    [Fact]
    public void Validate_PositionIsNotInvariantCulture_Error()
    {
        var model = new LocationModel() { Id = 1, Name = "x", Description = "description", Position = "5,10 6,11" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Position);
    }

    [Fact]
    public void Validate_PositionNoLongitude_Error()
    {
        var model = new LocationModel() { Id = 1, Name = "x", Description = "description", Position = "5.10 " };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Position);
    }
}

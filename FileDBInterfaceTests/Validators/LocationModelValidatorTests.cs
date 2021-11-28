using FileDBInterface.Model;
using FileDBInterface.Validators;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBInterfaceTests.Validators
{
    [TestClass]
    public class LocationModelValidatorTests
    {
        private LocationModelValidator validator;

        [TestInitialize]
        public void Initialize()
        {
            validator = new();
        }

        [TestMethod]
        public void Validate_NoOptionalData()
        {
            var model = new LocationModel() { Id = 1, Name = "name", Description = null, Position = null };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod]
        public void Validate_AllOptionalData()
        {
            var model = new LocationModel() { Id = 1, Name = "name", Description = "description", Position = "5.10 6.11" };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod]
        public void Validate_IdIsNegative_Error()
        {
            var model = new LocationModel() { Id = -1, Name = "name", Description = "description", Position = "5.10 6.11" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [TestMethod]
        public void Validate_NameIsEmpty_Error()
        {
            var model = new LocationModel() { Id = 1, Name = string.Empty, Description = "description", Position = "5.10 6.11" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [TestMethod]
        public void Validate_DescriptionIsEmpty_Error()
        {
            var model = new LocationModel() { Id = 1, Name = "x", Description = string.Empty, Position = "5.10 6.11" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [TestMethod]
        public void Validate_PositionIsNoPosition_Error()
        {
            var model = new LocationModel() { Id = 1, Name = "x", Description = "description", Position = "test" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Position);
        }

        [TestMethod]
        public void Validate_PositionIsNotInvariantCulture_Error()
        {
            var model = new LocationModel() { Id = 1, Name = "x", Description = "description", Position = "5,10 6,11" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Position);
        }

        [TestMethod]
        public void Validate_PositionNoLongitude_Error()
        {
            var model = new LocationModel() { Id = 1, Name = "x", Description = "description", Position = "5.10 " };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Position);
        }
    }
}

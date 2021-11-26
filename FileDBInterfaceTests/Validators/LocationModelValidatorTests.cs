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
            var model = new LocationModel() { id = 1, name = "name", description = null, position = null };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod]
        public void Validate_AllOptionalData()
        {
            var model = new LocationModel() { id = 1, name = "name", description = "description", position = "5.10 6.11" };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod]
        public void Validate_IdIsNegative_Error()
        {
            var model = new LocationModel() { id = -1, name = "name", description = "description", position = "5.10 6.11" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.id);
        }

        [TestMethod]
        public void Validate_NameIsEmpty_Error()
        {
            var model = new LocationModel() { id = 1, name = string.Empty, description = "description", position = "5.10 6.11" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.name);
        }

        [TestMethod]
        public void Validate_DescriptionIsEmpty_Error()
        {
            var model = new LocationModel() { id = 1, name = "x", description = string.Empty, position = "5.10 6.11" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.description);
        }

        [TestMethod]
        public void Validate_PositionIsNoPosition_Error()
        {
            var model = new LocationModel() { id = 1, name = "x", description = "description", position = "test" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.position);
        }

        [TestMethod]
        public void Validate_PositionIsNotInvariantCulture_Error()
        {
            var model = new LocationModel() { id = 1, name = "x", description = "description", position = "5,10 6,11" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.position);
        }

        [TestMethod]
        public void Validate_PositionNoLongitude_Error()
        {
            var model = new LocationModel() { id = 1, name = "x", description = "description", position = "5.10 " };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.position);
        }
    }
}

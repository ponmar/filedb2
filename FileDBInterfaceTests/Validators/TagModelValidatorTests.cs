using FileDBInterface.Model;
using FileDBInterface.Validators;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBInterfaceTests.Validators
{
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
            var model = new TagModel() { id = 1, name = "name" };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod]
        public void Validate_IdIsNegative_Error()
        {
            var model = new TagModel() { id = -1, name = "name" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.id);
        }

        [TestMethod]
        public void Validate_NameIsEmpty_Error()
        {
            var model = new TagModel() { id = 1, name = string.Empty };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.name);
        }

        [TestMethod]
        public void Validate_NameIsTooShort_Error()
        {
            var model = new TagModel() { id = 1, name = "x" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.name);
        }
    }
}

using FileDBInterface.Model;
using FileDBInterface.Validators;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBInterfaceTests.Validators
{
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
            var model = new FilesModel() { id = 1, path = "name", description = null, datetime = null, position = null };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod]
        public void Validate_AllData_Valid()
        {
            var model = new FilesModel() { id = 1, path = "name", description = "description", datetime = "2010-07-20T20:50:10", position = "15.1 64.10" };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod]
        public void Validate_IdIsNegative_Error()
        {
            var model = new FilesModel() { id = -1, path = "my/path/file.jpg", description = "description", datetime = "2010-07-20T20:50:10", position = "15.1 64.10" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.id);
        }

        [TestMethod]
        public void Validate_PathIsNull_Error()
        {
            var model = new FilesModel() { id = -1, path = null, description = "description", datetime = "2010-07-20T20:50:10", position = "15.1 64.10" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.path);
        }

        [TestMethod]
        public void Validate_DescriptonIsEmpty_Error()
        {
            var model = new FilesModel() { id = -1, path = "my/path/file.jpg", description = string.Empty, datetime = "2010-07-20T20:50:10", position = "15.1 64.10" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.description);
        }

        [TestMethod]
        public void Validate_DatetimeIsYear_Error()
        {
            var model = new FilesModel() { id = -1, path = "my/path/file.jpg", description = string.Empty, datetime = "2010", position = "15.1 64.10" };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.datetime);
        }

        [TestMethod]
        public void Validate_DatetimeIsDate_Error()
        {
            var model = new FilesModel() { id = -1, path = "my/path/file.jpg", description = string.Empty, datetime = "2010-03-05", position = "15.1 64.10" };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.datetime);
        }

        [TestMethod]
        public void Validate_DatetimeIsDateAndTime_Error()
        {
            var model = new FilesModel() { id = -1, path = "my/path/file.jpg", description = string.Empty, datetime = "2010-03-05T20:50:10", position = "15.1 64.10" };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.datetime);
        }

        // TODO: re-use gps position test from location?
    }
}
